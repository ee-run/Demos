using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportToolDemo.ReportServiceTool.Helper
{
    /// <summary>
    /// 文件辅助类
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// 编码方式
        /// </summary>
        private static readonly Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// 获取指定文件夹下的所有文件名称集合（带路径）
        /// </summary>
        /// <param name="pDirectoryPath">文件夹路径</param>
        /// <param name="pType">服务类型</param>
        /// <returns>返回所有文件名称列表</returns>
        public static List<string> GetAllFilesFromDic(string pDirectoryPath, string pType)
        {
            try
            {
                if (!Directory.Exists(pDirectoryPath))
                {
                    Directory.CreateDirectory(pDirectoryPath);
                }

                List<string> lstFileName = new List<string>();
                List<string> lstTmpFile = GetAllFile(pDirectoryPath);
                foreach (string itm in lstTmpFile)
                {
                    if (!File.Exists(itm))
                    {
                        continue;
                    }

                    lstFileName.Add(itm);
                }

                frmMain.ins.Value.PrintLog(string.Format("从来源目录：{0}，获取待推送文件成功，待推送文件总数：{1}。", pDirectoryPath, lstFileName.Count), pType);
                return lstFileName;
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("从来源目录：{0}，获取待推送文件出现异常：{1}。", pDirectoryPath, ex.Message), pType);
                return null;
            }
        }

        /// <summary>
        /// 复制文件夹（及文件夹下所有子文件夹和文件）
        /// </summary>
        /// <param name="pSourcePath">待复制的文件夹路径</param>
        /// <param name="pDestinationPath">目标路径</param>
        public static void CopyDirectory(string pSourcePath, string pDestinationPath)
        {
            if (string.IsNullOrWhiteSpace(pSourcePath) || string.IsNullOrWhiteSpace(pDestinationPath))
            {
                return;
            }

            if (!Directory.Exists(pSourcePath))
            {
                Directory.CreateDirectory(pSourcePath);
            }

            DirectoryInfo objDirInfo = new DirectoryInfo(pSourcePath);
            Directory.CreateDirectory(pDestinationPath);
            foreach (FileSystemInfo itm in objDirInfo.GetFileSystemInfos())
            {
                string strFileName = Path.Combine(pDestinationPath, itm.Name);

                if (itm is FileInfo)
                {
                    //文件对象为文件直接复制文件
                    File.Copy(itm.FullName, strFileName, true);
                }
                else
                {
                    //文件对象为文件夹则首先创建文件夹，然后递归复制文件
                    Directory.CreateDirectory(strFileName);
                    CopyDirectory(itm.FullName, strFileName);
                }
            }
        }

        /// <summary>
        /// 删除文件夹下所有子文件夹和文件
        /// </summary>
        /// <param name="pDirectoryPath">待删除的文件夹路径</param>
        public static void DeleteFolderFiles(string pDirectoryPath)
        {
            foreach (string itm in Directory.GetFileSystemEntries(pDirectoryPath))
            {
                if (File.Exists(itm))
                {
                    FileInfo objFileInfo = new FileInfo(itm);
                    if (objFileInfo.Attributes.ToString().IndexOf("ReadOnly", StringComparison.Ordinal) != -1)
                    {
                        objFileInfo.Attributes = FileAttributes.Normal;
                    }

                    //删除文件    
                    File.Delete(itm);
                }
                else
                {
                    //删除文件夹 
                    DeleteFolderFiles(itm);
                }
            }
        }

        /// <summary>
        /// 删除单个文件
        /// </summary>
        /// <param name="pFileName">待删除的文件</param>
        /// <param name="pType">服务类型</param>
        public static void DeleteFiles(string pFileName, string pType)
        {
            try
            {
                if (!File.Exists(pFileName))
                {
                    frmMain.ins.Value.PrintLog(string.Format("删除本地文件{0}失败，文件不存在。", pFileName), pType);
                    return;
                }

                FileInfo objFileInfo = new FileInfo(pFileName);
                if (objFileInfo.Attributes.ToString().IndexOf("ReadOnly", StringComparison.Ordinal) != -1)
                {
                    objFileInfo.Attributes = FileAttributes.Normal;
                }

                //删除文件    
                File.Delete(pFileName);
                frmMain.ins.Value.PrintLog(string.Format("删除本地文件{0}成功。", pFileName), pType);
            }
            catch (Exception ex)
            {
                frmMain.ins.Value.PrintLog(string.Format("删除本地文件{0}，出现异常：{1}。", pFileName, ex.Message), pType);
            }
        }

        /// <summary>
        /// 递归获取路径中所有文件
        /// </summary>
        /// <param name="pPath">路径</param>
        /// <returns>返回文件列表</returns>
        public static List<string> GetAllFile(string pPath)
        {
            List<string> lstFileName = new List<string>();

            //递归获取
            if (Directory.Exists(pPath))
            {
                string[] aryDirectory = Directory.GetDirectories(pPath);
                if (aryDirectory != null && aryDirectory.Length > 0)
                {
                    foreach (string dir in aryDirectory)
                    {
                        lstFileName.AddRange(GetAllFile(dir));
                    }
                }

                string[] aryFiles = Directory.GetFiles(pPath);
                if (aryFiles != null && aryFiles.Length > 0)
                {
                    lstFileName.AddRange(aryFiles);
                }
            }
            else if (File.Exists(pPath))
            {
                //如果路径是文件，添加到list
                lstFileName.Add(pPath);
            }

            return lstFileName;
        }


        /// <summary> 
        /// 从文件读取 Stream
        /// </summary> 
        public static Stream FileToStream(string fileName)
        {
            // 打开文件 
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[] 
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream 
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
    }
}