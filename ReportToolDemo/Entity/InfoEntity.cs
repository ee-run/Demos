using ReportToolDemo.ReportServiceTool.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportToolDemo.ReportServiceTool.Entity
{
    /// <summary>
    /// 报文拉取所要用的信息对象
    /// </summary>
    public class InfoEntity
    {
        /// <summary>
        /// ftp服务器IP
        /// </summary>
        public string FTPServerIP { set; get; }

        /// <summary>
        /// FTP服务器端口
        /// </summary>
        public string FTPPort { set; get; }

        /// <summary>
        /// ftp登录名
        /// </summary>
        public string FTPLoginName { set; get; }

        /// <summary>
        /// ftp登录密码
        /// </summary>
        public string FTPLoginPwd { set; get; }

        /// <summary>
        /// 报文拉取目录路径
        /// </summary>
        public string PullDicPath { set; get; }

        /// <summary>
        /// 报文拉取路径集合（从多个文件夹中获取）
        /// </summary>
        public List<string> PullDicPathList { set; get; }

        /// <summary>
        /// 报文拉取存放目录路径
        /// </summary>
        public string PullStoragePath { set; get; }

        /// <summary>
        /// 报文拉取备份目录路径
        /// </summary>
        public string PullBakPath { set; get; }

        /// <summary>
        ///  监管区域
        /// </summary>
        public string SuperviseArea { set; get; }

        /// <summary>
        /// 监管机构
        /// </summary>
        public string SuperviseOrg { set; get; }

        /// <summary>
        /// 来源目录
        /// </summary>
        public string OriginDicPath { set; get; }

        /// <summary>
        /// 推送目录
        /// </summary>
        public string PushDicPath { set; get; }

        /// <summary>
        /// 拉取主FTP报文推送至MSMQ目录
        /// </summary>
        public string PushMSMQPath_MSMQ { set; get; }

        /// <summary>
        /// 推送MSMQ获取文件路径
        /// </summary>
        public string PushMSMQPath { set; get; }

        /// <summary>
        /// 推送备份目录
        /// </summary>
        public string PushInfoBakPath { set; get; }

        /// <summary>
        /// 要拉取的商家
        /// </summary>
        public string Merchants { set; get; }

        /// <summary>
        /// 主流程是否勾选
        /// </summary>
        public bool isMainServiceChecked = false;

        /// <summary>
        /// 信息对象类型
        /// </summary>
        public OrgType OrgType { set; get; }

        /// <summary>
        /// 直连配置信息对象
        /// </summary>
        public DirectConfigEntity DirectConfigEntity { set; get; }
    }
}
