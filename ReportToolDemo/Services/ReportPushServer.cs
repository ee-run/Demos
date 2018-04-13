using ReportToolDemo.ReportServiceTool.Commons;
using ReportToolDemo.ReportServiceTool.Entity;
using ReportToolDemo.ReportServiceTool.Helper;
using System;
using System.Collections.Generic;
using System.IO;

namespace ReportToolDemo.ReportServiceTool.Services
{
    public class ReportPushServer
    {
        private InfoEntity infoEntity;
        private FTPHelper pushFtpHelper;

        /// <summary>
        /// 构造函数初始化信息对象
        /// </summary>
        /// <param name="infoEntity"></param>
        public ReportPushServer(InfoEntity infoEntity)
        {
            this.infoEntity = infoEntity;
            InitFtpHelper(infoEntity);
        }

        /// <summary>
        /// 初始化报文推送的ftpHelper
        /// </summary>
        /// <param name="infoEntity"></param>
        private void InitFtpHelper(InfoEntity infoEntity)
        {
            if (infoEntity == null)
            {
                return;
            }

            frmMain.ins.Value.PrintLog(
                string.Format("初始化推送pushFtpHelper: FTP:{0}，登录名：{1}，登录密码：{2}，连接模式：{3}，推送目录：{4}",
                infoEntity.FTPServerIP + ":" + infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, frmMain.isPasv ? "被动" : "主动", infoEntity.PushDicPath),
                infoEntity.OrgType.ToString());
            this.pushFtpHelper = new FTPHelper(infoEntity.FTPServerIP, infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, infoEntity.PushDicPath, frmMain.isPasv);
        }

        /// <summary>
        /// 本地待推送报文文件到主FTP服务器
        /// </summary>
        /// <returns></returns>
        public bool Push()
        {
            if (this.infoEntity == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(infoEntity.PushDicPath) || string.IsNullOrEmpty(infoEntity.OriginDicPath))
            {
                return false;
            }

            try
            {
                //获取本地待推送的所有文件 
                frmMain.ins.Value.PrintLog(string.Format("开始从本地来源目录：{0}，获取所有待推送文件。", infoEntity.OriginDicPath), infoEntity.OrgType.ToString());
                List<string> lstFileName = FileHelper.GetAllFilesFromDic(infoEntity.OriginDicPath, infoEntity.OrgType.ToString());

                if (lstFileName == null)
                {
                    return false;
                }

                string strTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                string strFtpPushDic = string.Format(@"{0}/{1}/{2}/{3}", infoEntity.SuperviseArea, BaseConfig.TypeReceive, infoEntity.SuperviseOrg, strTime);

                //验证FTP是否存在相应文件夹结构
                CheckFtpDicExist(pushFtpHelper, strTime);

                string strFtpUri = pushFtpHelper.ftpURI + pushFtpHelper.ftpRemotePath + @"/" + strTime + @"/";
                bool isUploadAll = true;
                foreach (string itm in lstFileName)
                {
                    if (itm.Trim() == string.Empty)
                    {
                        continue;
                    }

                    FileInfo objFileInfo = new FileInfo(itm);
                    string strUploadFile = strFtpUri + "/" + objFileInfo.Name;

                    frmMain.ins.Value.PrintLog(string.Format("开始推送文件：{0}至FTP服务器：{1}", itm, strUploadFile), infoEntity.OrgType.ToString());
                    this.pushFtpHelper = new FTPHelper(infoEntity.FTPServerIP, infoEntity.FTPPort, infoEntity.FTPLoginName, infoEntity.FTPLoginPwd, strFtpUri, frmMain.isPasv);

                    //推送报文
                    bool isSuccess = pushFtpHelper.Upload2(strUploadFile, itm, infoEntity.OrgType.ToString());
                    if (isSuccess)
                    {
                        frmMain.ins.Value.PrintLog(string.Format("开始删除本地文件{0}。", itm), infoEntity.OrgType.ToString());
                        FileHelper.DeleteFiles(itm, infoEntity.OrgType.ToString());
                    }
                    else
                    {
                        isUploadAll = false;
                    }
                }

                return isUploadAll;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("推送报文到主FTP服务器出现异常：" + ex.Message, infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 验证ftp上是否存在文件夹，不存在则创建
        /// </summary>
        private void CheckFtpDicExist(FTPHelper ftpHelper, string RemoteDirectoryName)
        {
            string strFtpUri = ftpHelper.ftpURI + ftpHelper.ftpRemotePath;
            if (!ftpHelper.CheckDicExist(strFtpUri, RemoteDirectoryName))
            {
                ftpHelper.MakeDir(RemoteDirectoryName, infoEntity.OrgType.ToString());
            }
        }

        /// <summary>
        /// 备份推送文件
        /// </summary>
        /// <returns>返回操作结果</returns>
        public bool BakPushFiles()
        {
            try
            {
                string strTime = string.Format("{0}{1}{2}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Day.ToString().PadLeft(2, '0'));
                string strPushBackPath = string.Format(@"{0}\{1}\{2}\{3}\{4}", infoEntity.PushInfoBakPath, BaseConfig.TypeReceive, infoEntity.SuperviseArea, infoEntity.SuperviseOrg, strTime);

                if (!Directory.Exists(strPushBackPath))
                {
                    Directory.CreateDirectory(strPushBackPath);
                }

                frmMain.ins.Value.PrintLog(string.Format("备份推送目录：{0}中所有文件至备份目录：{1}", this.infoEntity.OriginDicPath, strPushBackPath), infoEntity.OrgType.ToString());
                FileHelper.CopyDirectory(this.infoEntity.OriginDicPath, strPushBackPath);
                frmMain.ins.Value.PrintLog(string.Format("备份推送目录：{0}中所有文件至备份目录：{1}成功。", this.infoEntity.OriginDicPath, strPushBackPath), infoEntity.OrgType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog("备份推送目录文件失败，出现异常：" + ex.Message, infoEntity.OrgType.ToString());
                return false;
            }
        }

        /// <summary>
        /// 删除所推送的文件
        /// </summary>
        /// <returns></returns>
        public bool DeletePushFiles()
        {
            try
            {
                frmMain.ins.Value.PrintLog(string.Format("开始删除推送目录：{0}中所有文件......", infoEntity.OriginDicPath), infoEntity.OrgType.ToString());
                FileHelper.DeleteFolderFiles(this.infoEntity.OriginDicPath);
                frmMain.ins.Value.PrintLog(string.Format("删除推送目录：{0}中所有文件成功。", infoEntity.OriginDicPath), infoEntity.OrgType.ToString());

                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("删除推送目录：{0}中所有文件失败，出现异常：{1}", infoEntity.OriginDicPath, ex.Message), infoEntity.OrgType.ToString());
                return false;
            }
        }
    }
}