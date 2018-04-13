using ReportToolDemo.ReportServiceTool.Commons;

namespace ReportToolDemo.ReportServiceTool.Helper
{
    /// <summary>
    /// 日志辅助类
    /// </summary>
    public class LogHelper
    {
        public static readonly log4net.ILog objSysLog = log4net.LogManager.GetLogger("SystemLog");
        public static readonly log4net.ILog objCIQLog = log4net.LogManager.GetLogger("CIQInfoLog");
        public static readonly log4net.ILog objCCKLog = log4net.LogManager.GetLogger("CCKInfoLog");
        public static readonly log4net.ILog objIRSLog = log4net.LogManager.GetLogger("IRSInfoLog");
        public static readonly log4net.ILog objSPELog = log4net.LogManager.GetLogger("SPEInfoLog");
        public static readonly log4net.ILog objMSMQLog = log4net.LogManager.GetLogger("MSMQInfoLog");

        /// <summary>
        /// 写入系统日志
        /// </summary>
        /// <param name="pInfoMsg">日志信息</param>
        public static void WriteLog(string pInfoMsg, string pType = "")
        {
            pInfoMsg = string.IsNullOrEmpty(pType) ? pInfoMsg : Common.GetOrgTypeName(pType) + "：" + pInfoMsg;
            switch (pType)
            {
                case "CIQ":
                    if (objCIQLog.IsInfoEnabled)
                    {
                        objCIQLog.Info(pInfoMsg);
                    }
                    break;
                case "CCK":
                    if (objCCKLog.IsInfoEnabled)
                    {
                        objCCKLog.Info(pInfoMsg);
                    }
                    break;
                case "IRS":
                    if (objIRSLog.IsInfoEnabled)
                    {
                        objIRSLog.Info(pInfoMsg);
                    }
                    break;
                case "SPE":
                    if (objSPELog.IsInfoEnabled)
                    {
                        objSPELog.Info(pInfoMsg);
                    }
                    break;
                case "MSMQ":
                    if (objMSMQLog.IsInfoEnabled)
                    {
                        objMSMQLog.Info(pInfoMsg);
                    }
                    break;
                default:
                    if (objSysLog.IsInfoEnabled)
                    {
                        objSysLog.Info(pInfoMsg);
                    }
                    break;
            }
        }
    }
}