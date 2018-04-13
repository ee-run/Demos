using ReportToolDemo.ReportServiceTool.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ReportToolDemo.ReportServiceTool.Forms
{
    public partial class frmDirectConfig : Form
    {
        /// <summary>
        /// tab页的名称
        /// </summary>
        private string m_Text = string.Empty;

        /// <summary>
        /// tab页的服务类型，默认为海关
        /// </summary>
        private OrgType m_OrgType = OrgType.CIQ;

        /// <summary>
        /// 无参构造函数
        /// </summary>
        private frmDirectConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        /// <param name="sText"></param>
        public frmDirectConfig(string sText, OrgType orgType) : this()
        {
            this.m_OrgType = orgType;
            this.Text = string.Format("{0}直连FTP服务配置", sText);
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
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmDirectConfig_Load(object sender, EventArgs e)
        {
            // 窗体加载时，读取配置文件，加载配置数据
            LoadDataByType(m_OrgType);
        }

        /// <summary>
        /// 根据服务类型加载配置数据
        /// </summary>
        /// <param name="m_OrgType"></param>
        private void LoadDataByType(OrgType m_OrgType)
        {
            try
            {
                switch (m_OrgType)
                {
                    case OrgType.CIQ:
                        LoadConfigData_CIQ();
                        break;
                    case OrgType.CCK:
                        LoadConfigData_CCK();
                        break;
                    case OrgType.IRS:
                        LoadConfigData_IRS();
                        break;
                    case OrgType.SPE:
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("根据服务类型{0}加载配置数据出现异常：{2}", m_Text, ex.Message));
                return;
            }
        }

        /// <summary>
        /// 加载区内监管直连FTP配置数据
        /// </summary>
        private void LoadConfigData_IRS()
        {
            this.txtFTPIP.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_IRS);
            this.txtLoginName.Text = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_IRS);
            this.txtLoginPwd.Text = BaseConfig.GetNodeValueByName(BaseConfig.Password_IRS);
            this.txtFTPPort.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_IRS);
            this.txtDirectFTPPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_IRS);
            rbtnPasv.Checked = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_IRS).ToLower() == "true";
            rbtnPort.Checked = !rbtnPasv.Checked;
        }

        /// <summary>
        /// 加载国检直连FTP配置数据
        /// </summary>
        private void LoadConfigData_CCK()
        {
            this.txtFTPIP.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_CCK);
            this.txtLoginName.Text = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_CCK);
            this.txtLoginPwd.Text = BaseConfig.GetNodeValueByName(BaseConfig.Password_CCK);
            this.txtFTPPort.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_CCK);
            this.txtDirectFTPPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_CCK);
            rbtnPasv.Checked = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_CCK).ToLower() == "true";
            rbtnPort.Checked = !rbtnPasv.Checked;
        }

        /// <summary>
        /// 加载海关直连FTP配置数据
        /// </summary>
        private void LoadConfigData_CIQ()
        {
            this.txtFTPIP.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPIP_CIQ);
            this.txtLoginName.Text = BaseConfig.GetNodeValueByName(BaseConfig.LoginName_CIQ);
            this.txtLoginPwd.Text = BaseConfig.GetNodeValueByName(BaseConfig.Password_CIQ);
            this.txtFTPPort.Text = BaseConfig.GetNodeValueByName(BaseConfig.FTPPort_CIQ);
            this.txtDirectFTPPath.Text = BaseConfig.GetNodeValueByName(BaseConfig.DirectFTPPushPath_CIQ);
            rbtnPasv.Checked = BaseConfig.GetNodeValueByName(BaseConfig.ConnectType_CIQ).ToLower() == "true";
            rbtnPort.Checked = !rbtnPasv.Checked;
        }

        /// <summary>
        /// 配置确认
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfigOk_Click(object sender, EventArgs e)
        {
            //验证配置数据
            if (!CheckConfigData())
            {
                return;
            }

            // 更新配置信息到INI文件
            bool bUpdateSuccess = false;
            bUpdateSuccess = UpdateConfigDataByType(m_OrgType);

            // 若更新失败，则取消并提示
            if (!bUpdateSuccess)
            {
                this.DialogResult = DialogResult.Cancel;
                return;
            }

            // 更新成功，则选择直连服务
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 验证配置数据
        /// </summary>
        /// <returns></returns>
        private bool CheckConfigData()
        {
            if (this.txtFTPIP.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器IP。");
                return false;
            }

            if (this.txtFTPPort.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器端口。");
                return false;
            }

            if (this.txtLoginName.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器登录名。");
                return false;
            }

            if (this.txtLoginPwd.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置FTP服务器登录密码。");
                return false;
            }

            if (this.txtDirectFTPPath.Text.Trim() == string.Empty)
            {
                Common.ShowPromptInfo("请配置直连FTP服务器目录。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 更新配置信息
        /// </summary>
        /// <param name="m_OrgType"></param>
        /// <returns></returns>
        private bool UpdateConfigDataByType(OrgType m_OrgType)
        {
            try
            {
                string sTypeStr = m_OrgType.ToString();

                BaseConfig.SetNodeValueByName(string.Format("FTPIP_{0}", sTypeStr), this.txtFTPIP.Text.Trim());
                BaseConfig.SetNodeValueByName(string.Format("FTPPort_{0}", sTypeStr), this.txtFTPPort.Text.Trim());
                BaseConfig.SetNodeValueByName(string.Format("LoginName_{0}", sTypeStr), this.txtLoginName.Text.Trim());
                BaseConfig.SetNodeValueByName(string.Format("Password_{0}", sTypeStr), this.txtLoginPwd.Text.Trim());
                BaseConfig.SetNodeValueByName(string.Format("DirectFTPPushPath_{0}", sTypeStr), this.txtDirectFTPPath.Text.Trim());
                BaseConfig.SetNodeValueByName(string.Format("ConnectType_{0}", sTypeStr), rbtnPasv.Checked.ToString().ToLower());

                return true;
            }
            catch (Exception ex)
            {
                Common.ShowPromptInfo(string.Format("配置直连FTP失败：{0}", ex.Message));
                return false;
            }
        }

        /// <summary>
        /// 配置取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConfigCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
