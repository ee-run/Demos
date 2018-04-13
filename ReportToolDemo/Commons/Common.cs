using ReportToolDemo.ReportServiceTool.Helper;
using System.Windows.Forms;

namespace ReportToolDemo.ReportServiceTool.Commons
{
    public enum OrgType
    {
        /// <summary>
        /// 海关
        /// </summary>
        CIQ,

        /// <summary>
        /// 国检
        /// </summary>
        CCK,

        /// <summary>
        /// 区内监管
        /// </summary>
        IRS,

        /// <summary>
        /// 特殊推送
        /// </summary>
        SPE,

        /// <summary>
        /// MSMQ消息队列
        /// </summary>
        MSMQ
    }

    public static class Common
    {
        /// <summary>
        /// 弹出提示信息框
        /// </summary>
        /// <param name="Info"></param>
        public static void ShowPromptInfo(string Info)
        {
            MessageBox.Show(Info, "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        /// <summary>
        /// 弹出错误信息框
        /// </summary>
        /// <param name="Info"></param>
        public static void ShowErrorInfo(string Info)
        {
            LogHelper.WriteLog(Info);
            MessageBox.Show(Info, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// 显示提示确认信息框
        /// </summary>
        /// <param name="Info"></param>
        public static DialogResult ShowConfirmInfo(string Info)
        {
            return MessageBox.Show(Info, "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// 根据类型获取名称
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetOrgTypeNameByCode(OrgType type)
        {
            string sName = string.Empty;
            switch (type)
            {
                case OrgType.CIQ:
                    sName = "海关服务";
                    break;
                case OrgType.CCK:
                    sName = "国检服务";
                    break;
                case OrgType.IRS:
                    sName = "区内监管服务";
                    break;
                case OrgType.SPE:
                    sName = "特殊文件推送服务";
                    break;
                case OrgType.MSMQ:
                    sName = "MSMQ文件推送服务";
                    break;
                default:
                    sName = "未知服务";
                    break;
            }

            return sName;
        }

        /// <summary>
        /// 根据类型获取名称
        /// </summary>
        /// <param name="pOrgType">服务类型</param>
        /// <returns>返回服务名称</returns>
        public static string GetOrgTypeName(string pOrgType)
        {
            string strOrgName = string.Empty;
            switch (pOrgType)
            {
                case "CIQ":
                    strOrgName = "海关服务";
                    break;
                case "CCK":
                    strOrgName = "国检服务";
                    break;
                case "IRS":
                    strOrgName = "区内监管服务";
                    break;
                case "SPE":
                    strOrgName = "特殊文件推送服务";
                    break;
                case "MSMQ":
                    strOrgName = "MSMQ文件推送服务";
                    break;
                default:
                    strOrgName = "未知服务";
                    break;
            }

            return strOrgName;
        }
    }
}