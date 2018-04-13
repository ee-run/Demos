using ReportToolDemo.ReportServiceTool.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportToolDemo.ReportServiceTool.Entity
{
    /// <summary>
    /// 直连FTP配置信息类
    /// </summary>
    public class DirectConfigEntity
    {
        /// <summary>
        /// 直连FTP服务器IP
        /// </summary>
        public string DirectFTPIP { get; set; }

        /// <summary>
        /// 直连FTP服务器端口
        /// </summary>
        public string DirectFTPPort { set; get; }

        /// <summary>
        /// 直连FTP服务器登录名
        /// </summary>
        public string DirectFTPLoginName { set; get; }

        /// <summary>
        /// 直连FTP服务器登录密码
        /// </summary>
        public string DirectFTPLoginPwd { set; get; }

        /// <summary>
        /// 是否勾选了直连FTP服务
        /// </summary>
        public bool isServiceChecked = false;

        /// <summary>
        /// 直连FTP的发送存放路径
        /// </summary>
        public string DirectFTPPushPath { set; get; }

        /// <summary>
        /// 连接模式
        /// </summary>
        public string ConnectType { set; get; }

        /// <summary>
        /// 服务类型
        /// </summary>
        public OrgType OrgType { set; get; }
    }
}
