using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Services;
using System;

namespace ReportToolDemo.ReportServiceTool.WorkFlows
{
    /// <summary>
    /// 报文服务的主要流程
    /// </summary>
    public class ServiceWorkFlow
    {
        private InfoEntity infoEntity;
        private ReportPullServer pullServer;
        private ReportPushServer pushServer;

        public ServiceWorkFlow(InfoEntity infoEntity)
        {
            this.infoEntity = infoEntity;
            pullServer = new ReportPullServer(this.infoEntity);
            pushServer = new ReportPushServer(this.infoEntity);
        }

        /// <summary>
        /// 服务流程开启
        /// </summary>
        public bool Start()
        {
            try
            {
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

                //备份推送的文件（从来源文件夹中备份到备份文件夹）
                frmMain.ins.Value.PrintLog("开始备份推送目录...", infoEntity.OrgType.ToString());
                bool isBakPushFileSuccess = pushServer.BakPushFiles();
                if (!isBakPushFileSuccess)
                {
                    return false;
                }

                //开始推送到主FTP
                frmMain.ins.Value.PrintLog("开始推送本地文件到主FTP...", infoEntity.OrgType.ToString());
                bool IsPushSuccess = pushServer.Push();
                if (IsPushSuccess)
                {
                    // 删除推送的文件
                    //frmMain.ins.Value.PrintLog("本地报文全部推送成功,删除所有推送的文件...", infoEntity.OrgType.ToString());
                    //pushServer.DeletePushFiles();
                }
                else
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
    }
}