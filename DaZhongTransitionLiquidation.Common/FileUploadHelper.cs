using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;
using SyntacticSugar;

namespace DaZhongTransitionLiquidation.Common
{
    public class FileUploadHelper
    {

        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="path">附件网络路径配置文件</param>
        /// <param name="Filedata">附件上传类</param>
        /// <param name="pathName">文件存储名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool UploadFile(string path, System.Web.HttpPostedFileBase Filedata, out string pathName, out string fileName)
        {
            string newFileName = string.Empty;
            //文件存库名称
            pathName = string.Empty;
            //文件名称
            fileName = string.Empty;
            try
            {
                //string rootFilePath = ConfigSugar.GetAppString("UploadPath");
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                    if (System.IO.Directory.Exists(rootFilePath + path))
                    {
                        fileName = Path.GetFileName(Filedata.FileName);
                        var tempName = Path.GetFileNameWithoutExtension(fileName);
                        if (!tempName.IsNullOrEmpty())
                        {
                            tempName = tempName + "_";
                        }
                        newFileName = tempName + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(fileName);
                        pathName = rootFilePath + path + string.Format(@"/{0}", newFileName);
                        //上传附件
                        Filedata.SaveAs(pathName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="path">附件网络路径配置文件</param>
        /// <param name="Filedata">附件上传类</param>
        /// <param name="pathName">文件存储名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool UploadReNameFile(string path, System.Web.HttpPostedFileBase Filedata, out string pathName, out string fileName, out string relativePath)
        {
            string newFileName = string.Empty;
            //文件存库名称
            pathName = string.Empty;
            //文件名称
            fileName = string.Empty;
            //相对路径
            relativePath = string.Empty;
            try
            {
                //string rootFilePath = ConfigSugar.GetAppString("UploadPath");
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                    if (System.IO.Directory.Exists(rootFilePath + path))
                    {
                        fileName = Path.GetFileName(Filedata.FileName);
                        var tempName = Path.GetFileNameWithoutExtension(fileName);
                        if (!tempName.IsNullOrEmpty())
                        {
                            tempName = tempName + "_";
                        }
                        newFileName = tempName + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(fileName);
                        pathName = Path.Combine(rootFilePath, path, newFileName);
                        relativePath = Path.Combine(path, newFileName);
                        //上传附件
                        Filedata.SaveAs(pathName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }
        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="path">附件网络路径配置文件</param>
        /// <param name="Filedata">附件上传类</param>
        /// <param name="pathName">文件存储名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool UploadFile(string path, System.Web.HttpPostedFileBase Filedata, out string pathName, out string fileName, out string relativePath)
        {
            string newFileName = string.Empty;
            //文件存库名称
            pathName = string.Empty;
            //文件名称
            fileName = string.Empty;
            //相对路径
            relativePath = string.Empty;
            try
            {
                //string rootFilePath = ConfigSugar.GetAppString("UploadPath");
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                    if (System.IO.Directory.Exists(rootFilePath + path))
                    {
                        fileName = Path.GetFileName(Filedata.FileName);
                        var tempName = Path.GetFileNameWithoutExtension(fileName);
                        if (!tempName.IsNullOrEmpty())
                        {
                            tempName = tempName + "_";
                        }
                        newFileName = tempName + DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(fileName);
                        pathName = Path.Combine(rootFilePath, path, fileName);
                        relativePath = Path.Combine(path, fileName);
                        //上传附件
                        Filedata.SaveAs(pathName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 验证是否存在路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool IsExistPath(string path)
        {
            try
            {
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }

        /// <summary>
        ///  删除文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool DeleteFile(string path, string fileName)
        {
            try
            {
                // 根目录
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 要删除的文件是否存在
                if (System.IO.Directory.Exists(rootFilePath + path))
                {
                    if (File.Exists(rootFilePath + path + @"\" + fileName))
                    {
                        File.Delete(rootFilePath + path + @"\" + fileName);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }

        /// <summary>
        ///  删除文件
        /// </summary>
        /// <param name="path">文件路径名称</param>
        /// <param name="fileName">文件名称</param>
        /// <param name="directory">文件目录</param>
        /// <returns></returns>
        public bool DeleteFile(string pathName, string fileName, string directory)
        {
            try
            {
                // 根目录
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 要删除的文件是否存在
                if (System.IO.Directory.Exists(rootFilePath + directory))
                {
                    if (File.Exists(rootFilePath + directory + @"\" + pathName))
                    {
                        File.Delete(rootFilePath + directory + @"\" + pathName);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 验证文件是否存在
        /// </summary>
        /// <param name="pathName"></param>
        /// <param name="fileName">文件名</param>
        /// <param name="directory">路径</param>
        /// <returns></returns>
        public static bool IsExistFile(string fileName, string directory)
        {
            // 根目录
            string rootFilePath = HttpContext.Current.Server.MapPath(directory);
            // 文件是否存在
            if (File.Exists(rootFilePath + "\\" + fileName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// 附件上传
        ///Sample Resiger 上传附件不修改文件名称
        /// </summary>
        /// <param name="path">附件网络路径配置文件</param>
        /// <param name="Filedata">附件上传类</param>
        /// <param name="pathName">文件存储名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool UploadFileNoUpdateName(string path, System.Web.HttpPostedFileBase Filedata, out string pathName, out string fileName, out string relativePath, out string rePath)
        {
            string newFileName = string.Empty;
            //文件存库名称
            pathName = string.Empty;
            //文件名称
            fileName = string.Empty;
            //相对路径
            relativePath = string.Empty;
            rePath = string.Empty;
            try
            {
                //string rootFilePath = ConfigSugar.GetAppString("UploadPath");
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                    if (System.IO.Directory.Exists(rootFilePath + path))
                    {
                        fileName = Path.GetFileName(Filedata.FileName);
                        var tempName = Path.GetFileNameWithoutExtension(fileName);

                        newFileName = tempName + Path.GetExtension(fileName);
                        pathName = Path.Combine(rootFilePath, path, newFileName);
                        relativePath = Path.Combine(path, newFileName);
                        rePath = ConfigSugar.GetAppString("UploadPath") + Path.Combine(path, newFileName);
                        //上传附件
                        Filedata.SaveAs(pathName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 附件上传
        /// </summary>
        /// <param name="path">附件网络路径配置文件</param>
        /// <param name="Filedata">附件上传类</param>
        /// <param name="pathName">文件存储名称</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public bool TestPlanUploadFile(string path, System.Web.HttpPostedFileBase Filedata, out string pathName, out string fileName)
        {
            string newFileName = string.Empty;
            //文件存库名称
            pathName = string.Empty;
            //文件名称
            fileName = string.Empty;
            try
            {
                //string rootFilePath = ConfigSugar.GetAppString("UploadPath");
                string rootFilePath = HttpContext.Current.Server.MapPath(ConfigSugar.GetAppString("UploadPath"));
                // 根目录是否存在
                if (System.IO.Directory.Exists(rootFilePath))
                {
                    if (!System.IO.Directory.Exists(rootFilePath + path))
                    {
                        // 创建根目录下子文件夹
                        System.IO.Directory.CreateDirectory(rootFilePath + path);
                    }
                    if (System.IO.Directory.Exists(rootFilePath + path))
                    {
                        fileName = Path.GetFileName(Filedata.FileName);
                        var tempName = Path.GetFileNameWithoutExtension(fileName);

                        newFileName = tempName + Path.GetExtension(fileName);
                        pathName = rootFilePath + path + string.Format(@"/{0}", newFileName);
                        //上传附件
                        Filedata.SaveAs(pathName);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return false;
            }
        }
    }
}
