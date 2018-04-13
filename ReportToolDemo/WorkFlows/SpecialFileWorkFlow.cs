using ReportToolDemo.ReportServiceTool.Commons;
using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Services;
using System;
using System.IO;

namespace ReportToolDemo.ReportServiceTool.WorkFlows
{
    /// <summary>
    /// 特殊文件推送流程类
    /// </summary>
    public class SpecialFileWorkFlow
    {
        private InfoEntity infoEntity;
        private ReportPullServer pullServer;

        public SpecialFileWorkFlow(InfoEntity infoEntity)
        {
            this.infoEntity = infoEntity;
            pullServer = new ReportPullServer(this.infoEntity);
        }

        /// <summary>
        /// 特殊文件推送服务启动
        /// </summary>
        /// <returns></returns>
        public bool Start()
        {
            try
            {
                if (infoEntity == null || infoEntity.OrgType != OrgType.SPE)
                {
                    frmMain.ins.Value.PrintLog("启动失败，配置实体类为空或者配置服务名称" + infoEntity.OrgType.ToString() + "不正确。", OrgType.SPE.ToString());
                    return false;
                }

                //开始拉取报文文件
                frmMain.ins.Value.PrintLog(string.Format("开始从配置路径：{0}，拉取报文文件...", infoEntity.PullDicPath), infoEntity.OrgType.ToString());
                if (string.IsNullOrEmpty(infoEntity.PullDicPath))
                {
                    frmMain.ins.Value.PrintLog("拉取报文文件失败，配置拉取报文路径为空。", infoEntity.OrgType.ToString());
                    return false;
                }

                //从主FTP服务器拉取报文文件
                bool isPullSuccess = pullServer.Pull(infoEntity.PullDicPath);
                if (!isPullSuccess)
                {
                    return false;
                }

                //开始备份拉取报文文件
                if (string.IsNullOrEmpty(infoEntity.PullBakPath))
                {
                    frmMain.ins.Value.PrintLog("备份报文文件失败，配置备份报文路径为空。", infoEntity.OrgType.ToString());
                    return false;
                }

                //备份拉取报文文件
                bool isBakSuccess = BakFiles();
                if (!isBakSuccess)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("操作出现异常：{0}", ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 备份文件
        /// </summary>
        /// <returns>返回操作结果</returns>
        private bool BakFiles()
        {
            try
            {
                string strDateTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                string sDicPath = infoEntity.PullBakPath + "/" + strDateTime;
                if (!Directory.Exists(sDicPath))
                {
                    Directory.CreateDirectory(sDicPath);
                }

                DirectoryInfo dicInfo = new DirectoryInfo(infoEntity.PullStoragePath);
                if (dicInfo == null)
                {
                    frmMain.ins.Value.PrintLog(string.Format("备份报文文件失败，从报文拉取存放路径：{0}获取文件列表为空", infoEntity.PullStoragePath), infoEntity.OrgType.ToString());
                    return false;
                }

                foreach (FileInfo fileInfo in dicInfo.GetFiles())
                {
                    File.Copy(fileInfo.FullName, sDicPath + "/" + fileInfo.Name, true);
                    frmMain.ins.Value.PrintLog(string.Format("开始把报文{0}备份至路径{1}", fileInfo.FullName, sDicPath + "/" + fileInfo.Name), infoEntity.OrgType.ToString());
                }

                frmMain.ins.Value.PrintLog("备份报文文件成功。", infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("备份报文文件失败，异常原因：{0}", ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }
    }
}
