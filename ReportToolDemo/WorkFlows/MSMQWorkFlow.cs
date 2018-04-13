using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Helper;
using ReportToolDemo.ReportServiceTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Messaging;
using System.Threading;

namespace ReportToolDemo.ReportServiceTool.WorkFlows
{
    /// <summary>
    /// MSMQ报文服务的主要流程
    /// </summary>
    public class MSMQWorkFlow
    {
        #region 参数配置
        /// <summary>
        /// 信号量
        /// </summary>
        private ManualResetEvent objSignal = new ManualResetEvent(false);

        /// <summary>
        /// 是否正在监听
        /// </summary>
        private bool isListen = false;

        /// <summary>
        /// 配置信息实体类
        /// </summary>
        private InfoEntity infoEntity;

        /// <summary>
        /// 推送报表至主FTP服务
        /// </summary>
        private ReportPushServer pushServer;

        /// <summary>
        /// 拉取主FTP报文
        /// </summary>
        private ReportPullServer pullServer;

        /// <summary>
        /// 拉取报文MSMQ消息队列
        /// </summary>
        private MessageQueue pullMainQueue = null;

        /// <summary>
        /// 接收消息委托
        /// </summary>
        /// <param name="pMessage">MQ消息</param>
        private delegate void DelegateReceiveMsg(object pMessage);
        #endregion

        public MSMQWorkFlow(InfoEntity pInfoEntity)
        {
            this.infoEntity = pInfoEntity;
            pullServer = new ReportPullServer(this.infoEntity);
            pushServer = new ReportPushServer(this.infoEntity);
            isListen = false;
        }

        /// <summary>
        /// 服务流程开启
        /// </summary>
        public bool Start()
        {
            try
            {
                //发送报文至MSMQ
                SendPullToMSMQ();

                //开始拉取报文文件
                frmMain.ins.Value.PrintLog("开始拉取报文文件...", infoEntity.OrgType.ToString());
                if (infoEntity == null || infoEntity.PullDicPathList == null || infoEntity.PullDicPathList.Count == 0)
                {
                    frmMain.ins.Value.PrintLog("拉取报文失败，配置实体类或者报文拉取路径为空。", infoEntity.OrgType.ToString());
                    return false;
                }

                //拉取FTP服务器报文
                bool isPullSuccess = pullServer.Pull();
                if (!isPullSuccess)
                {
                    return false;
                }

                //发送报文至MSMQ
                SendPullToMSMQ();

                //开始推送报文至主FTP
                frmMain.ins.Value.PrintLog("开始推送本地文件到主FTP...", infoEntity.OrgType.ToString());
                bool IsPushSuccess = pushServer.Push();
                if (!IsPushSuccess)
                {
                    return false;
                }

                frmMain.ins.Value.PrintLog("主服务操作流程结束。", infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("推送本地文件到主FTP操作流程出现异常：" + ex.Message + "。", infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 开始发送拉取报文至MSMQ
        /// </summary>
        public bool SendPullToMSMQ()
        {
            try
            {
                //开始初始化消息队列
                frmMain.ins.Value.PrintLog("开始初始化推送报文MSMQ消息队列", infoEntity.OrgType.ToString());
                if (string.IsNullOrEmpty(infoEntity.PushMSMQPath_MSMQ))
                {
                    frmMain.ins.Value.PrintLog("MSMQ消息队列推送配置路径为空。", infoEntity.OrgType.ToString());
                    return false;
                }
             
                if (string.IsNullOrEmpty(infoEntity.PullStoragePath))
                {
                    frmMain.ins.Value.PrintLog("MSMQ消息队列拉取报文存放路径为空。", infoEntity.OrgType.ToString());
                    return false;
                }

                List<string> lstAllFiles = FileHelper.GetAllFilesFromDic(infoEntity.PullStoragePath, infoEntity.OrgType.ToString());
                frmMain.ins.Value.PrintLog(
                         string.Format("获取本地报文路径中:{0}所有文件成功，待推送文件总数：{1}。",
                         infoEntity.PullStoragePath, lstAllFiles.Count),
                         infoEntity.OrgType.ToString());

                MessageQueue objSendMQueue = new MessageQueue(infoEntity.PushMSMQPath_MSMQ);
                for (int i = 0; i < lstAllFiles.Count; i++)
                {
                    try
                    {
                        frmMain.ins.Value.PrintLog(
                            string.Format("开始推送本地报文：{0}至MSMQ队列：{1}。", lstAllFiles[i], infoEntity.PushMSMQPath_MSMQ),
                            infoEntity.OrgType.ToString());

                        MessageQueueTransaction myTransaction = new MessageQueueTransaction();
                        //启动事务
                        myTransaction.Begin();
                        System.Messaging.Message objMessage = new System.Messaging.Message();
                        objMessage.Formatter = new XmlMessageFormatter();
                        objMessage.BodyStream = FileHelper.FileToStream(lstAllFiles[i]);

                        //发送报文至MSMQ
                        objSendMQueue.Send(objMessage, myTransaction);
                        myTransaction.Commit();

                        frmMain.ins.Value.PrintLog(
                            string.Format("推送本地报文：{0}至MSMQ队列：{1}成功。", lstAllFiles[i], infoEntity.PushMSMQPath_MSMQ),
                            infoEntity.OrgType.ToString());

                        //删除已推送文件
                        FileHelper.DeleteFiles(lstAllFiles[i], infoEntity.OrgType.ToString());
                    }
                    catch (Exception ex)
                    {
                        frmMain.ins.Value.PrintLog(
                            string.Format("本地报文：{0}推送至MSMQ队列：{1}失败，异常原因：{2}。",
                            lstAllFiles[i], infoEntity.PushMSMQPath_MSMQ, ex.Message),
                            infoEntity.OrgType.ToString());
                        continue;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(
                    "MSMQ推送消息队列初始化流程出现异常：" + ex.Message + ex.StackTrace, infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 开始拉取MSMQ消息队列
        /// </summary>
        public void StartPullMQueue()
        {
            try
            {
                if (isListen)
                {
                    return;
                }

                //开始初始化消息队列
                frmMain.ins.Value.PrintLog("开始初始化拉取报文MSMQ消息队列", infoEntity.OrgType.ToString());
                if (string.IsNullOrEmpty(infoEntity.PushMSMQPath))
                {
                    frmMain.ins.Value.PrintLog("MSMQ消息队列拉取配置路径为空。", infoEntity.OrgType.ToString());
                    return;
                }

                if (!MessageQueue.Exists(infoEntity.PushMSMQPath))
                {
                    frmMain.ins.Value.PrintLog("MSMQ消息队列拉取配置路径[" + infoEntity.PushMSMQPath + "不存在]。", infoEntity.OrgType.ToString());
                    return;
                }

                pullMainQueue = new MessageQueue(infoEntity.PushMSMQPath);
                isListen = true;

                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveMessage), pullMainQueue);
                Thread.Sleep(100);
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("MSMQ消息队列初始化流程出现异常：" + ex.Message + ex.StackTrace, infoEntity.OrgType.ToString());
                isListen = false;
            }
        }

        /// <summary>
        /// ReceiveCompleted事件的实现
        /// </summary>
        /// <param name="pSource">MQ消息队列原Object</param>
        private void ReceiveMessage(object pSource)
        {
            try
            {
                while (isListen)
                {
                    //启动事务
                    MessageQueueTransaction myTransaction = new MessageQueueTransaction();
                    myTransaction.Begin();
                    Message myMessage = pullMainQueue.Receive(myTransaction);
                    myMessage.Formatter = new XmlMessageFormatter();
                    Stream objBodyStream = myMessage.BodyStream;
                    frmMain.ins.Value.PrintLog("MSMQ消息队列接收消息成功,接收字节长度：" + objBodyStream.Length, infoEntity.OrgType.ToString());
                    object[] aryReceive = new object[2] { objBodyStream, myTransaction };
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SaveMessage), aryReceive);
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("MSMQ消息队列接收消息出现异常：" + ex.Message + ex.StackTrace, infoEntity.OrgType.ToString());
                isListen = false;
            }
        }

        /// <summary>
        /// 接收MQ报文消息
        /// </summary>
        /// <param name="pMessage">MQ报文消息</param>
        private void SaveMessage(object pMessage)
        {
            object[] aryReceive = pMessage as object[];
            string strDateTime = string.Format(
                "{0}{1}{2}",
                DateTime.Now.Year,
                DateTime.Now.Month.ToString().PadLeft(2, '0'),
                DateTime.Now.Day.ToString().PadLeft(2, '0'));
            string strBackDicPath = infoEntity.PushInfoBakPath + @"\" + strDateTime;
            string strOriginDicPath = infoEntity.OriginDicPath + @"\" + strDateTime;
            if (!Directory.Exists(strBackDicPath))
            {
                Directory.CreateDirectory(strBackDicPath);
            }

            if (!Directory.Exists(strOriginDicPath))
            {
                Directory.CreateDirectory(strOriginDicPath);
            }

            string strGuid = Guid.NewGuid().ToString().Replace("-", string.Empty).ToUpper() + ".xml";
            string strFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + strGuid;
            string strBackFileName = strBackDicPath + @"\" + strFileName;
            string strOriginFileName = strOriginDicPath + @"\" + strFileName;
            frmMain.ins.Value.PrintLog(
              string.Format("接收MMSQ队列：{0}报文消息成功，开始写入本地文件：{1}", infoEntity.PushMSMQPath, strBackFileName),
              infoEntity.OrgType.ToString());
            try
            {
                Stream objBodyStream = aryReceive[0] as Stream;
                byte[] aryFileData = new byte[objBodyStream.Length];
                objBodyStream.Read(aryFileData, 0, aryFileData.Length);
                objBodyStream.Seek(0, SeekOrigin.Begin);

                using (FileStream objStream = new FileStream(strBackFileName, FileMode.Create))
                {
                    objStream.Write(aryFileData, 0, aryFileData.Length);
                };

                File.Copy(strBackFileName, strOriginFileName);
                frmMain.ins.Value.PrintLog(
                    string.Format("备份本地文件：{0}至{1}成功。", strBackFileName, strOriginFileName),
                    infoEntity.OrgType.ToString());

                MessageQueueTransaction myTransaction = aryReceive[1] as MessageQueueTransaction;
                myTransaction.Commit();
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("保存MQ报文消息至本地文件：" + strBackFileName + "，出现异常：" + ex.Message, infoEntity.OrgType.ToString());
            }
        }

        /// <summary>
        /// 停止监听MSMQ消息队列
        /// </summary>
        public void StopPullMQueue()
        {
            isListen = false;
            pullMainQueue.Close();
            pullMainQueue.Dispose();
            pullMainQueue = null;
        }
    }
}