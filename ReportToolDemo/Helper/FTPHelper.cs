using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ReportToolDemo.ReportServiceTool.Helper
{
    /// <summary>
    /// FTP辅助类
    /// </summary>
    public class FTPHelper
    {
        public string ftpRemotePath;
        string ftpUserID;
        string ftpPassword;
        public string ftpURI;
        string ftpServerIP;

        public bool isPasvMode = true;

        /// <summary>
        /// 连接FTP
        /// </summary>
        /// <param name="FtpServerIP">FTP连接地址</param>
        /// <param name="FtpRemotePath">指定FTP连接成功后的当前目录, 如果不指定即默认为根目录</param>
        /// <param name="FtpUserID">用户名</param>
        /// <param name="FtpPassword">密码</param>
        public FTPHelper(string FtpServerIP, string FtpPort, string FtpUserID, string FtpPassword, string FtpRemotePath, bool pIsPasvMode)
        {
            ftpServerIP = FtpServerIP;
            ftpUserID = FtpUserID;
            ftpPassword = FtpPassword;
            ftpURI = "ftp://" + ftpServerIP + ":" + FtpPort + "/";
            ftpRemotePath = FtpRemotePath;
            isPasvMode = pIsPasvMode;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="pFtpUri">FTP地址</param>
        /// <param name="pFileName">文件名称</param>
        /// <param name="pType">服务名称</param>
        public bool Upload2(string pFtpUri, string pFileName, string pType)
        {
            string strFtpUri = pFtpUri;

            try
            {
                FileInfo fileInf = new FileInfo(pFileName);
                FtpWebRequest reqFTP;
                reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(strFtpUri));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = isPasvMode;
                reqFTP.Timeout = 600000;
                reqFTP.ReadWriteTimeout = 600000;
                reqFTP.ContentLength = fileInf.Length;
                int buffLength = 2048;
                byte[] buff = new byte[buffLength];
                int contentLen;
                FileStream fs = fileInf.OpenRead();

                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }

                strm.Close();
                fs.Close();

                frmMain.ins.Value.PrintLog(string.Format("上传文件：{0}至FTP服务器：{1}，成功。", pFileName, pFtpUri), pType);
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("上传文件：{0}至FTP服务器：{1}出现异常：{2}。", pFileName, pFtpUri, ex.Message), pType);
                return false;
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="pFtpUri">FTP文件地址</param>
        /// <param name="pFilePath">文件路径</param>
        /// <param name="pFileName">文件名称</param>
        /// <param name="pType">服务名称</param>
        /// <returns>返回下载结果</returns>
        public bool Download(string pFtpUri, string pFilePath, string pFileName, string pType)
        {
            try
            {
                FileStream objFileStream = new FileStream(pFilePath + "\\" + pFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(pFtpUri));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.KeepAlive = false;
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = isPasvMode;
                reqFTP.Timeout = 600000;
                reqFTP.ReadWriteTimeout = 600000;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    objFileStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }

                ftpStream.Close();
                response.Close();
                objFileStream.Close();

                frmMain.ins.Value.PrintLog(string.Format("下载FTP报文：{0}至本地备份文件：{1}成功。", pFtpUri, pFilePath + "\\" + pFileName), pType);
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("下载报文：{0}失败，出现异常：{1}", pFtpUri, ex.Message), pType);
                return false;
            }
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="pFtpUri">FTP文件地址</param>
        /// <param name="pType">服务名称</param>
        /// <param name="isDirect">是否直连</param>
        public bool Delete(string pFtpUri, string pType, bool isDirect)
        {
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(pFtpUri));
                reqFTP.UsePassive = isPasvMode;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                reqFTP.Timeout = 600000;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;

                string strResult = String.Empty;
                FtpWebResponse objResponse = (FtpWebResponse)reqFTP.GetResponse();
                Stream objDataStream = objResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objDataStream);
                strResult = objReader.ReadToEnd();

                objReader.Close();
                objDataStream.Close();
                objResponse.Close();

                frmMain.ins.Value.PrintLog(string.Format("{0}删除报文：{1}成功。", isDirect ? "[直连]-" : "", pFtpUri), pType);
                return true;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("{0}删除报文：{1}失败，出现异常：{2}", isDirect ? "[直连]-" : "", pFtpUri, ex.Message), pType);
                return false;
            }
        }

        /// <summary>
        /// 获取当前目录下文件列表(仅文件)
        /// </summary>
        /// <param name="pType">服务名称</param>
        /// <returns>返回文件列表</returns>
        public string[] GetFileList(string pMask, string pType)
        {
            StringBuilder strResult = new StringBuilder();
            WebResponse objResponse = null;
            StreamReader objReader = null;
            try
            {
                string sTemp = ftpRemotePath.Substring(ftpRemotePath.LastIndexOf(@"/") + 1) + @"/";
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(ftpURI + ftpRemotePath));
                reqFTP.UseBinary = true;
                reqFTP.UsePassive = isPasvMode;
                reqFTP.Timeout = 600000;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                objResponse = reqFTP.GetResponse();
                objReader = new StreamReader(objResponse.GetResponseStream(), Encoding.Default);

                string strLine = objReader.ReadLine();
                while (strLine != null)
                {
                    if (pMask.Trim() != string.Empty && pMask.Trim() != "*.*")
                    {

                        string mask_ = pMask.Substring(0, pMask.IndexOf("*"));
                        if (strLine.Substring(0, mask_.Length) == mask_)
                        {
                            strResult.Append(strLine);
                            strResult.Append("\n");
                        }
                    }
                    else
                    {
                        strResult.Append(strLine);
                        strResult.Append("\n");
                    }
                    strLine = objReader.ReadLine();
                }

                int iStartIndex = strResult.ToString().LastIndexOf('\n');
                if (iStartIndex >= 0)
                {
                    strResult.Remove(iStartIndex, 1);
                }

                return strResult.ToString().Replace(sTemp, string.Empty).Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("获取当前FTP服务器目录{0}下文件列表出现异常：{1}", ftpURI + ftpRemotePath, ex.Message), pType);
                return null;
            }
            finally
            {
                if (objReader != null)
                {
                    objReader.Close();
                }
                if (objResponse != null)
                {
                    objResponse.Close();
                }
            }
        }

        /// <summary>  
        /// 列出FTP服务器上面当前目录的所有文件和目录  
        /// </summary>  
        /// <param name="pFtpUri">FTP目录</param>  
        /// <returns>返回所有文件和目录</returns>  
        public List<FileStruct> ListFilesAndDirectories(string pFtpUri)
        {
            WebResponse webresp = null;
            StreamReader objReader = null;
            try
            {
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(pFtpUri));
                ftpRequest.UsePassive = isPasvMode;
                ftpRequest.Timeout = 600000;
                ftpRequest.KeepAlive = false;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                ftpRequest.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                webresp = ftpRequest.GetResponse();
                objReader = new StreamReader(webresp.GetResponseStream(), Encoding.Default);
            }
            catch (Exception ex)
            {
                throw new Exception("FTP服务器目录：" + pFtpUri + "，获取文件列表出错，错误信息如下：" + ex.Message);
            }

            string Datastring = objReader.ReadToEnd();
            return GetList(Datastring);
        }

        /// <summary>
        /// 验证文件夹是否存在
        /// </summary>
        /// <param name="pFtpUri">FTP地址</param>
        /// <param name="pDicName">文件夹名称</param>
        /// <returns>存在返回True,否则返回False</returns>
        public bool CheckDicExist(string pFtpUri, string pDicName)
        {
            List<FileStruct> ListFilesAndDirectories = this.ListFilesAndDirectories(pFtpUri);
            foreach (FileStruct itm in ListFilesAndDirectories)
            {
                if (!itm.IsDirectory)
                {
                    continue;
                }

                if (itm.Name.Trim() == pDicName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>  
        /// 获得文件和目录列表  
        /// </summary>  
        /// <param name="pDataString">FTP返回的列表字符信息</param>  
        /// <returns>返回文件和目录列表</returns>
        private List<FileStruct> GetList(string pDataString)
        {
            List<FileStruct> lstFileStruct = new List<FileStruct>();
            string[] dataRecords = pDataString.Split('\n');
            FileListStyle objFileStyle = GuessFileListStyle(dataRecords);
            foreach (string itm in dataRecords)
            {
                if (objFileStyle != FileListStyle.Unknown && itm != "")
                {
                    FileStruct objFile = new FileStruct();
                    objFile.Name = "..";
                    switch (objFileStyle)
                    {
                        case FileListStyle.UnixStyle:
                            objFile = ParseFileStructFromUnixStyleRecord(itm);
                            break;
                        case FileListStyle.WindowsStyle:
                            objFile = ParseFileStructFromWindowsStyleRecord(itm);
                            break;
                    }

                    if (!(objFile.Name == "." || objFile.Name == ".."))
                    {
                        lstFileStruct.Add(objFile);
                    }
                }
            }

            return lstFileStruct;
        }

        /// <summary>  
        /// 从Unix格式中返回文件信息  
        /// </summary>  
        /// <param name="pRecord">文件信息</param>  
        /// <returns>返回文件信息</returns>
        private FileStruct ParseFileStructFromUnixStyleRecord(string pRecord)
        {
            string strProcess = pRecord.Trim();
            FileStruct objStruct = new FileStruct();
            objStruct.Flags = strProcess.Substring(0, 10);
            objStruct.IsDirectory = (objStruct.Flags[0] == 'd');
            strProcess = (strProcess.Substring(11)).Trim();

            //跳过一部分  
            CutStringWithTrim(ref strProcess, ' ', 0);
            objStruct.Owner = CutStringWithTrim(ref strProcess, ' ', 0);
            objStruct.Group = CutStringWithTrim(ref strProcess, ' ', 0);
            //跳过一部分  
            CutStringWithTrim(ref strProcess, ' ', 0);
            string strTime = strProcess.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[2];
            if (strTime.IndexOf(":") >= 0)  //time  
            {
                strProcess = strProcess.Replace(strTime, DateTime.Now.Year.ToString());
            }
            objStruct.CreateTime = DateTime.Parse(CutStringWithTrim(ref strProcess, ' ', 8));
            objStruct.Name = strProcess;
            return objStruct;
        }

        /// <summary>  
        /// 从Windows格式中返回文件信息  
        /// </summary>  
        /// <param name="pRecord">文件信息</param>  
        /// <returns>返回文件信息</returns>
        private FileStruct ParseFileStructFromWindowsStyleRecord(string pRecord)
        {
            string strProcess = pRecord.Trim();
            FileStruct objStruct = new FileStruct();
            string strData = strProcess.Substring(0, 8);
            strProcess = (strProcess.Substring(8, strProcess.Length - 8)).Trim();
            string strTime = strProcess.Substring(0, 7);
            strProcess = (strProcess.Substring(7, strProcess.Length - 7)).Trim();
            DateTimeFormatInfo objFormat = new CultureInfo("en-US", false).DateTimeFormat;
            objFormat.ShortTimePattern = "t";
            objStruct.CreateTime = DateTime.Parse(strData + " " + strTime, objFormat);
            if (strProcess.Substring(0, 5) == "<DIR>")
            {
                objStruct.IsDirectory = true;
                strProcess = (strProcess.Substring(5, strProcess.Length - 5)).Trim();
            }
            else
            {
                string[] strs = strProcess.Split(new char[] { ' ' }, 2);
                strProcess = strs[1];
                objStruct.IsDirectory = false;
            }

            objStruct.Name = strProcess;
            return objStruct;
        }

        /// <summary>  
        /// 按照一定的规则进行字符串截取  
        /// </summary>  
        /// <param name="pCut">截取的字符串</param>  
        /// <param name="pQuery">查找的字符</param>  
        /// <param name="pStartIndex">查找的位置</param>  
        /// <returns>返回截取字符串</returns>
        private string CutStringWithTrim(ref string pCut, char pQuery, int pStartIndex)
        {
            int intPost = pCut.IndexOf(pQuery, pStartIndex);
            string strResult = pCut.Substring(0, intPost);
            pCut = (pCut.Substring(intPost)).Trim();
            return strResult;
        }

        /// <summary>  
        /// 判断文件列表的方式Window方式还是Unix方式  
        /// </summary>  
        /// <param name="pRecordList">文件信息列表</param>  
        /// <returns>返回文件列表方式</returns>
        private FileListStyle GuessFileListStyle(string[] pRecordList)
        {
            foreach (string itm in pRecordList)
            {
                if (itm.Length > 10 && Regex.IsMatch(itm.Substring(0, 10), "(-|d)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)(-|r)(-|w)(-|x)"))
                {
                    return FileListStyle.UnixStyle;
                }
                else if (itm.Length > 8 && Regex.IsMatch(itm.Substring(0, 8), "[0-9][0-9]-[0-9][0-9]-[0-9][0-9]"))
                {
                    return FileListStyle.WindowsStyle;
                }
            }

            return FileListStyle.Unknown;
        }

        /// <summary>
        /// 创建文件夹
        /// </summary>
        /// <param name="pDirName">文件夹名称</param>
        /// <param name="pType">服务名称</param>
        public void MakeDir(string pDirName, string pType)
        {
            string strDirName = this.ftpURI + ftpRemotePath + @"/" + pDirName;
            try
            {
                FtpWebRequest reqFTP = (FtpWebRequest)WebRequest.Create(new Uri(strDirName));
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.UsePassive = isPasvMode;
                reqFTP.Timeout = 600000;
                reqFTP.UseBinary = true;
                reqFTP.KeepAlive = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("创建FTP服务器文件夹{0}，出现异常：{1}", strDirName, ex.Message), pType);
            }
        }

        #region 文件信息结构  
        public struct FileStruct
        {
            public string Flags;
            public string Owner;
            public string Group;
            public bool IsDirectory;
            public DateTime CreateTime;
            public string Name;
        }
        public enum FileListStyle
        {
            UnixStyle,
            WindowsStyle,
            Unknown
        }
        #endregion
    }
}