using ReportToolDemo.ReportServiceTool.Helper;
using System;
using System.Data;

namespace ReportToolDemo.ReportServiceTool.Commons
{
    public static class BaseConfig
    {
        public static readonly string IniFilePath = AppDomain.CurrentDomain.BaseDirectory + @"ConfigFiles\ServiceConfig.ini";

        public static readonly string VALUE = "VALUE";

        public static readonly string FTPIP = "FTPIP";
        public static readonly string LoginName = "LoginName";
        public static readonly string Password = "Password";
        public static readonly string FTPPort = "FTPPort";
        public static readonly string SuperviseArea = "SuperviseArea";

        public static readonly string TypeSend = "send";
        public static readonly string TypeReceive = "receive";

        /// <summary>
        /// 推送报文类型 - 2
        /// </summary>
        public static readonly string PushTypeStr = "2";

        #region CIQ 海关

        public static readonly string PullPath_CIQ = "PullPath_CIQ";
        public static readonly string LocalSendPath_CIQ = "LocalSendPath_CIQ";
        public static readonly string LocalSendBakPath_CIQ = "LocalSendBakPath_CIQ";

        public static readonly string PushPath_CIQ = "PushPath_CIQ";
        public static readonly string LocalReceivePath_CIQ = "LocalReceivePath_CIQ";
        public static readonly string LocalReceiveBakPath_CIQ = "LocalReceiveBakPath_CIQ";

        public static readonly string Merchants_CIQ = "Merchants_CIQ";
        public static readonly string SuperviseOrg_CIQ = "SuperviseOrg_CIQ";

        public static readonly string FTPIP_CIQ = "FTPIP_CIQ";
        public static readonly string LoginName_CIQ = "LoginName_CIQ";
        public static readonly string Password_CIQ = "Password_CIQ";
        public static readonly string FTPPort_CIQ = "FTPPort_CIQ";
        public static readonly string DirectFTPPushPath_CIQ = "DirectFTPPushPath_CIQ";
        public static readonly string ConnectType_CIQ = "ConnectType_CIQ";

        #endregion

        #region CCK 国签

        public static readonly string PullPath_CCK = "PullPath_CCK";
        public static readonly string LocalSendPath_CCK = "LocalSendPath_CCK";
        public static readonly string LocalSendBakPath_CCK = "LocalSendBakPath_CCK";

        public static readonly string PushPath_CCK = "PushPath_CCK";
        public static readonly string LocalReceivePath_CCK = "LocalReceivePath_CCK";
        public static readonly string LocalReceiveBakPath_CCK = "LocalReceiveBakPath_CCK";

        public static readonly string Merchants_CCK = "Merchants_CCK";
        public static readonly string SuperviseOrg_CCK = "SuperviseOrg_CCK";

        public static readonly string FTPIP_CCK = "FTPIP_CCK";
        public static readonly string LoginName_CCK = "LoginName_CCK";
        public static readonly string Password_CCK = "Password_CCK";
        public static readonly string FTPPort_CCK = "FTPPort_CCK";

        public static readonly string DirectFTPPushPath_CCK = "DirectFTPPushPath_CCK";
        public static readonly string ConnectType_CCK = "ConnectType_CCK";

        #endregion

        #region IRS区内监管

        public static readonly string PullPath_IRS = "PullPath_IRS";
        public static readonly string LocalSendPath_IRS = "LocalSendPath_IRS";
        public static readonly string LocalSendBakPath_IRS = "LocalSendBakPath_IRS";

        public static readonly string PushPath_IRS = "PushPath_IRS";
        public static readonly string LocalReceivePath_IRS = "LocalReceivePath_IRS";
        public static readonly string LocalReceiveBakPath_IRS = "LocalReceiveBakPath_IRS";

        public static readonly string Merchants_IRS = "Merchants_IRS";
        public static readonly string SuperviseOrg_IRS = "SuperviseOrg_IRS";

        public static readonly string FTPIP_IRS = "FTPIP_IRS";
        public static readonly string LoginName_IRS = "LoginName_IRS";
        public static readonly string Password_IRS = "Password_IRS";
        public static readonly string FTPPort_IRS = "FTPPort_IRS";
        public static readonly string DirectFTPPushPath_IRS = "DirectFTPPushPath_IRS";
        public static readonly string ConnectType_IRS = "ConnectType_IRS";

        #endregion

        #region 特殊文件推送
        public static readonly string PullPath_SPE = "PullPath_SPE";
        public static readonly string LocalSendPath_SPE = "LocalSendPath_SPE";
        public static readonly string LocalSendBakPath_SPE = "LocalSendBakPath_SPE";
        #endregion

        #region MSMQ文件推送
        /// <summary>
        /// 配置拉取商家
        /// </summary>
        public static readonly string Merchants_MSMQ = "Merchants_MSMQ";

        /// <summary>
        /// MSMQ推送目录
        /// </summary>
        public static readonly string SuperviseOrg_MSMQ = "SuperviseOrg_MSMQ";
        /// <summary>
        /// MSMQ推送目录
        /// </summary>
        public static readonly string PushPath_MSMQ = "PushPath_MSMQ";

        /// <summary>
        /// MSMQ拉取报文FTP目录
        /// </summary>
        public static readonly string PullDicPath_MSMQ = "PullDicPath_MSMQ";

        /// <summary>
        /// MSMQ消息队列目录（拉取主FTP报文推送至MSMQ目录）
        /// </summary>
        public static readonly string PushMSMQPath_MSMQ = "PushMSMQPath_MSMQ";

        /// <summary>
        /// MSMQ拉取报文存放目录（主FTP拉取报文存放目录，服务轮询推送报文至MSMQ）
        /// </summary>
        public static readonly string StorageDicPath_MSMQ = "StorageDicPath_MSMQ";

        /// <summary>
        /// MSMQ拉取报文存放备份目录（主FTP拉取报文存放备份目录）
        /// </summary>
        public static readonly string PullInfoBak_MSMQ = "PullInfoBak_MSMQ";

        /// <summary>
        /// MSMQ来源目录（服务轮询从来源目录中获取文件推送至主FTP）
        /// </summary>
        public static readonly string OriginDicPath_MSMQ = "OriginDicPath_MSMQ";
        /// <summary>
        /// MSMQ报表备份目录（MSMQ监听服务从消息队列中获取文件存放备份目录）
        /// </summary>
        public static readonly string PushInfoBak_MSMQ = "PushInfoBak_MSMQ";
        /// <summary>
        /// MSMQ消息队列目录（MSMQ监听服务从此队列中获取文件）
        /// </summary>
        public static readonly string PullPath_MSMQ = "PullPath_MSMQ";
        #endregion

        #region 配置文件读取/设置
        /// <summary>
        /// 获取INI配置文件中指定节点的值
        /// </summary>
        /// <param name="sNodeName"></param>
        /// <returns></returns>
        public static string GetNodeValueByName(string sNodeName)
        {
            try
            {
                return IniHelper.ReadIniData(sNodeName, VALUE, string.Empty, IniFilePath);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("获取INI配置文件中指定节点{0}的值出现异常:{1}{2}", sNodeName, ex.Message, Environment.NewLine));
                return string.Empty;
            }
        }

        /// <summary>
        /// 设置INI配置文件中指定节点的值
        /// </summary>
        /// <param name="sNodeName"></param>
        /// <param name="sValue"></param>
        public static void SetNodeValueByName(string sNodeName, string sValue)
        {
            try
            {
                IniHelper.WriteIniData(sNodeName, VALUE, sValue, IniFilePath);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(string.Format("设置INI配置文件中指定节点{0}的值出现异常:{1}{2}", sNodeName, ex.Message, Environment.NewLine));
                return;
            }
        }
        #endregion

        #region 数据库配置脚本定义
        /// <summary>
        /// 商家配置查询SQL语句
        /// </summary>
        public static readonly string SQL_Merchants = "select customer_company_name from customer where customer_status = 2";
        #endregion

        #region 数据库配置
        ///// <summary>
        ///// 获取数据库配置信息
        ///// </summary>
        ///// <param name="sNodeName"></param>
        ///// <returns></returns>
        //public static string GetConfigValueByDB(string pSQL)
        //{
        //    try
        //    {
        //        string strValue = string.Empty;
        //        DataTable dttResult = MySqlDbHelper.ExecuteDataTable(pSQL);
        //        foreach (DataRow drTmp in dttResult.Rows)
        //        {
        //            strValue += drTmp[0].ToString() + ",";
        //        }

        //        strValue = strValue.Trim(',');
        //        return strValue;
        //    }
        //    catch (Exception ex)
        //    {
        //        LogHelper.WriteLog(string.Format("数据库中读取商家配置信息失败，异常信息：{0}", ex.Message));
        //        return string.Empty;
        //    }
        //}
        #endregion
    }
}