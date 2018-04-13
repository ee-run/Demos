using ReportToolDemo.ReportServiceTool.Commons;
using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Forms;
using ReportToolDemo.ReportServiceTool.Helper;
using ReportToolDemo.ReportServiceTool.WorkFlows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReportToolDemo.ReportServiceTool
{
    public partial class frmMain : Form
    {
        public static bool bServiceStopFlag = false; // 是否停止标记

        private static frmMain frmMainObj = null;
        public delegate void PrintLogDelegate(string msg);
        private static RichTextBox ric = null;
        private static CheckBox ckBox = null;
        List<InfoEntity> infoEntityList = new List<InfoEntity>();  // 配置信息对象集合
        List<TaskEntity> taskEntityList = new List<TaskEntity>(); // 开启的任务集合

        /// <summary>
        /// MSMQ监听任务
        /// </summary>
        private static MSMQWorkFlow MSMQListenTask;

        /// <summary>
        /// 是否被动模式
        /// </summary>
        public static bool isPasv { get; private set; }

        int iFailWaitMins = 5; // 失败后等待的分钟时长

        private int iMaxLength = 2000; // 日志最大显示长度


        public static readonly Lazy<frmMain> ins = new Lazy<frmMain>(() =>
        {
            return GetfrmMainInstance();
        });

        private frmMain()
        {
            InitializeComponent();
            ric = this.rhtbServiceLog;
            ckBox = this.chbShowLog;
        }

        /// <summary>
        /// 获取主窗体的实例
        /// </summary>
        /// <returns></returns>
        private static frmMain GetfrmMainInstance()
        {
            if (frmMainObj == null)
            {
                return new frmMain();
            }
            else
            {
                return frmMainObj;
            }
        }

        /// <summary>
        /// 计时器更新当前时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void curTimer_Tick(object sender, EventArgs e)
        {
            this.lblCurTime.Text = string.Format("{0}：{1}", "当前时间", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenService_Click(object sender, EventArgs e)
        {
            this.rhtbServiceLog.Text = string.Empty;
            taskEntityList.Clear();

            isPasv = this.rbtnPasv.Checked;

            if (!this.chbService_CCK.Checked
                && !chbService_CIQ.Checked
                && !chbService_IRS.Checked
                && !chbDirectService_CIQ.Checked
                && !chbDirectService_CCK.Checked
                && !chbDirectService_IRS.Checked
                && !chbSpeService.Checked
                && !chbService_MSMQ.Checked)
            {
                Common.ShowPromptInfo("未勾选任何服务！");
                return;
            }

            // 数据验证
            if (!CheckValues())
            {
                return;
            }

            // 禁用所有控件（除了关闭）
            SetCurFormAllGBoxEnable(false);

            // 更新INI配置文件
            UpdateINIInfo();

            // 再次加载
            InitData();

            // 初始化信息对象集合
            InitInfoEntityList();
            if (infoEntityList == null)
            {
                PrintLog("界面信息对象集合为空。");
                return;
            }
            CancellationTokenSource cts;
            Dictionary<Task, CancellationTokenSource> dicTask;
            TaskEntity taskEntity;
            // 启动所有服务
            foreach (var infoEntity in infoEntityList)
            {
                if (infoEntity == null)
                {
                    continue;
                }

                cts = new CancellationTokenSource();
                dicTask = new Dictionary<Task, CancellationTokenSource>();
                taskEntity = new Entity.TaskEntity();

                Task task = Task.Factory.StartNew(() =>
                 {
                     StartService(infoEntity, cts.Token);
                 }, cts.Token);

                taskEntity.Task = task;
                taskEntity.CancellationTokenSource = cts;

                taskEntityList.Add(taskEntity);
            }
        }

        /// <summary>
        ///  设置groupBox里的子控件是否可用
        /// </summary>
        /// <param name="bEnabled"></param>
        private void SetGroupBoxControlEnable(GroupBox gBox, bool bEnabled)
        {
            try
            {
                foreach (Control item in gBox.Controls)
                {
                    item.Enabled = bEnabled;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        /// <summary>
        /// 设置当前界面的所有groupbox的子控件是否可用
        /// </summary>
        /// <param name="bEnabled"></param>
        private void SetCurFormAllGBoxEnable(bool bEnabled)
        {
            SetGroupBoxControlEnable(this.groupBox1, bEnabled);
            SetGroupBoxControlEnable(this.groupBox3, bEnabled);
            SetGroupBoxControlEnable(this.groupBox4, bEnabled);
            SetGroupBoxControlEnable(this.groupBox5, bEnabled);
            SetGroupBoxControlEnable(this.groupBox6, bEnabled);
            SetGroupBoxControlEnable(this.groupBox7, bEnabled);
            SetGroupBoxControlEnable(this.groupBox8, bEnabled);
            SetGroupBoxControlEnable(this.groupBox9, bEnabled);
        }

        /// <summary>
        /// 根据启动的服务更新相应的标签信息
        /// </summary>
        private void UpdateINIInfo()
        {
            try
            {
                PrintLog("开始更新配置信息...");

                UpdateBaseInfo();

                if (this.chbService_CIQ.Checked || this.chbDirectService_CIQ.Checked)
                {
                    UpdateCIQInfo();
                }

                if (this.chbService_CCK.Checked || this.chbDirectService_CCK.Checked)
                {
                    UpdateCCKInfo();
                }

                if (this.chbService_IRS.Checked || this.chbDirectService_IRS.Checked)
                {
                    UpdateIRSInfo();
                }

                if (this.chbSpeService.Checked)
                {
                    UpdateSPEInfo();
                }

                if (this.chbService_MSMQ.Checked)
                {
                    UpdateMSMQInfo();
                }

                PrintLog("更新配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新配置信息失败：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 更新MSMQ文件推送配置
        /// </summary>
        private void UpdateMSMQInfo()
        {
            try
            {
                PrintLog("开始更新MSMQ文件推送配置信息...");
                string sPushDic = string.Format("{0}/{1}/{2}", this.txtSuperviseArea.Text.Trim(), "receive", this.txtSuperviseOrg_MSMQ.Text.Trim());

                //报表拉取配置
                BaseConfig.SetNodeValueByName(BaseConfig.StorageDicPath_MSMQ, this.txtStorageDicPath_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullInfoBak_MSMQ, this.txtPullInfoBak_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullDicPath_MSMQ, this.txtPullDicPath_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushMSMQPath_MSMQ, this.txtPushMSMQPath_MSMQ.Text.Trim());

                BaseConfig.SetNodeValueByName(BaseConfig.SuperviseOrg_MSMQ, this.txtSuperviseOrg_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.OriginDicPath_MSMQ, this.txtOriginDicPath_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushInfoBak_MSMQ, this.txtPushInfoBak_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullPath_MSMQ, this.txtPullMSMQPath_MSMQ.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushPath_MSMQ, sPushDic);

                PrintLog("更新MSMQ文件推送配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新MSMQ文件推送配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 更新特殊文件推送配置
        /// </summary>
        private void UpdateSPEInfo()
        {
            try
            {
                PrintLog("开始更新特殊文件推送配置信息...");

                BaseConfig.SetNodeValueByName(BaseConfig.PullPath_SPE, this.txtSpePullDicPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendPath_SPE, this.txtSpeStorageDicPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendBakPath_SPE, this.txtSpeBakDicPath.Text.Trim());

                PrintLog("更新特殊文件推送配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新特殊文件推送配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 基础配置信息
        /// </summary>
        private void UpdateBaseInfo()
        {
            try
            {
                PrintLog("开始更新基础配置信息...");

                BaseConfig.SetNodeValueByName(BaseConfig.FTPIP, this.txtFTPIP.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LoginName, this.txtLoginName.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.Password, this.txtLoginPwd.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.FTPPort, this.txtFTPPort.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.SuperviseArea, this.txtSuperviseArea.Text.Trim());

                PrintLog("更新基础信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新基础配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 更新区内监管配置信息
        /// </summary>
        private void UpdateIRSInfo()
        {
            try
            {
                PrintLog("开始更新区内监管配置信息...");

                string sPushDic = string.Format("{0}/{1}/{2}", this.txtSuperviseArea.Text.Trim(), "receive", this.txtSuperviseOrg_IRS.Text.Trim());

                BaseConfig.SetNodeValueByName(BaseConfig.SuperviseOrg_IRS, this.txtSuperviseOrg_IRS.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullPath_IRS, this.txtPullDicPath_IRS.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendPath_IRS, this.txtStorageDicPath_IRS.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendBakPath_IRS, this.txtPullInfoBakPath_IRS.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushPath_IRS, sPushDic);
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceivePath_IRS, this.txtOriginDicPath_IRS.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceiveBakPath_IRS, this.txtPushInfoBakPath_IRS.Text.Trim());

                PrintLog("更新区内监管配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新区内监管配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 更新国检配置信息
        /// </summary>
        private void UpdateCCKInfo()
        {
            try
            {
                PrintLog("开始更新国检配置信息...");
                string sPushDic = string.Format("{0}/{1}/{2}", this.txtSuperviseArea.Text.Trim(), "receive", this.txtSuperviseOrg_CCK.Text.Trim());

                BaseConfig.SetNodeValueByName(BaseConfig.SuperviseOrg_CCK, this.txtSuperviseOrg_CCK.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullPath_CCK, this.txtPullDicPath_CCK.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendPath_CCK, this.txtStorageDicPath_CCK.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendBakPath_CCK, this.txtPullInfoBakPath_CCK.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushPath_CCK, sPushDic);
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceivePath_CCK, this.txtOriginDicPath_CCK.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceiveBakPath_CCK, this.txtPushInfoBakPath_CCK.Text.Trim());

                PrintLog("更新国检配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新国检配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 更新海关配置信息
        /// </summary>
        private void UpdateCIQInfo()
        {
            try
            {
                PrintLog("开始更新海关配置信息...");
                string sPushDic = string.Format("{0}/{1}/{2}", this.txtSuperviseArea.Text.Trim(), "receive", this.txtCIQSuperviseOrg.Text.Trim());

                BaseConfig.SetNodeValueByName(BaseConfig.SuperviseOrg_CIQ, this.txtCIQSuperviseOrg.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PullPath_CIQ, this.txtCIQPullDicPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendPath_CIQ, this.txtCIQStorageDicPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalSendBakPath_CIQ, this.txtCIQPullInfoBakPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.PushPath_CIQ, sPushDic);
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceivePath_CIQ, this.txtCIQOriginDicPath.Text.Trim());
                BaseConfig.SetNodeValueByName(BaseConfig.LocalReceiveBakPath_CIQ, this.txtCIQPushInfoBakPath.Text.Trim());

                PrintLog("更新海关配置信息完成。");
            }
            catch (Exception ex)
            {
                PrintLog("更新海关配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 初始化信息对象集合
        /// </summary>
        private void InitInfoEntityList()
        {
            infoEntityList = new List<Entity.InfoEntity>();

            if (this.chbService_CIQ.Checked || this.chbDirectService_CIQ.Checked)
            {
                InfoEntity CIQInfoEntity = GetCIQInfoEntity();

                if (CIQInfoEntity != null)
                {
                    infoEntityList.Add(CIQInfoEntity);
                }
            }

            if (this.chbService_CCK.Checked || this.chbDirectService_CCK.Checked)
            {
                InfoEntity CCKInfoEntity = GetCCKInfoEntity();

                if (CCKInfoEntity != null)
                {
                    infoEntityList.Add(CCKInfoEntity);
                }
            }

            if (this.chbService_IRS.Checked || this.chbDirectService_IRS.Checked)
            {
                InfoEntity IRSInfoEntity = GetIRSInfoEntity();

                if (IRSInfoEntity != null)
                {
                    infoEntityList.Add(IRSInfoEntity);
                }
            }

            if (this.chbSpeService.Checked)
            {
                InfoEntity SPEInfoEntity = GetSPEInfoEntity();

                if (SPEInfoEntity != null)
                {
                    infoEntityList.Add(SPEInfoEntity);
                }
            }

            if (this.chbService_MSMQ.Checked)
            {
                InfoEntity MSMQInfoEntity = GetMSMQInfoEntity();

                if (MSMQInfoEntity != null)
                {
                    infoEntityList.Add(MSMQInfoEntity);
                }
            }
        }

        /// <summary>
        /// 获取MSMQ信息对象
        /// </summary>
        /// <returns></returns>
        private InfoEntity GetMSMQInfoEntity()
        {
            InfoEntity infoEntity = new InfoEntity();

            infoEntity.OrgType = OrgType.MSMQ;
            infoEntity.FTPServerIP = this.txtFTPIP.Text.Trim();
            infoEntity.FTPLoginName = this.txtLoginName.Text.Trim();
            infoEntity.FTPLoginPwd = this.txtLoginPwd.Text.Trim();
            infoEntity.FTPPort = this.txtFTPPort.Text.Trim();

            infoEntity.PullDicPath = this.txtPullDicPath_MSMQ.Text.Trim();
            infoEntity.PullStoragePath = this.txtStorageDicPath_MSMQ.Text.Trim();
            infoEntity.PullBakPath = this.txtPullInfoBak_MSMQ.Text.Trim();
            infoEntity.PullDicPathList = this.txtPullDicPath_MSMQ.Text.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            infoEntity.PushMSMQPath_MSMQ = this.txtPushMSMQPath_MSMQ.Text.Trim();

            infoEntity.SuperviseArea = this.txtSuperviseArea.Text.Trim();
            infoEntity.SuperviseOrg = this.txtSuperviseOrg_MSMQ.Text.Trim();
            infoEntity.OriginDicPath = this.txtOriginDicPath_MSMQ.Text.Trim();
            infoEntity.PushInfoBakPath = this.txtPushInfoBak_MSMQ.Text.Trim();
            infoEntity.PushMSMQPath = this.txtPullMSMQPath_MSMQ.Text.Trim();
            infoEntity.PushDicPath = this.txtPushPath_MSMQ.Text.Trim();

            infoEntity.Merchants = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_MSMQ);

            return infoEntity;
        }

        /// <summary>
        /// 获取特殊文件信息对象
        /// </summary>
        /// <returns></returns>
        private InfoEntity GetSPEInfoEntity()
        {
            InfoEntity infoEntity = new InfoEntity();

            infoEntity.OrgType = OrgType.SPE;
            infoEntity.FTPServerIP = this.txtFTPIP.Text.Trim();
            infoEntity.FTPLoginName = this.txtLoginName.Text.Trim();
            infoEntity.FTPLoginPwd = this.txtLoginPwd.Text.Trim();
            infoEntity.FTPPort = this.txtFTPPort.Text.Trim();
            infoEntity.SuperviseArea = this.txtSuperviseArea.Text.Trim();

            infoEntity.PullDicPath = this.txtSpePullDicPath.Text.Trim();
            infoEntity.PullStoragePath = this.txtSpeStorageDicPath.Text.Trim();
            infoEntity.PullBakPath = this.txtSpeBakDicPath.Text.Trim();

            return infoEntity;
        }

        /// <summary>
        /// 开启服务
        /// </summary>
        private void StartService(InfoEntity infoEntity, CancellationToken ct)
        {
            // 判断信息对象是否为空
            if (infoEntity == null)
            {
                PrintLog("界面信息对象为空。");
                return;
            }

            // 设置结束标记为false
            bServiceStopFlag = false;
            // 设置启动按钮不可用
            new Action(delegate
            {
                this.Invoke(new Action(delegate
                {
                    this.btnOpenService.Enabled = false;
                    this.btnCloseService.Enabled = true;
                    this.nddServiceInvalTime.ReadOnly = true;
                    this.rhtbServiceLog.Focus();
                }));
            }).BeginInvoke(null, null);

            // 获取服务时间间隔，默认为30秒
            int iServerInterval = 30;
            string sServerInterval = this.nddServiceInvalTime.Text.Trim();
            int.TryParse(sServerInterval, out iServerInterval);
            List<Task> FlowTaskList = new List<Task>();
            Task mainServiceTask;
            Task directServiceTask;
            Task specialFileServiceTask;
            Task MSMQServiceTask;

            while (!bServiceStopFlag)
            {
                if (ct.IsCancellationRequested)
                {
                    break;
                }

                // ct.ThrowIfCancellationRequested();

                // 主服务流程
                if (infoEntity.isMainServiceChecked)
                {
                    mainServiceTask = Task.Factory.StartNew<bool>(() =>
                    {
                        bool bIsMainServiceSuccess = StartMainService(infoEntity);
                        if (!bIsMainServiceSuccess)
                        {
                            PrintLog(string.Format("主服务流程失败，等待{0}分钟。", iFailWaitMins), infoEntity.OrgType.ToString());
                            Thread.Sleep(iFailWaitMins * 60 * 1000); // 等待5分钟
                        }

                        return bIsMainServiceSuccess;
                    });

                    FlowTaskList.Add(mainServiceTask);
                }

                // 直连FTP服务流程
                if (infoEntity.DirectConfigEntity != null && infoEntity.DirectConfigEntity.isServiceChecked)
                {
                    directServiceTask = Task.Factory.StartNew<bool>(() =>
                    {
                        bool bIsDirectServiceSuccess = StartDirectService(infoEntity);
                        if (!bIsDirectServiceSuccess)
                        {
                            PrintLog(string.Format("[直连]-FTP服务流程失败，等待{0}分钟。", iFailWaitMins), infoEntity.OrgType.ToString());
                            Thread.Sleep(iFailWaitMins * 60 * 1000); // 等待5分钟
                        }

                        return bIsDirectServiceSuccess;
                    });

                    FlowTaskList.Add(directServiceTask);
                }

                // 特殊文件推送
                if (infoEntity.OrgType == OrgType.SPE)
                {
                    specialFileServiceTask = Task.Factory.StartNew<bool>(() =>
                    {
                        bool bIsSpecialFileServiceSuccess = StartSpecialFileService(infoEntity);
                        if (!bIsSpecialFileServiceSuccess)
                        {
                            PrintLog(string.Format("特殊文件推送流程失败，等待{0}分钟。", iFailWaitMins), infoEntity.OrgType.ToString());
                            Thread.Sleep(iFailWaitMins * 60 * 1000); // 等待5分钟
                        }

                        return bIsSpecialFileServiceSuccess;
                    });

                    FlowTaskList.Add(specialFileServiceTask);
                }

                // MSMQ文件推送
                if (infoEntity.OrgType == OrgType.MSMQ)
                {
                    //启动MSMQ监听服务
                    MSMQListenTask = new MSMQWorkFlow(infoEntity);
                    Task.Factory.StartNew(() =>
                    {
                        MSMQListenTask.StartPullMQueue();
                    });

                    MSMQServiceTask = Task.Factory.StartNew<bool>(() =>
                    {
                        bool isMSMQServiceSuccess = StartMSMQService(infoEntity);
                        if (!isMSMQServiceSuccess)
                        {
                            PrintLog(string.Format("MSMQ文件推送流程失败，等待{0}分钟。", iFailWaitMins), infoEntity.OrgType.ToString());
                            Thread.Sleep(iFailWaitMins * 60 * 1000); // 等待5分钟
                        }

                        return isMSMQServiceSuccess;
                    });

                    FlowTaskList.Add(MSMQServiceTask);
                }

                if (!bServiceStopFlag && !ct.IsCancellationRequested)
                {
                    Task.WaitAll(FlowTaskList.ToArray());

                    // 周期服务完毕
                    PrintLog("单轮服务执行完毕，开始服务周期等待....", infoEntity.OrgType.ToString());
                    Thread.Sleep(iServerInterval * 1000);
                }
            }
        }

        /// <summary>
        /// 启动MSMQ服务流程
        /// </summary>
        /// <param name="infoEntity"></param>
        private bool StartMSMQService(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                PrintLog(string.Format("{0}：主服务流程信息配置对象为空.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
                return false;
            }

            PrintLog(string.Format("{0}：开始主服务流程...", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            // 启动MSMQ服务流程
            MSMQWorkFlow workFlow = new MSMQWorkFlow(infoEntity);
            bool isSuccess = workFlow.Start();
            if (!isSuccess)
            {
                PrintLog(string.Format("{0}：主服务流程执行失败.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            }

            PrintLog(string.Format("{0}：主服务流程结束.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            return isSuccess;
        }

        /// <summary>
        ///  特殊文件推送服务流程
        /// </summary>
        /// <param name="infoEntity"></param>
        /// <returns></returns>
        private bool StartSpecialFileService(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                PrintLog(string.Format("{0}：特殊文件推送服务流程信息配置对象为空.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
                return false;
            }

            PrintLog(string.Format("{0}：开始特殊文件推送服务流程...", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            // 启动主服务流程
            SpecialFileWorkFlow workFlow = new SpecialFileWorkFlow(infoEntity);
            bool isSuccess = workFlow.Start();
            if (!isSuccess)
            {
                PrintLog(string.Format("{0}：特殊文件推送服务流程执行失败.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            }

            PrintLog(string.Format("{0}：特殊文件推送服务流程结束.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            return isSuccess;
        }

        /// <summary>
        /// 启动主服务流程
        /// </summary>
        /// <param name="infoEntity"></param>
        private bool StartMainService(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                PrintLog(string.Format("{0}：主服务流程信息配置对象为空.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
                return false;
            }

            PrintLog(string.Format("{0}：开始主服务流程...", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            // 启动主服务流程
            ServiceWorkFlow workFlow = new ServiceWorkFlow(infoEntity);
            bool isSuccess = workFlow.Start();
            if (!isSuccess)
            {
                PrintLog(string.Format("{0}：主服务流程执行失败.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            }

            PrintLog(string.Format("{0}：主服务流程结束.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            return isSuccess;
        }

        /// <summary>
        /// 启动直连FTP流程
        /// </summary>
        /// <param name="infoEntity"></param>
        private bool StartDirectService(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                PrintLog(string.Format("{0}直连FTP流程信息配置对象为空.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));

                return false;
            }

            PrintLog(string.Format("{0}开始直连FTP流程...", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));

            bool isDirectServiceSuccess = true;
            if (infoEntity.DirectConfigEntity != null && infoEntity.DirectConfigEntity.isServiceChecked)
            {

                DirectFTPWorkFlow directWorkFlow = new DirectFTPWorkFlow(infoEntity);
                isDirectServiceSuccess = directWorkFlow.Start();
            }

            if (!isDirectServiceSuccess)
            {
                PrintLog(string.Format("{0}直连FTP服务流程执行失败.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            }

            PrintLog(string.Format("{0}直连FTP流程结束.", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
            return isDirectServiceSuccess;
        }

        /// <summary>
        /// 封装海关配置信息对象
        /// </summary>
        /// <returns></returns>
        private InfoEntity GetCIQInfoEntity()
        {
            InfoEntity infoEntity = new InfoEntity();

            infoEntity.OrgType = OrgType.CIQ;
            infoEntity.FTPServerIP = this.txtFTPIP.Text.Trim();
            infoEntity.FTPLoginName = this.txtLoginName.Text.Trim();
            infoEntity.FTPLoginPwd = this.txtLoginPwd.Text.Trim();
            infoEntity.PullDicPath = this.txtCIQPullDicPath.Text.Trim();
            infoEntity.PullStoragePath = this.txtCIQStorageDicPath.Text.Trim();
            infoEntity.PullBakPath = this.txtCIQPullInfoBakPath.Text.Trim();
            infoEntity.OriginDicPath = this.txtCIQOriginDicPath.Text.Trim();
            infoEntity.PushDicPath = this.txtCIQPushDicPath.Text.Trim();
            infoEntity.PushInfoBakPath = this.txtCIQPushInfoBakPath.Text.Trim();
            infoEntity.SuperviseArea = this.txtSuperviseArea.Text.Trim();
            infoEntity.SuperviseOrg = this.txtCIQSuperviseOrg.Text.Trim();
            infoEntity.FTPPort = this.txtFTPPort.Text.Trim();
            infoEntity.PullDicPathList = this.txtCIQPullDicPath.Text.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            infoEntity.isMainServiceChecked = this.chbService_CIQ.Checked;

            //新需求，商家信息从数据库中获取
            infoEntity.Merchants = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_CIQ);
            //infoEntity.Merchants = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            // 组装直连配置信息对象
            DirectConfigEntity directConfigEntity = new DirectConfigEntity();
            directConfigEntity.DirectFTPIP = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_CIQ);
            directConfigEntity.DirectFTPLoginName = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_CIQ);
            directConfigEntity.DirectFTPLoginPwd = BaseConfig.GetNodeValueByName(BaseConfig.Password_CIQ);
            directConfigEntity.DirectFTPPort = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_CIQ);
            directConfigEntity.isServiceChecked = this.chbDirectService_CIQ.Checked;
            directConfigEntity.DirectFTPPushPath = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_CIQ);
            directConfigEntity.ConnectType = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_CIQ);
            directConfigEntity.OrgType = OrgType.CIQ;

            infoEntity.DirectConfigEntity = directConfigEntity;

            return infoEntity;
        }

        /// <summary>
        /// 封装国签配置信息对象
        /// </summary>
        /// <returns></returns>
        private InfoEntity GetCCKInfoEntity()
        {
            InfoEntity infoEntity = new InfoEntity();

            infoEntity.OrgType = OrgType.CCK;
            infoEntity.FTPServerIP = this.txtFTPIP.Text.Trim();
            infoEntity.FTPLoginName = this.txtLoginName.Text.Trim();
            infoEntity.FTPLoginPwd = this.txtLoginPwd.Text.Trim();
            infoEntity.FTPPort = this.txtFTPPort.Text.Trim();
            infoEntity.SuperviseArea = this.txtSuperviseArea.Text.Trim();
            infoEntity.PullDicPath = this.txtPullDicPath_CCK.Text.Trim();
            infoEntity.PullStoragePath = this.txtStorageDicPath_CCK.Text.Trim();
            infoEntity.PullBakPath = this.txtPullInfoBakPath_CCK.Text.Trim();
            infoEntity.OriginDicPath = this.txtOriginDicPath_CCK.Text.Trim();
            infoEntity.PushDicPath = this.txtPushDicPath_CCK.Text.Trim();
            infoEntity.PushInfoBakPath = this.txtPushInfoBakPath_CCK.Text.Trim();
            infoEntity.SuperviseOrg = this.txtSuperviseOrg_CCK.Text.Trim();
            infoEntity.PullDicPathList = this.txtPullDicPath_CCK.Text.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            infoEntity.isMainServiceChecked = this.chbService_CCK.Checked;

            //新需求，商家信息从数据库中获取
            infoEntity.Merchants = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_CCK);
            //infoEntity.Merchants = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            // 组装直连配置信息对象
            DirectConfigEntity directConfigEntity = new DirectConfigEntity();
            directConfigEntity.DirectFTPIP = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_CCK);
            directConfigEntity.DirectFTPLoginName = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_CCK);
            directConfigEntity.DirectFTPLoginPwd = BaseConfig.GetNodeValueByName(BaseConfig.Password_CCK);
            directConfigEntity.DirectFTPPort = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_CCK);
            directConfigEntity.isServiceChecked = this.chbDirectService_CCK.Checked;
            directConfigEntity.DirectFTPPushPath = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_CCK);
            directConfigEntity.ConnectType = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_CCK);
            directConfigEntity.OrgType = OrgType.CCK;

            infoEntity.DirectConfigEntity = directConfigEntity;

            return infoEntity;
        }

        /// <summary>
        /// 封装国检配置信息对象
        /// </summary>
        /// <returns></returns>
        private InfoEntity GetIRSInfoEntity()
        {
            InfoEntity infoEntity = new InfoEntity();

            infoEntity.OrgType = OrgType.IRS;
            infoEntity.FTPServerIP = this.txtFTPIP.Text.Trim();
            infoEntity.FTPLoginName = this.txtLoginName.Text.Trim();
            infoEntity.FTPLoginPwd = this.txtLoginPwd.Text.Trim();
            infoEntity.FTPPort = this.txtFTPPort.Text.Trim();
            infoEntity.SuperviseArea = this.txtSuperviseArea.Text.Trim();
            infoEntity.PullDicPath = this.txtPullDicPath_IRS.Text.Trim();
            infoEntity.PullStoragePath = this.txtStorageDicPath_IRS.Text.Trim();
            infoEntity.PullBakPath = this.txtPullInfoBakPath_IRS.Text.Trim();
            infoEntity.OriginDicPath = this.txtOriginDicPath_IRS.Text.Trim();
            infoEntity.PushDicPath = this.txtPushDicPath_IRS.Text.Trim();
            infoEntity.PushInfoBakPath = this.txtPushInfoBakPath_IRS.Text.Trim();
            infoEntity.SuperviseOrg = this.txtSuperviseOrg_IRS.Text.Trim();
            infoEntity.PullDicPathList = this.txtPullDicPath_IRS.Text.Trim().Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            infoEntity.isMainServiceChecked = this.chbService_IRS.Checked;

            //新需求，商家信息从数据库中获取
            infoEntity.Merchants = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_IRS);
            //infoEntity.Merchants = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            // 组装直连配置信息对象
            DirectConfigEntity directConfigEntity = new DirectConfigEntity();
            directConfigEntity.DirectFTPIP = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_IRS);
            directConfigEntity.DirectFTPLoginName = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_IRS);
            directConfigEntity.DirectFTPLoginPwd = BaseConfig.GetNodeValueByName(BaseConfig.Password_IRS);
            directConfigEntity.DirectFTPPort = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_IRS);
            directConfigEntity.isServiceChecked = this.chbDirectService_IRS.Checked;
            directConfigEntity.DirectFTPPushPath = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_IRS);
            directConfigEntity.ConnectType = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_IRS);
            directConfigEntity.OrgType = OrgType.IRS;

            infoEntity.DirectConfigEntity = directConfigEntity;

            return infoEntity;
        }

        /// <summary>
        /// 验证基础数据配置
        /// </summary>
        /// <returns></returns>
        private bool CheckBaseCofig()
        {
            if (this.txtFTPIP.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器IP.");
                this.txtFTPIP.Focus();
                return false;
            }

            if (txtLoginName.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP登录名.");
                this.txtLoginName.Focus();
                return false;
            }

            if (this.txtLoginPwd.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP登录密码.");
                this.txtLoginPwd.Focus();
                return false;
            }

            if (this.txtFTPPort.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器端口.");
                this.txtFTPPort.Focus();
                return false;
            }

            if (this.txtSuperviseArea.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置监管区域.");
                this.txtSuperviseArea.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证海关配置
        /// </summary>
        /// <returns></returns>
        private bool CheckCIQConfig()
        {
            if (this.txtCIQSuperviseOrg.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置监管机构.");
                this.txtCIQSuperviseOrg.Focus();
                return false;
            }

            if (this.txtCIQPullDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。.");
                this.txtCIQPullDicPath.Focus();
                return false;
            }

            if (this.txtCIQStorageDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。");
                this.txtCIQStorageDicPath.Focus();
                return false;
            }

            if (this.txtCIQPullInfoBakPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取备份目录.");
                this.txtCIQPullInfoBakPath.Focus();
                return false;
            }

            if (this.txtCIQOriginDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置来源目录.");
                this.txtCIQOriginDicPath.Focus();
                return false;
            }

            if (this.txtCIQPushDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送目录.");
                this.txtCIQPushDicPath.Focus();
                return false;
            }

            if (this.txtCIQPushInfoBakPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送备份目录.");
                this.txtCIQPushInfoBakPath.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 开启服务前的数据验证
        /// </summary>
        /// <returns></returns>
        private bool CheckValues()
        {
            if (!CheckBaseCofig())
            {
                return false;
            }

            if (this.chbService_CIQ.Checked)
            {
                if (!CheckCIQConfig())
                {
                    return false;
                }
            }


            if (chbService_CCK.Checked)
            {
                if (!CheckCCKConfig())
                {
                    return false;
                }
            }

            if (chbService_IRS.Checked)
            {
                if (!CheckIRSConfig())
                {
                    return false;
                }
            }

            if (chbSpeService.Checked)
            {
                if (!CheckSPEConfig())
                {
                    return false;
                }
            }

            if (chbService_MSMQ.Checked)
            {
                if (!CheckMSMQConfig())
                {
                    return false;
                }
            }

            if (this.nddServiceInvalTime.Text.Trim() == string.Empty)
            {
                this.nddServiceInvalTime.Text = "30";
            }

            return true;
        }
        /// <summary>
        /// 检查MSMQ文件处理配置
        /// </summary>
        /// <returns></returns>
        private bool CheckMSMQConfig()
        {
            if (this.txtSuperviseOrg_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置监管机构.");
                this.txtSuperviseOrg_MSMQ.Focus();
                return false;
            }

            if (this.txtPullDicPath_MSMQ.Text.Trim() == string.Empty)
            {
                BaseConfig.SetNodeValueByName(BaseConfig.Merchants_MSMQ, string.Empty);
                Common.ShowPromptInfo("请配置拉取商家。");
                this.txtPullDicPath_MSMQ.Focus();
                return false;
            }

            if (txtStorageDicPath_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ报文拉取存放目录.");
                this.txtStorageDicPath_MSMQ.Focus();
                return false;
            }

            if (this.txtPullInfoBak_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ报文拉取备份目录.");
                this.txtPullInfoBak_MSMQ.Focus();
                return false;
            }

            if (txtOriginDicPath_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ文件来源目录.");
                this.txtOriginDicPath_MSMQ.Focus();
                return false;
            }

            if (this.txtPushInfoBak_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ文件备份目录.");
                this.txtPushInfoBak_MSMQ.Focus();
                return false;
            }

            if (this.txtPullMSMQPath_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ拉取目录.");
                this.txtPullMSMQPath_MSMQ.Focus();
                return false;
            }

            if (this.txtPushMSMQPath_MSMQ.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置MSMQ推送目录.");
                this.txtPushMSMQPath_MSMQ.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查特殊文件处理配置
        /// </summary>
        /// <returns></returns>
        private bool CheckSPEConfig()
        {
            if (this.txtSpePullDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置特殊文件拉取目录.");
                this.txtSpePullDicPath.Focus();
                return false;
            }

            if (this.txtSpeStorageDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置特殊文件存放目录.");
                this.txtSpeStorageDicPath.Focus();
                return false;
            }

            if (this.txtSpeBakDicPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置特殊文件备份目录.");
                this.txtSpeBakDicPath.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证区内监管配置
        /// </summary>
        /// <returns></returns>
        private bool CheckIRSConfig()
        {
            if (this.txtSuperviseOrg_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置监管机构.");
                this.txtSuperviseOrg_IRS.Focus();
                return false;
            }

            if (this.txtPullDicPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。");
                this.txtPullDicPath_IRS.Focus();
                return false;
            }

            if (this.txtStorageDicPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。");
                this.txtStorageDicPath_IRS.Focus();
                return false;
            }

            if (this.txtPullInfoBakPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取备份目录.");
                this.txtPullInfoBakPath_IRS.Focus();
                return false;
            }

            if (this.txtOriginDicPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置来源目录.");
                this.txtOriginDicPath_IRS.Focus();
                return false;
            }

            if (this.txtPushDicPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送目录.");
                this.txtPushDicPath_IRS.Focus();
                return false;
            }

            if (this.txtPushInfoBakPath_IRS.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送备份目录.");
                this.txtPushInfoBakPath_IRS.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 验证国签配置
        /// </summary>
        /// <returns></returns>
        private bool CheckCCKConfig()
        {
            if (this.txtSuperviseOrg_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置监管机构.");
                this.txtSuperviseOrg_CCK.Focus();
                return false;
            }

            if (this.txtPullDicPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。.");
                this.txtPullDicPath_CCK.Focus();
                return false;
            }

            if (this.txtStorageDicPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取商家。");
                this.txtStorageDicPath_CCK.Focus();
                return false;
            }

            if (this.txtPullInfoBakPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置拉取备份目录.");
                this.txtPullInfoBakPath_CCK.Focus();
                return false;
            }

            if (this.txtOriginDicPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置来源目录.");
                this.txtOriginDicPath_CCK.Focus();
                return false;
            }

            if (this.txtPushDicPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送目录.");
                this.txtPushDicPath_CCK.Focus();
                return false;
            }

            if (this.txtPushInfoBakPath_CCK.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置推送备份目录.");
                this.txtPushInfoBakPath_CCK.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 浏览报文拉取存放目录文件夹路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewStoragePath_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtCIQStorageDicPath);
        }

        /// <summary>
        /// 浏览文件夹选择路径
        /// </summary>
        /// <param name="txtBox"></param>
        private void ViewDicPath(TextBox txtBox)
        {
            FolderBrowserDialog dilog = new FolderBrowserDialog();
            dilog.Description = "请选择文件夹";
            if (dilog.ShowDialog() == DialogResult.OK)
            {
                txtBox.Text = dilog.SelectedPath;
            }
        }

        /// <summary>
        /// 浏览报文拉取备份目录文件夹路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewPullBakPath_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtCIQPullInfoBakPath);
        }

        /// <summary>
        /// 浏览报文推送备份目录问价夹路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewPushBakPath_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtCIQPushInfoBakPath);
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCloseService_Click(object sender, EventArgs e)
        {
            CloseService();
        }

        /// <summary>
        /// 关闭服务
        /// </summary>
        public void CloseService()
        {
            bServiceStopFlag = true;

            new Action(delegate
            {
                this.Invoke(new Action(delegate
                {
                    this.btnCloseService.Enabled = false;
                    this.btnOpenService.Enabled = true;
                    this.nddServiceInvalTime.ReadOnly = false;
                    SetCurFormAllGBoxEnable(true);

                    if (taskEntityList != null)
                    {
                        foreach (TaskEntity taskEntity in taskEntityList)
                        {
                            if (taskEntity == null || taskEntity.Task == null || taskEntity.CancellationTokenSource == null)
                            {
                                continue;
                            }

                            taskEntity.CancellationTokenSource.Cancel();
                        }
                    }
                }));
            }).BeginInvoke(null, null);

            //关闭MSMQ监听服务
            MSMQListenTask.StopPullMQueue();
        }

        /// <summary>
        /// 是否显示日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbShowLog_CheckedChanged(object sender, EventArgs e)
        {
            bool isCheck = chbShowLog.Checked;
            if (isCheck)
            {
                // 日志打印在富文本框中
                this.rhtbServiceLog.Text = "开启打印日志...";
            }
            else
            {
                this.rhtbServiceLog.Text = string.Empty;
            }
        }

        /// <summary>
        /// 是否显示密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ckbPwd_CheckedChanged(object sender, EventArgs e)
        {
            if (!ckbPwd.Checked)
                this.txtLoginPwd.PasswordChar = '*';
            else
                txtLoginPwd.PasswordChar = (char)0;
        }

        /// <summary>
        /// 打印日志
        /// </summary>
        /// <param name="pMessage">日志信息</param>
        /// <param name="pType">服务类型</param>
        public void PrintLog(string pMessage, string pType = "")
        {
            if (string.IsNullOrEmpty(pMessage))
            {
                return;
            }

            if (!ckBox.Checked)
            {
                LogHelper.WriteLog(pMessage);
                return;
            }

            if (ric.InvokeRequired)
            {
                Action<string> ac = (p) =>
                {
                    if (ric.Text.Length >= iMaxLength)
                    {
                        ric.Text = string.Empty;
                    }

                    ric.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + p + Environment.NewLine);
                };

                this.BeginInvoke(ac, pMessage);

                //PrintLogDelegate d = new PrintLogDelegate(PrintLog);
                //this.BeginInvoke(d, ric, msg);//在创建控件的基础句柄所在线程上，用指定的参数异步执行指定委托。
            }
            else
            {
                if (ric.Text.Length >= iMaxLength)
                {
                    ric.Text = string.Empty;
                }

                ric.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ： " + pMessage + Environment.NewLine);
                ric.SelectionStart = ric.Text.Length;
                ric.ScrollToCaret();
            }

            LogHelper.WriteLog(pMessage, pType);
        }
        /// <summary>
        /// 浏览选择来源文件夹目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnViewOriganDicPath_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtCIQOriginDicPath);
        }

        /// <summary>
        /// 打开公司网址
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lkCompany_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", "http://www.eccang.com");
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMain_Load(object sender, EventArgs e)
        {
            // 初始化配置信息
            InitData();
            this.btnOpenService.Focus();
        }

        /// <summary>
        /// 初始化配置信息
        /// </summary>
        private void InitData()
        {
            if (!File.Exists(BaseConfig.IniFilePath))
            {
                return;
            }

            try
            {
                InitBaseDataInfo();
                InitCIQDataInfo();
                InitCCKDataInfo();
                InitIRSDataInfo();
                InitSPEDataInfo();
                InitMSMQDataInfo();
            }
            catch (Exception ex)
            {
                PrintLog("初始化配置信息出现异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 初始化MSMQ文件推送配置信息
        /// </summary>
        private void InitMSMQDataInfo()
        {
            try
            {
                // 报文推送配置信息
                string strSuperviseAreaStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseArea);
                string strSuperviseOrg_MSMQStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_MSMQ);
                string strMerchantsStr = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_MSMQ);

                List<string> SuperviseAreaList = new List<string>();
                string strTempStr = @"{0}/{1}/{2}/{3}/";
                SuperviseAreaList = strMerchantsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                StringBuilder objPullDicPath = new StringBuilder();
                foreach (var item in SuperviseAreaList)
                {
                    objPullDicPath.Append(string.Format(
                        strTempStr, strSuperviseAreaStr, item, BaseConfig.TypeSend, strSuperviseOrg_MSMQStr) + ";");
                }

                //报文拉取配置信息
                this.txtPullDicPath_MSMQ.Text = objPullDicPath.ToString();
                this.txtPushPath_MSMQ.Text = string.Format(
                    "{0}/{1}/{2}", strSuperviseAreaStr, BaseConfig.TypeReceive, strSuperviseOrg_MSMQStr);
                this.txtStorageDicPath_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.StorageDicPath_MSMQ);
                this.txtPullInfoBak_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.PullInfoBak_MSMQ);
                this.txtOriginDicPath_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.OriginDicPath_MSMQ);
                this.txtPushInfoBak_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.PushInfoBak_MSMQ);
                this.txtPullMSMQPath_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.PullPath_MSMQ);
                this.txtPushMSMQPath_MSMQ.Text = BaseConfig.GetNodeValueByName(BaseConfig.PushMSMQPath_MSMQ);
                this.txtSuperviseOrg_MSMQ.Text = strSuperviseOrg_MSMQStr;
            }
            catch (Exception ex)
            {
                PrintLog("初始化MSMQ文件推送配置信息异常：" + ex.Message);
                return;
            }
        }

        /// <summary>
        /// 初始化特殊文件推送配置信息
        /// </summary>
        private void InitSPEDataInfo()
        {
            try
            {
                this.txtSpePullDicPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.PullPath_SPE);
                this.txtSpeStorageDicPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendPath_SPE);
                this.txtSpeBakDicPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendBakPath_SPE);
            }
            catch (Exception ex)
            {
                PrintLog("初始化特殊文件推送配置信息异常：" + ex.Message);
                return;
            }
        }

        private void InitIRSDataInfo()
        {
            //新需求，商家信息从数据库中获取
            string sMerchantsStr = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_IRS);
            //string sMerchantsStr = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            string sSuperviseAreaStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseArea);
            string sSuperviseOrg_IRSStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_IRS);
            List<string> SuperviseAreaList = new List<string>();

            string sType = BaseConfig.TypeSend;
            string sTempStr = @"{0}/{1}/{2}/{3}/";
            SuperviseAreaList = sMerchantsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var item in SuperviseAreaList)
            {
                builder.Append(string.Format(sTempStr, sSuperviseAreaStr, item, sType, sSuperviseOrg_IRSStr) + ";");
            }
            string sPullDicPath = builder.ToString();

            // 报文拉取配置信息
            this.txtPullDicPath_IRS.Text = sPullDicPath;
            this.txtStorageDicPath_IRS.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendPath_IRS);
            this.txtPullInfoBakPath_IRS.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendBakPath_IRS);

            // 报文推送配置信息
            sType = BaseConfig.TypeReceive;
            this.txtPushDicPath_IRS.Text = string.Format("{0}/{1}/{2}", sSuperviseAreaStr, sType, sSuperviseOrg_IRSStr);
            this.txtOriginDicPath_IRS.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceivePath_IRS);
            this.txtPushInfoBakPath_IRS.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceiveBakPath_IRS);
            this.txtSuperviseOrg_IRS.Text = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_IRS);
        }

        private void InitCCKDataInfo()
        {
            //新需求，商家信息从数据库中获取
            string sMerchantsStr = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_CCK);
            //string sMerchantsStr = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            string sSuperviseAreaStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseArea);
            string sSuperviseOrg_CCKStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_CCK);
            List<string> SuperviseAreaList = new List<string>();

            string sType = BaseConfig.TypeSend;
            string sTempStr = @"{0}/{1}/{2}/{3}/";
            SuperviseAreaList = sMerchantsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            StringBuilder builder = new StringBuilder();
            foreach (var item in SuperviseAreaList)
            {
                builder.Append(string.Format(sTempStr, sSuperviseAreaStr, item, sType, sSuperviseOrg_CCKStr) + ";");
            }
            string sPullDicPath = builder.ToString().Trim();

            // 报文拉取配置信息
            this.txtPullDicPath_CCK.Text = sPullDicPath;

            this.txtStorageDicPath_CCK.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendPath_CCK);
            this.txtPullInfoBakPath_CCK.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendBakPath_CCK);

            // 报文推送配置信息
            sType = BaseConfig.TypeReceive;
            this.txtPushDicPath_CCK.Text = string.Format("{0}/{1}/{2}", sSuperviseAreaStr, sType, sSuperviseOrg_CCKStr);
            this.txtOriginDicPath_CCK.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceivePath_CCK);
            this.txtPushInfoBakPath_CCK.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceiveBakPath_CCK);
            this.txtSuperviseOrg_CCK.Text = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_CCK);
        }

        private void InitBaseDataInfo()
        {
            //基础配置信息
            this.txtFTPIP.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP);
            this.txtLoginName.Text = BaseConfig.GetNodeValueByName(BaseConfig.LoginName);
            this.txtLoginPwd.Text = BaseConfig.GetNodeValueByName(BaseConfig.Password);
            this.txtFTPPort.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort);
            this.txtSuperviseArea.Text = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseArea);
        }

        private void InitCIQDataInfo()
        {
            //新需求，商家信息从数据库配置中获取
            string sMerchantsStr = BaseConfig.GetNodeValueByName(BaseConfig.Merchants_CIQ);
            //string sMerchantsStr = BaseConfig.GetConfigValueByDB(BaseConfig.SQL_Merchants);

            string sSuperviseAreaStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseArea);
            string sSuperviseOrg_CIQStr = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_CIQ);
            List<string> SuperviseAreaList = new List<string>();

            string sType = BaseConfig.TypeSend;
            string sTempStr = @"{0}/{1}/{2}/{3}/";
            SuperviseAreaList = sMerchantsStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            StringBuilder builder = new StringBuilder();
            foreach (var item in SuperviseAreaList)
            {
                builder.Append(string.Format(sTempStr, sSuperviseAreaStr, item, sType, sSuperviseOrg_CIQStr) + ";");
            }

            // 报文拉取配置信息
            this.txtCIQPullDicPath.Text = builder.ToString().Trim();

            this.txtCIQStorageDicPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendPath_CIQ);
            this.txtCIQPullInfoBakPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalSendBakPath_CIQ);

            // 报文推送配置信息
            sType = BaseConfig.TypeReceive;
            this.txtCIQPushDicPath.Text = string.Format("{0}/{1}/{2}", sSuperviseAreaStr, sType, sSuperviseOrg_CIQStr);
            this.txtCIQOriginDicPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceivePath_CIQ);
            this.txtCIQPushInfoBakPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.LocalReceiveBakPath_CIQ);
            this.txtCIQSuperviseOrg.Text = BaseConfig.GetNodeValueByName(BaseConfig.SuperviseOrg_CIQ);
        }

        #region 浏览文件夹
        private void btnViewStoragePath_CCK_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtStorageDicPath_CCK);
        }

        private void btnViewPullBakPath_CCK_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPullInfoBakPath_CCK);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtStorageDicPath_IRS);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPullInfoBakPath_IRS);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtOriginDicPath_IRS);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPushInfoBakPath_IRS);
        }

        private void btnViewOriganDicPath_CCK_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtOriginDicPath_CCK);
        }

        private void btnViewPushBakPath_CCK_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPushInfoBakPath_CCK);
        }

        #endregion

        /// <summary>
        /// 海关直连FTP服务选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbDirectService_CIQ_CheckedChanged(object sender, EventArgs e)
        {
            ShowDirectForm(chbDirectService_CIQ, "海关", OrgType.CIQ);

            if (chbService_CIQ.Checked && chbDirectService_CIQ.Checked)
            {
                chbService_CIQ.Checked = false;
            }
        }

        /// <summary>
        ///  国检直连FTP服务选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbDirectService_CCK_CheckedChanged(object sender, EventArgs e)
        {
            ShowDirectForm(chbDirectService_CCK, "国检", OrgType.CCK);

            if (chbService_CCK.Checked && chbDirectService_CCK.Checked)
            {
                chbService_CCK.Checked = false;
            }
        }

        /// <summary>
        ///  区内监管直连FTP服务选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbDirectService_IRS_CheckedChanged(object sender, EventArgs e)
        {
            ShowDirectForm(chbDirectService_IRS, "区内监管", OrgType.IRS);

            if (chbService_IRS.Checked && chbDirectService_IRS.Checked)
            {
                chbService_IRS.Checked = false;
            }
        }

        /// <summary>
        /// 直连FTP配置
        /// </summary>
        /// <param name="cBox"></param>
        /// <param name="sName"></param>
        /// <param name="type"></param>
        private void ShowDirectForm(CheckBox cBox, string sName, OrgType type)
        {
            if (!cBox.Checked)
            {
                return;
            }

            // 勾选后弹出配置框
            frmDirectConfig frm = new frmDirectConfig(sName, type);
            var dResult = frm.ShowDialog();

            cBox.Checked = dResult == DialogResult.OK ? true : false;
        }

        /// <summary>
        /// 海关服务勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbService_CIQ_CheckedChanged(object sender, EventArgs e)
        {
            if (chbService_CIQ.Checked && this.chbDirectService_CIQ.Checked)
            {
                chbDirectService_CIQ.Checked = false;
            }
        }

        /// <summary>
        /// 国检服务勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbService_CCK_CheckedChanged(object sender, EventArgs e)
        {
            if (chbService_CCK.Checked && this.chbDirectService_CCK.Checked)
            {
                chbDirectService_CCK.Checked = false;
            }
        }

        /// <summary>
        /// 区内监管服务勾选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chbService_IRS_CheckedChanged(object sender, EventArgs e)
        {
            if (chbService_IRS.Checked && this.chbDirectService_IRS.Checked)
            {
                chbDirectService_IRS.Checked = false;
            }
        }

        /// <summary>
        /// 浏览特殊文件存放位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSpeStorage_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtSpeStorageDicPath);
        }

        /// <summary>
        /// 浏览特殊文件备份位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSpeBak_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtSpeBakDicPath);
        }

        /// <summary>
        /// MSMQ来源目录
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btnOriginDicPath_MSMQ_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtOriginDicPath_MSMQ);
        }

        /// <summary>
        /// MSMQ备份目录
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btnPushInfoBak_MSMQ_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPushInfoBak_MSMQ);
        }

        private void btnStorageDicPath_MSMQ_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtStorageDicPath_MSMQ);
        }

        private void btnPullInfoBak_MSMQ_Click(object sender, EventArgs e)
        {
            ViewDicPath(this.txtPullInfoBak_MSMQ);
        }
    }
}
