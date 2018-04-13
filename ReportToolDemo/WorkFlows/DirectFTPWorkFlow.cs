using ReportToolDemo.ReportServiceTool.Commons;
using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Helper;
using ReportToolDemo.ReportServiceTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ReportToolDemo.ReportServiceTool.Helper.FTPHelper;

namespace ReportToolDemo.ReportServiceTool.WorkFlows
{
    /// <summary>
    /// 直连FTP服务器的服务流程
    /// </summary>
    public class DirectFTPWorkFlow
    {
        private InfoEntity infoEntity;
        private ReportPullServer pullServer;
        private ReportPushServer pushServer;

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private DirectFTPWorkFlow() { }

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="infoEntity"></param>
        public DirectFTPWorkFlow(InfoEntity infoEntity)
        {
            this.infoEntity = infoEntity;
            pullServer = new ReportPullServer(this.infoEntity);
            pushServer = new ReportPushServer(this.infoEntity);
        }

        /// <summary>
        ///  服务启动
        ///  1、拉取
        ///    从我们自己的主FTP服务器上指定目录拉取报文后，备份报文，
        ///   然后根据报文的类型【拉取的目录中可获取报文类型】
        ///   （配置报文类型对应的目的目录），将报文发送到配置的FTP指定的目录下。
        ///  2、推送
        ///遍历国检主目录下，所有out文件夹下的文件，将这些文件备份到本地后，推送到主FTP的回执目录下。
        /// </summary>
        public bool Start()
        {
            try
            {
                //拉取报文 -- 从主Ftp上拉取文件到本地备份文件，然后复制到存放文件
                bool bPullSuccess = DirectPull();
                if (!bPullSuccess)
                {
                    return false;
                }

                //// 发送报文到指定的FTP服务器上
                //bool bSend2FTPSuccess = SendFiles2Ftp();
                //if (!bSend2FTPSuccess)
                //{
                //    frmMain.ins.Value.PrintLog(string.Format("{0}直连FTP服务发送报文文件到配置的FTP指定的目录失败。", Common.GetOrgTypeNameByCode(infoEntity.OrgType)));
                //    return false;
                //}

                //将主目录下的所有out文件夹下的文件下载到本地备份文件夹，并复制到存放文件夹
                bool bDownLoadSuccess = DownLoadOutFiles();
                if (!bDownLoadSuccess)
                {
                    return false;
                }

                //直连报文推送
                bool bPushSuccess = pushServer.Push();
                if (bPushSuccess)
                {
                    // 删除推送的文件
                    //frmMain.ins.Value.PrintLog("[直连]-本地报文全部推送成功,删除所有推送的文件...", infoEntity.OrgType.ToString());

                    //推送完毕，删除receive下的文件
                    //pushServer.DeletePushFiles();
                }
                else
                {
                    return false;
                }

                frmMain.ins.Value.PrintLog("[直连]-直连服务操作流程结束。", infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("[直连]-直连服务操作出现异常：" + ex.Message + "。", infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 直连服务拉取报文
        /// </summary>
        /// <returns>返回操作结果</returns>
        private bool DirectPull()
        {
            if (this.infoEntity == null || infoEntity.PullDicPathList == null || infoEntity.PullDicPathList.Count == 0)
            {
                frmMain.ins.Value.PrintLog("[直连]-主FTP服务器指定的报文拉取路径为空。", infoEntity.OrgType.ToString());
                return false;
            }

            try
            {
                foreach (string pullPath in infoEntity.PullDicPathList)
                {
                    DownLoadByDicPath(pullPath);
                }

                frmMain.ins.Value.PrintLog("[直连]-主FTP服务器拉取报文操作流程结束。", infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("[直连]-主FTP服务器拉取报文操作失败，出现异常：" + ex.Message + "。", infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 下载文件夹下的所有文件
        /// </summary>
        /// <param name="pPullPath">报文拉取路径</param>
        public void DownLoadByDicPath(string pPullPath)
        {
            FTPHelper pullFtpHelper = new FTPHelper(infoEntity.FTPServerIP, infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, pPullPath, frmMain.isPasv);

            string strFtpUri = pullFtpHelper.ftpURI + pPullPath;
            List<FileStruct> lstFiles = pullFtpHelper.ListFilesAndDirectories(strFtpUri);
            if (lstFiles == null || lstFiles.Count == 0)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-主FTP服务器路径：{0}获取文件目录列表为空。", strFtpUri), infoEntity.OrgType.ToString());
                return;
            }

            foreach (FileStruct itm in lstFiles)
            {
                if (itm.IsDirectory)
                {
                    DownLoadByDicPath(pPullPath + "/" + itm.Name);
                }
                else
                {
                    string strTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                    string strPullBakPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", infoEntity.PullBakPath, BaseConfig.TypeSend, infoEntity.SuperviseArea, infoEntity.SuperviseOrg, strTime);
                    if (!Directory.Exists(strPullBakPath))
                    {
                        Directory.CreateDirectory(strPullBakPath);
                    }

                    //先下载到本地备份文件夹
                    bool downLoadSuccess = false;
                    bool copySuccess = false;
                    bool deleteSuccess = false;
                    bool pushSuccess = false;
                    string strPullFile = strFtpUri + "/" + itm.Name;
                    string strBackFile = strPullBakPath + "/" + itm.Name;

                    //下载报文到本地备份文件夹
                    downLoadSuccess = pullFtpHelper.Download(strPullFile, strPullBakPath, itm.Name, infoEntity.OrgType.ToString());
                    if (downLoadSuccess)
                    {
                        //删除FTP服务器报文
                        deleteSuccess = pullFtpHelper.Delete(strPullFile, infoEntity.OrgType.ToString(), true);

                        //拷贝本地备份报文到存放文件夹
                        string strStorageFile = infoEntity.PullStoragePath + "/" + itm.Name;
                        copySuccess = CopyFile2PullStoragePath(strStorageFile, strBackFile);
                    }

                    if (copySuccess)
                    {
                        //从存放文件夹推送报文至直连FTP服务
                        string strRemotePath = GetRemotePath(pPullPath);
                        if (string.IsNullOrEmpty(strRemotePath))
                        {
                            continue;
                        }

                        string strFileFullName = string.Format("{0}/{1}", infoEntity.PullStoragePath, itm.Name);
                        var directInfoEntity = infoEntity.DirectConfigEntity;
                        if (directInfoEntity == null)
                        {
                            frmMain.ins.Value.PrintLog("[直连]-直连FTP服务器的配置信息对象为空。", infoEntity.OrgType.ToString());
                            continue;
                        }

                        bool isPasvMode = directInfoEntity.ConnectType.ToLower() == "true";
                        FTPHelper ftpHelper = new FTPHelper(
                            directInfoEntity.DirectFTPIP,
                            directInfoEntity.DirectFTPPort,
                            directInfoEntity.DirectFTPLoginName,
                            infoEntity.DirectConfigEntity.DirectFTPLoginPwd,
                            strRemotePath,
                            isPasvMode);

                        pushSuccess = SendFiles2Ftp(ftpHelper, strRemotePath, strFileFullName);
                    }
                }
            }
        }

        /// <summary>
        /// 下载直连FTP服务器的主目录下的所有out文件夹下的文件
        /// </summary>
        /// <returns></returns>
        private bool DownLoadOutFiles()
        {
            bool isPasvMode = infoEntity.DirectConfigEntity.ConnectType.ToLower() == "true";

            frmMain.ins.Value.PrintLog(
                string.Format("初始化拉取直连ftpHelper: FTP:{0}，登录名：{1}，登录密码：{2}，连接模式：{3}，拉取目录：{4}",
                infoEntity.DirectConfigEntity.DirectFTPIP + ":" + infoEntity.DirectConfigEntity.DirectFTPPort,
                infoEntity.DirectConfigEntity.DirectFTPLoginName,
                infoEntity.DirectConfigEntity.DirectFTPLoginPwd,
                isPasvMode ? "被动" : "主动",
                string.Empty),
                infoEntity.OrgType.ToString());

            FTPHelper ftpHelper = new FTPHelper(infoEntity.DirectConfigEntity.DirectFTPIP,
              infoEntity.DirectConfigEntity.DirectFTPPort, infoEntity.DirectConfigEntity.DirectFTPLoginName,
              infoEntity.DirectConfigEntity.DirectFTPLoginPwd, string.Empty, isPasvMode);

            string strFtpUri = string.Format("ftp://{0}:{1}/", infoEntity.DirectConfigEntity.DirectFTPIP, infoEntity.DirectConfigEntity.DirectFTPPort);
            try
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-开始递归获取直连FTP：{0}根目录文件列表。", strFtpUri), infoEntity.OrgType.ToString());
                List<FileStruct> lstFileName = ftpHelper.ListFilesAndDirectories(strFtpUri);
                if (lstFileName == null || lstFileName.Count == 0)
                {
                    frmMain.ins.Value.PrintLog(string.Format("[直连]-开始递归获取直连FTP：{0}根目录文件列表为空", strFtpUri), infoEntity.OrgType.ToString());
                    return false;
                }

                bool isOutDic = false;
                bool curDownLoadSuccess = false;
                int TotalCount = lstFileName.Where(x => x.IsDirectory == true).Count();
                int downLoadSuccessCount = 0;

                foreach (FileStruct itm in lstFileName)
                {
                    if (!itm.IsDirectory)
                    {
                        continue;
                    }

                    isOutDic = itm.Name.ToLower() == "out" ? true : false;
                    curDownLoadSuccess = DownLoadFilesByDicPath(itm.Name, isOutDic);
                    if (curDownLoadSuccess)
                    {
                        downLoadSuccessCount += 1;
                    }
                }

                if (downLoadSuccessCount != TotalCount)
                {
                    frmMain.ins.Value.PrintLog(
                        string.Format("[直连]-主目录{0}总文件夹{1}，成功下载{2}个文件夹下文件，失败{3}个。",
                        strFtpUri, TotalCount, downLoadSuccessCount, TotalCount - downLoadSuccessCount),
                        infoEntity.OrgType.ToString());
                    return false;
                }

                frmMain.ins.Value.PrintLog(string.Format("[直连]-主目录{0}，成功下载{1}个文件夹下文件。", strFtpUri, TotalCount), infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-下载直连FTP：{0}目录所有Out文件夹内文件失败，出现异常：{1}。", strFtpUri, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 下载指定文件夹下的所有文件
        /// </summary>
        /// <param name="pPath">Ftp文件夹路径</param>
        /// <param name="IsOutDic">是否Out文件夹</param>
        private bool DownLoadFilesByDicPath(string pPath, bool pIsOutDic)
        {
            bool isPasvMode = infoEntity.DirectConfigEntity.ConnectType.ToLower() == "true";
            FTPHelper ftpHelper = new FTPHelper(
                infoEntity.DirectConfigEntity.DirectFTPIP,
              infoEntity.DirectConfigEntity.DirectFTPPort,
              infoEntity.DirectConfigEntity.DirectFTPLoginName,
              infoEntity.DirectConfigEntity.DirectFTPLoginPwd,
              pPath,
              isPasvMode);

            string strFtpUri = string.Format("ftp://{0}:{1}/{2}", infoEntity.DirectConfigEntity.DirectFTPIP, infoEntity.DirectConfigEntity.DirectFTPPort, pPath);
            try
            {
                List<FileStruct> lstFileName = ftpHelper.ListFilesAndDirectories(strFtpUri);
                foreach (FileStruct item in lstFileName)
                {
                    // 是文件夹，则判断该文件夹是否out文件夹
                    if (item.IsDirectory && item.Name.ToLower() == "out")
                    {
                        DownLoadFilesByDicPath(pPath + "/" + item.Name, true);
                        continue;
                    }
                    else if (item.IsDirectory && item.Name.ToLower() != "out")
                    {
                        // 不是out文件夹，则继续往下找有没有out文件夹
                        DownLoadFilesByDicPath(pPath + "/" + item.Name, false);
                        continue;
                    }

                    if (pIsOutDic)
                    {
                        string strTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                        string strPushInfoBakPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", infoEntity.PushInfoBakPath, BaseConfig.TypeReceive, infoEntity.SuperviseArea, infoEntity.SuperviseOrg, strTime);

                        if (!Directory.Exists(strPushInfoBakPath))
                        {
                            Directory.CreateDirectory(strPushInfoBakPath);
                        }

                        bool downLoadSuccess = false;
                        bool copySuccess = false;
                        bool deleteSuccess = false;

                        string strFtpFile = strFtpUri + "/" + item.Name;
                        frmMain.ins.Value.PrintLog(string.Format("[直连]-开始下载直连FTP报文文件：{0}。", strFtpFile), infoEntity.OrgType.ToString());
                        downLoadSuccess = ftpHelper.Download(strFtpFile, strPushInfoBakPath, item.Name, infoEntity.OrgType.ToString());
                        if (downLoadSuccess)
                        {
                            // 下载成功，从备份文件夹中拷贝到存放文件夹
                            deleteSuccess = ftpHelper.Delete(strFtpFile, infoEntity.OrgType.ToString(), true);
                        }

                        if (deleteSuccess)
                        {
                            string strStorageFile = infoEntity.OriginDicPath + "/" + item.Name;
                            string strBackFile = strPushInfoBakPath + "/" + item.Name;
                            copySuccess = CopyFile2PullStoragePath(strStorageFile, strBackFile);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-下载直连FTP：{0}路径内文件失败，出现异常：{1}。", strFtpUri, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 将下载到本地备份文件夹的文件复制到存放文件夹
        /// </summary>
        /// <param name="pSaveFile">存放文件</param>
        /// <param name="pBackFile">备份文件</param>
        /// <returns></returns>
        private bool CopyFile2PullStoragePath(string pSaveFile, string pBackFile)
        {
            try
            {
                if (!File.Exists(pBackFile))
                {
                    frmMain.ins.Value.PrintLog(string.Format("[直连]-本地文件备份至存放路径失败，备份文件：{0}不存在。", pBackFile), infoEntity.OrgType.ToString());
                    return false;
                }

                File.Copy(pBackFile, pSaveFile, true);
                frmMain.ins.Value.PrintLog(string.Format("[直连]-将本地备份文件：{0}，复制到存放文件：{1}成功。", pBackFile, pSaveFile), infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-将本地备份文件：{0}，复制到存放文件：{1}失败，出现异常：{2}。", pBackFile, pSaveFile, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 发送报文文件到配置的FTP指定的目录
        /// </summary>
        /// <returns></returns>
        private bool SendFiles2Ftp()
        {
            try
            {
                // 从本地存放目录上传文件到配置的直连FTP服务器
                DirectConfigEntity directInfoEntity = infoEntity.DirectConfigEntity;
                if (directInfoEntity == null || string.IsNullOrEmpty(infoEntity.PullStoragePath))
                {
                    frmMain.ins.Value.PrintLog(
                        string.Format("[直连]-直接配置信息或者报文拉取存放路径配置为空，无法推送文件。"),
                        infoEntity.OrgType.ToString());
                    return false;
                }

                // 获取本地待推送的所有文件 
                frmMain.ins.Value.PrintLog(
                    string.Format("[直连]-开始从本地配置目录：{0}获取所有报文推送至直连FTP。", infoEntity.PullStoragePath),
                    infoEntity.OrgType.ToString());
                List<string> fileNames = FileHelper.GetAllFilesFromDic(infoEntity.PullStoragePath, infoEntity.OrgType.ToString());

                int intSuccess = 0;
                int intFault = 0;
                if (fileNames == null || fileNames.Count == 0)
                {
                    frmMain.ins.Value.PrintLog(
                       string.Format("[直连]-本地配置目录：{0}获取所有报文为空。", infoEntity.PullStoragePath),
                       infoEntity.OrgType.ToString());
                    return false;
                }

                frmMain.ins.Value.PrintLog(
                  string.Format("[直连]-本地配置目录：{0}推送报文成功，文件总数：{1}。", infoEntity.PullStoragePath, fileNames.Count),
                  infoEntity.OrgType.ToString());
                bool isPasvMode = directInfoEntity.ConnectType.ToLower() == "true";
                FTPHelper ftpHelper = new FTPHelper(directInfoEntity.DirectFTPIP,
                    directInfoEntity.DirectFTPPort, directInfoEntity.DirectFTPLoginName,
                    infoEntity.DirectConfigEntity.DirectFTPLoginPwd, "", isPasvMode);
                foreach (var fileName in fileNames)
                {
                    if (fileName.Trim() == string.Empty || !File.Exists(fileName))
                    {
                        continue;
                    }
                    bool uploadSuccess = false;

                    FileInfo fileinfo = new FileInfo(fileName);
                    string sRemotePath = GetRemotePath(fileinfo.DirectoryName);
                    string sUri = ftpHelper.ftpURI + sRemotePath;
                    string strFTPFileName = sUri + "/" + fileinfo.Name;

                    frmMain.ins.Value.PrintLog(
                        string.Format("[直连]-开始推送本地报文：{0}至直连FTP服务：{1}。", fileName, strFTPFileName),
                        infoEntity.OrgType.ToString());
                    uploadSuccess = ftpHelper.Upload2(strFTPFileName, fileName, infoEntity.OrgType.ToString());
                    if (uploadSuccess)
                    {
                        //上传成功后，删除本地的该文件
                        intSuccess += 1;
                        File.Delete(fileName);
                        frmMain.ins.Value.PrintLog(string.Format("[直连]-删除本地报文：{0}成功。", fileName), infoEntity.OrgType.ToString());
                    }
                    else
                    {
                        intFault += 1;
                    }
                }

                frmMain.ins.Value.PrintLog(
                      string.Format("[直连]-本地配置目录：{0}推送报文至直连FTP服务操作结束，推送成功总数：{1}，推送失败总数：{2}。",
                      infoEntity.PullStoragePath, intSuccess, intFault),
                      infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-推送本地报文至直连FTP出现异常：{0}。", ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 发送单个报文文件到配置的FTP指定的目录
        /// </summary>
        /// <param name="pFtpHelper">FTP直连辅助类</param>
        /// <param name="pRemotePath">直连FTP目录</param>
        /// <param name="pFileFullName">推送报文</param>
        /// <returns>返回操作结果</returns>
        private bool SendFiles2Ftp(FTPHelper pFtpHelper, string pRemotePath, string pFileFullName)
        {
            string strFtpUri = pFtpHelper.ftpURI + pFtpHelper.ftpRemotePath;

            try
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-开始推送报文{0}至直连FTP的指定目录{1}...", pFileFullName, strFtpUri), infoEntity.OrgType.ToString());

                if (pFileFullName.Trim() == string.Empty || !File.Exists(pFileFullName))
                {
                    frmMain.ins.Value.PrintLog(string.Format("[直连]-推送报文{0}至直连FTP失败，报文不存在", pFileFullName), infoEntity.OrgType.ToString());
                    return false;
                }

                bool isSuccess = false;
                FileInfo fileinfo = new FileInfo(pFileFullName);
                isSuccess = pFtpHelper.Upload2(strFtpUri + "/" + fileinfo.Name, pFileFullName, infoEntity.OrgType.ToString());
                if (isSuccess)
                {
                    File.Delete(pFileFullName);
                    frmMain.ins.Value.PrintLog(string.Format("[直连]-删除本地报文{0}成功。", pFileFullName), infoEntity.OrgType.ToString());
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("[直连]-开始推送报文{0}至直连FTP的指定目录{1}失败，出现异常：{2}", pFileFullName, strFtpUri, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 根据类型获取发送路径
        /// </summary>
        /// <param name="pPath">主FTP报文拉取路径</param>
        /// <returns>返回直连FTP推送路径</returns>
        private string GetRemotePath(string pPath)
        {
            try
            {
                if (string.IsNullOrEmpty(pPath))
                {
                    frmMain.ins.Value.PrintLog("[直连]-根据类型获取发送路径,主FTP服务配置拉取路径为空。", infoEntity.OrgType.ToString());
                    return string.Empty;
                }

                string strFileType = pPath.Split(
                    new char[] { '/', '\\' },
                    StringSplitOptions.RemoveEmptyEntries).Last().Replace(@"\", string.Empty).Replace(@"/", string.Empty).Trim();

                string strRemotepath = BaseConfig.GetNodeValueByName(string.Format("DirectSendPath_{0}", strFileType));
                if (string.IsNullOrEmpty(strRemotepath))
                {
                    frmMain.ins.Value.PrintLog("[直连]-根据拉取路径文件夹名，匹配直连FTP推送文件路径为空。", infoEntity.OrgType.ToString());
                    return string.Empty;
                }

                return strRemotepath;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("[直连]-根据拉取路径文件夹名，获取直连FTP推送文件路径失败，出现异常：" + ex.Message + "。", infoEntity.OrgType.ToString());
                return string.Empty;
            }
        }
    }
}
