using ReportToolDemo.ReportServiceTool.Commons;
using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ReportToolDemo.ReportServiceTool.Helper.FTPHelper;

namespace ReportToolDemo.ReportServiceTool.Services
{
    /// <summary>
    /// 报文拉取服务对象
    /// </summary>
    public class ReportPullServer
    {
        private InfoEntity infoEntity; // 界面配置信息对象
        private FTPHelper pullFtpHelper; // 拉取的ftp对象

        /// <summary>
        /// 构造函数初始化信息对象
        /// </summary>
        /// <param name="infoEntity"></param>
        public ReportPullServer(InfoEntity infoEntity)
        {
            this.infoEntity = infoEntity;
            InitFtpHelper(infoEntity);
        }

        /// <summary>
        /// 初始化ftpHelper对象
        /// </summary>
        /// <param name="infoEntity"></param>
        private void InitFtpHelper(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                return;
            }

            frmMain.ins.Value.PrintLog(
                string.Format("初始化拉取pullFtpHelper: FTP:{0}，登录名：{1}，登录密码：{2}，连接模式：{3}，拉取目录：{4}",
                infoEntity.FTPServerIP + ":" + infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, frmMain.isPasv ? "被动" : "主动", infoEntity.PullDicPath),
                infoEntity.OrgType.ToString());
            this.pullFtpHelper = new FTPHelper(infoEntity.FTPServerIP, infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, infoEntity.PullDicPath, frmMain.isPasv);
        }

        /// <summary>
        /// 从主FTP服务器拉取文件（特殊文件推送服务专用）
        /// </summary>
        /// <param name="pPullPath">配置FTP拉取报文路径</param>
        /// <returns>返回操作结果</returns>
        public bool Pull(string pPullPath)
        {
            try
            {
                if (!Directory.Exists(infoEntity.PullStoragePath))
                {
                    Directory.CreateDirectory(infoEntity.PullStoragePath);
                }

                //获取拉取路径下的所有文件
                string strPath = pullFtpHelper.ftpURI + pullFtpHelper.ftpRemotePath;
                frmMain.ins.Value.PrintLog(string.Format("开始从主FTP服务器目录{0}获取文件列表。", strPath), infoEntity.OrgType.ToString());
                List<string> lstFilesName = pullFtpHelper.GetFileList("*.*", infoEntity.OrgType.ToString()).ToList();
                if (lstFilesName == null)
                {
                    return false;
                }

                if (lstFilesName.Count <= 0)
                {
                    frmMain.ins.Value.PrintLog(string.Format("从主FTP服务器目录{0}获取文件列表成功，当前目录为空，报文拉取操作结束。", strPath), infoEntity.OrgType.ToString());
                    return true;
                }

                #region 循环拉取报文
                foreach (var fileName in lstFilesName)
                {
                    if (fileName.Trim() == string.Empty)
                    {
                        continue;
                    }

                    string strFtpUri = pullFtpHelper.ftpURI + pullFtpHelper.ftpRemotePath + "/" + fileName;
                    string strStorageFile = infoEntity.PullStoragePath + @"\" + fileName;

                    //从主FTP服务器指定的目录（拉取路径）上拉取报文
                    frmMain.ins.Value.PrintLog(string.Format("开始从主FTP服务器下载报文：{0}到本地文件：{1}。", strFtpUri, strStorageFile), infoEntity.OrgType.ToString());
                    bool isSuccess = pullFtpHelper.Download(strFtpUri, infoEntity.PullStoragePath, fileName, infoEntity.OrgType.ToString());
                    if (!isSuccess)
                    {
                        continue;
                    }

                    //从主FTP服务器删除该文件
                    frmMain.ins.Value.PrintLog(string.Format("开始从主FTP服务器删除报文：{0}", strFtpUri), infoEntity.OrgType.ToString());
                    pullFtpHelper.Delete(strFtpUri, infoEntity.OrgType.ToString(), false);
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("报文拉取出现异常：" + ex.Message, infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 从主FTP服务器拉取文件
        /// </summary>
        /// <returns></returns>
        public bool Pull()
        {
            try
            {
                foreach (string pullPath in infoEntity.PullDicPathList)
                {
                    DownLoadByDicPath(pullPath);
                    frmMain.ins.Value.PrintLog("从主FTP服务：" + pullFtpHelper.ftpURI + "指定的目录：" + pullPath + "拉取报文操作完毕。", infoEntity.OrgType.ToString());
                }
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("从主FTP服务：" + pullFtpHelper.ftpURI + "拉取报文失败，出现异常：" + ex.Message, infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 下载文件夹下的所有文件
        /// </summary>
        /// <param name="pPath">FTP文件目录</param>
        public void DownLoadByDicPath(string pPath)
        {
            this.pullFtpHelper = new FTPHelper(infoEntity.FTPServerIP, infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, pPath, frmMain.isPasv);

            string strFtpUri = pullFtpHelper.ftpURI + pPath;
            List<FileStruct> fileAndDicList = pullFtpHelper.ListFilesAndDirectories(strFtpUri);

            foreach (FileStruct itm in fileAndDicList)
            {
                if (itm.IsDirectory)
                {
                    DownLoadByDicPath(pPath + "/" + itm.Name);
                }
                else
                {
                    #region 下载报文
                    string strFtpFileName = strFtpUri + "/" + itm.Name;
                    string strTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                    string strPullBakPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", infoEntity.PullBakPath, BaseConfig.TypeSend, infoEntity.SuperviseArea, infoEntity.SuperviseOrg, strTime);
                    if (!Directory.Exists(strPullBakPath))
                    {
                        Directory.CreateDirectory(strPullBakPath);
                    }

                    bool downLoadSuccess = false;
                    bool copySuccess = false;
                    string strBackFileName = strPullBakPath + "/" + itm.Name;
                    string strStorageFileName = infoEntity.PullStoragePath + "/" + itm.Name;

                    //先下载到本地备份文件夹
                    frmMain.ins.Value.PrintLog(string.Format("开始下载报文：{0}至本地备份文件：{1}", strFtpFileName, strBackFileName), infoEntity.OrgType.ToString());
                    downLoadSuccess = pullFtpHelper.Download(strFtpFileName, strPullBakPath, itm.Name, infoEntity.OrgType.ToString());

                    if (downLoadSuccess)
                    {
                        //下载成功，从备份文件夹中拷贝到存放文件夹
                        frmMain.ins.Value.PrintLog(string.Format("开始复制本地备份文件：{0}至存放文件：{1}。", strBackFileName, strStorageFileName), infoEntity.OrgType.ToString());
                        copySuccess = CopyFile2PullStoragePath(strBackFileName, strStorageFileName);
                    }

                    if (copySuccess)
                    {
                        //备份文件拷贝至存放路径成功后，删除FTP报文
                        frmMain.ins.Value.PrintLog(string.Format("开始删除报文：{0}", strFtpFileName), infoEntity.OrgType.ToString());
                        pullFtpHelper.Delete(strFtpFileName, infoEntity.OrgType.ToString(), false);
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        /// 将下载到本地备份文件夹的文件复制到存放文件夹
        /// </summary>
        /// <param name="pBackFile">备份文件</param>
        /// <param name="pStorageFile">存放文件</param>
        /// <returns>返回备份结果</returns>
        private bool CopyFile2PullStoragePath(string pBackFile, string pStorageFile)
        {
            try
            {
                if (!File.Exists(pBackFile))
                {
                    frmMain.ins.Value.PrintLog("备份文件：" + pBackFile + "不存在。", infoEntity.OrgType.ToString());
                    return false;
                }

                if (!Directory.Exists(infoEntity.PullStoragePath))
                {
                    Directory.CreateDirectory(infoEntity.PullStoragePath);
                }

                FileInfo objFileInfo = new FileInfo(pBackFile);
                File.Copy(pBackFile, pStorageFile, true);
                frmMain.ins.Value.PrintLog(string.Format("复制本地备份文件：{0}至存放文件：{1}成功。", pBackFile, pStorageFile), infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("复制本地备份文件：{0}至存放文件：{1}出现异常：{2}。", pBackFile, pStorageFile, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }
    }
}
