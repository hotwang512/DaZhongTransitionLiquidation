using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaZhongTransitionLiquidation.Common
{
    public static class FileHelper
    {
        #region  Methods

        /// <summary>
        ///     向文本文件的尾部追加内容
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="content">写入的内容</param>
        public static void AppendText(string filePath, string content)
        {
            File.AppendAllText(filePath, content);
        }


        /// <summary>
        ///     清空指定目录下所有文件及子目录,但该目录依然保存.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static void ClearDirectory(string directoryPath)
        {
            if (IsExistDirectory(directoryPath))
            {
                //删除目录中所有的文件
                var filePaths = GetFileNames(directoryPath);
                foreach (var filePath in filePaths)
                    DeleteFile(filePath);

                //删除目录中所有的子目录
                var directoryPaths = GetDirectories(directoryPath);
                foreach (var oneDirectoryPath in directoryPaths)
                    DeleteDirectory(oneDirectoryPath);
            }
        }


        /// <summary>
        ///     清空文件内容
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static void ClearFile(string filePath)
        {
            //删除文件
            File.Delete(filePath);

            //重新创建该文件
            CreateFile(filePath);
        }


        /// <summary>
        ///     检测指定目录中是否存在指定的文件,若要搜索子目录请使用重载方法.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">
        ///     模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        ///     范例："Log*.xml"表示搜索所有以Log开头的Xml文件。
        /// </param>
        public static bool Contains(string directoryPath, string searchPattern)
        {
            try
            {
                //获取指定的文件列表
                var fileNames = GetFileNames(directoryPath, searchPattern, false);

                //判断指定文件是否存在
                return fileNames.Length != 0;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return false;
            }
        }

        /// <summary>
        ///     检测指定目录中是否存在指定的文件
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">
        ///     模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        ///     范例："Log*.xml"表示搜索所有以Log开头的Xml文件。
        /// </param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static bool Contains(string directoryPath, string searchPattern, bool isSearchChild)
        {
            try
            {
                //获取指定的文件列表
                var fileNames = GetFileNames(directoryPath, searchPattern, true);

                //判断指定文件是否存在
                return fileNames.Length != 0;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return false;
            }
        }


        /// <summary>
        ///     将源文件的内容复制到目标文件中
        /// </summary>
        /// <param name="sourceFilePath">源文件的绝对路径</param>
        /// <param name="destFilePath">目标文件的绝对路径</param>
        public static void Copy(string sourceFilePath, string destFilePath)
        {
            File.Copy(sourceFilePath, destFilePath, true);
        }


        /// <summary>
        ///     将一个目录的文件拷贝到另外一个目录去
        /// </summary>
        /// <param name="srcPath">源目录</param>
        /// <param name="aimPath">目标目录</param>
        /// <param name="flag">是否允许覆盖同名文件</param>
        /// <param name="version">当出现多次调用此方法的时候，能够重命名多次，比如xxx.1.old,xxx.2.old</param>
        public static void CopyDir(string srcPath, string aimPath, bool flag, int version)
        {
            // 检查目标目录是否以目录分割字符结束如果不是则添加之
            if (aimPath[aimPath.Length - 1] != Path.DirectorySeparatorChar)
                aimPath += Path.DirectorySeparatorChar;

            // 判断目标目录是否存在如果不存在则新建之
            CreateDirectory(aimPath);

            // 得到源目录的文件列表，该里面是包含文件以及目录路径的一个数组
            // 如果你指向copy目标文件下面的文件而不包含目录请使用下面的方法
            var fileList = Directory.GetFileSystemEntries(srcPath);

            // 遍历所有的文件和目录
            foreach (var file in fileList)
                try
                {
                    // 如果这是一个目录，并且不是update目录，递归
                    if (Directory.Exists(file))
                    {
                        var path = Path.GetDirectoryName(file);
                        if (path != null)
                        {
                            var dirName = path.Split('\\');
                            if (dirName[dirName.Length - 1].ToLower() != "update")
                                CopyDir(file, aimPath + Path.GetFileName(file), flag, version);
                        }
                    }

                    // 否则直接Copy文件(不拷贝update.xml文件)
                    else if (!file.ToLower().Contains("update.xml"))
                    {
                        //如果是pdb文件，直接忽略
                        if (file.Contains(".pdb") || file.Contains(".vshost.exe"))
                            continue;
                        //如果文件是dll或者exe文件，则可能是正在使用的文件，不能直接替换
                        if (file.ToLower().Contains(".exe") || file.ToLower().Contains(".dll") ||
                            file.ToLower().Contains(".pdb"))
                        {
                            var exist = false;

                            //正在使用的文件将不能拷贝
                            var onUsedFiles = AppDomain.CurrentDomain.GetAssemblies();


                            foreach (var onUsedFile in onUsedFiles)
                            {
                                var filename = onUsedFile.ManifestModule.Name;

                                //正在使用的文件
                                if (file.Contains(filename))
                                {
                                    //先判断这个文件存在
                                    if (Contains(aimPath, Path.GetFileName(file)))
                                        File.Move(aimPath + Path.GetFileName(file),
                                            aimPath + Path.GetFileName(file) + "." + version + ".old");
                                    File.Copy(file, aimPath + Path.GetFileName(file), true);
                                    exist = true;
                                    break;
                                }
                            }

                            //不是依赖项并且本身不是更新程序本身
                            if (!exist)
                                File.Copy(file, aimPath + Path.GetFileName(file), true);
                            //更新程序自身的更新(单独调试此工程的时候需要用到这一段)
                            /*if (Path.GetFileName(file) == "EastMoney.BPF.DataPlatformUpgrade.exe")
                            {
                                try
                                {
                                    File.Move(aimPath + Path.GetFileName(file),
                                        aimPath + Path.GetFileName(file) + "." + version + ".old");

                                    File.Copy(file, aimPath + Path.GetFileName(file), true);
                                }
                                catch (Exception exception)
                                {
                                                    LogHelper.LogError("Error! ", exception);
                                }
                            }*/
                        }
                        else
                        {
                            File.Copy(file, aimPath + Path.GetFileName(file), true);
                        }
                    }
                }
                catch (Exception exception)
                {
                    LogHelper.WriteLog(exception.Message);
                }
        }


        /// <summary>
        ///     创建一个目录
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        public static void CreateDirectory(string directoryPath)
        {
            //如果目录不存在则创建该目录
            if (!IsExistDirectory(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }


        /// <summary>
        ///     创建一个文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static bool CreateFile(string filePath)
        {
            try
            {
                //如果文件不存在则创建该文件
                if (!IsExistFile(filePath))
                {
                    var fileInfo = new FileInfo(filePath); //创建一个FileInfo对象
                    var fileStream = fileInfo.Create(); //创建文件
                    fileStream.Close(); //关闭文件流
                }
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     创建一个文件,并将字节流写入文件。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="buffer">二进制流数据</param>
        public static bool CreateFile(string filePath, byte[] buffer)
        {
            try
            {
                //如果文件不存在则创建该文件
                if (!IsExistFile(filePath))
                {
                    //创建一个FileInfo对象
                    var file = new FileInfo(filePath);

                    //创建文件
                    var fs = file.Create();

                    //写入二进制流
                    fs.Write(buffer, 0, buffer.Length);

                    //关闭文件流
                    fs.Close();
                }
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return false;
            }
            return true;
        }


        /// <summary>
        ///     删除指定目录及其所有子目录(打开状态下降不能删除)
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static void DeleteDirectory(string directoryPath)
        {
            if (IsExistDirectory(directoryPath))
                try
                {
                    Directory.Delete(directoryPath, true);
                }
                catch (Exception exception)
                {
                    LogHelper.WriteLog(exception.Message);
                }
        }

        /// <summary>
        /// 调用bat删除目录，以防止系统底层的异步删除机制
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static bool DeleteDirectoryWithCmd(string dirPath)
        {
            var process = new Process(); //string path = ...;//bat路径  
            var processStartInfo = new ProcessStartInfo("CMD.EXE", "/C rd /S /Q \"" + dirPath + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            }; //第二个参数为传入的参数，string类型以空格分隔各个参数  
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            if (string.IsNullOrWhiteSpace(output))
                return true;
            return false;
        }

        /// <summary>
        /// 调用bat删除文件，以防止系统底层的异步删除机制
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool DelFileWithCmd(string filePath)
        {
            var process = new Process(); //string path = ...;//bat路径  
            var processStartInfo = new ProcessStartInfo("CMD.EXE", "/C del /F /S /Q \"" + filePath + "\"")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            }; //第二个参数为传入的参数，string类型以空格分隔各个参数  
            process.StartInfo = processStartInfo;
            process.Start();
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            if (output.Contains(filePath))
                return true;
            return false;
        }


        /// <summary>
        ///     删除指定文件
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static void DeleteFile(string filePath)
        {
            if (IsExistFile(filePath))
                File.Delete(filePath);
        }


        /// <summary>
        ///     通过后缀名在目录中查找文件
        /// </summary>
        /// <param name="directory">目录</param>
        /// <param name="pattern">后缀名</param>
        public static void DeleteFiles(DirectoryInfo directory, string pattern)
        {
            if (directory.Exists && pattern.Trim() != string.Empty)
            {
                try
                {
                    foreach (var fileInfo in directory.GetFiles(pattern))
                        fileInfo.Delete();
                }
                catch (Exception exception)
                {
                    LogHelper.WriteLog(exception.Message);
                }
                foreach (var info in directory.GetDirectories())
                    DeleteFiles(info, pattern);
            }
        }


        /// <summary>
        ///     将文件读取到缓冲区中
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static byte[] FileToBytes(string filePath)
        {
            //获取文件的大小 
            var fileSize = GetFileSize(filePath);

            //创建一个临时缓冲区
            var buffer = new byte[fileSize];

            //创建一个文件流
            var fileInfo = new FileInfo(filePath);
            var fileStream = fileInfo.Open(FileMode.Open);

            try
            {
                //将文件流读入缓冲区
                fileStream.Read(buffer, 0, fileSize);

                return buffer;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return null;
            }
            finally
            {
                fileStream.Close(); //关闭文件流
            }
        }


        /// <summary>
        ///     将文件读取到字符串中
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static string FileToString(string filePath)
        {
            return FileToString(filePath, Encoding.Default);
        }

        /// <summary>
        ///     将文件读取到字符串中
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="encoding">字符编码</param>
        public static string FileToString(string filePath, Encoding encoding)
        {
            //创建流读取器
            var reader = new StreamReader(filePath, encoding);
            try
            {
                return reader.ReadToEnd(); //读取流
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                reader.Close(); //关闭流读取器
            }
        }


        /// <summary>
        ///     获取指定目录中所有子目录列表,若要搜索嵌套的子目录列表,请使用重载方法.
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static string[] GetDirectories(string directoryPath)
        {
            try
            {
                return Directory.GetDirectories(directoryPath);
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return null;
            }
        }

        /// <summary>
        ///     获取指定目录及子目录中所有子目录列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">
        ///     模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        ///     范例："Log*.xml"表示搜索所有以Log开头的Xml文件。
        /// </param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static string[] GetDirectories(string directoryPath, string searchPattern, bool isSearchChild)
        {
            try
            {
                return Directory.GetDirectories(directoryPath, searchPattern,
                    isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return null;
            }
        }


        /// <summary>
        ///     从文件的绝对路径中获取扩展名
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static string GetExtension(string filePath)
        {
            var fileInfo = new FileInfo(filePath); //获取文件的名称
            return fileInfo.Extension;
        }


        /// <summary>
        ///     从文件的绝对路径中获取文件名( 包含扩展名 )
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static string GetFileName(string filePath)
        {
            //获取文件的名称
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Name;
        }


        /// <summary>
        ///     从文件的绝对路径中获取文件名( 不包含扩展名 )
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static string GetFileNameNoExtension(string filePath)
        {
            //获取文件的名称
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Name.Split('.')[0];
        }


        /// <summary>
        ///     获取指定目录中所有文件列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static string[] GetFileNames(string directoryPath)
        {
            if (!IsExistDirectory(directoryPath)) //如果目录不存在，则抛出异常
                throw new FileNotFoundException();
            return Directory.GetFiles(directoryPath); //获取文件列表
        }

        /// <summary>
        ///     获取指定目录及子目录中所有文件列表
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        /// <param name="searchPattern">
        ///     模式字符串，"*"代表0或N个字符，"?"代表1个字符。
        ///     范例："Log*.xml"表示搜索所有以Log开头的Xml文件。
        /// </param>
        /// <param name="isSearchChild">是否搜索子目录</param>
        public static string[] GetFileNames(string directoryPath, string searchPattern, bool isSearchChild)
        {
            if (!IsExistDirectory(directoryPath)) //如果目录不存在，则抛出异常
                throw new FileNotFoundException();
            try
            {
                return Directory.GetFiles(directoryPath, searchPattern,
                    isSearchChild ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return null;
            }
        }


        /// <summary>
        ///     获取一个文件的长度,单位为Byte
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static int GetFileSize(string filePath)
        {
            //创建一个文件对象
            var fileInfo = new FileInfo(filePath);

            //获取文件的大小
            return (int)fileInfo.Length;
        }

        /// <summary>
        ///     获取一个文件的长度,单位为KB
        /// </summary>
        /// <param name="filePath">文件的路径</param>
        public static double GetFileSizeByKB(string filePath)
        {
            var fileInfo = new FileInfo(filePath); //创建一个文件对象
            var size = fileInfo.Length / 1024;
            return double.Parse(size.ToString()); //获取文件的大小
        }

        /// <summary>
        ///     获取一个文件的长度,单位为MB
        /// </summary>
        /// <param name="filePath">文件的路径</param>
        public static double GetFileSizeByMB(string filePath)
        {
            var fileInfo = new FileInfo(filePath); //创建一个文件对象
            var size = fileInfo.Length / 1024 / 1024;
            return double.Parse(size.ToString()); //获取文件的大小
        }

        /// <summary>
        ///     得到文件大小
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static long GetFileSizeFromPath(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath)) return -1;
            if (!File.Exists(filePath)) return -1;
            var objFile = new FileInfo(filePath);
            return objFile.Length;
        }


        /// <summary>
        ///     获取文本文件的行数
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static int GetLineCount(string filePath)
        {
            //将文本文件的各行读到一个字符串数组中
            var rows = File.ReadAllLines(filePath);

            //返回行数
            return rows.Length;
        }


        /// <summary>
        ///     检测指定目录是否为空
        /// </summary>
        /// <param name="directoryPath">指定目录的绝对路径</param>
        public static bool IsEmptyDirectory(string directoryPath)
        {
            try
            {
                //判断是否存在文件
                var fileNames = GetFileNames(directoryPath);
                if (fileNames.Length > 0)
                    return false;

                //判断是否存在文件夹
                var directoryNames = GetDirectories(directoryPath);
                return directoryNames.Length <= 0;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        ///     检测指定目录是否存在
        /// </summary>
        /// <param name="directoryPath">目录的绝对路径</param>
        public static bool IsExistDirectory(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }


        /// <summary>
        ///     检测指定文件是否存在,如果存在则返回true。
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        public static bool IsExistFile(string filePath)
        {
            return File.Exists(filePath);
        }


        /// <summary>
        ///     将文件移动到指定目录
        /// </summary>
        /// <param name="sourceFilePath">需要移动的源文件的绝对路径</param>
        /// <param name="descDirectoryPath">移动到的目录的绝对路径</param>
        public static void Move(string sourceFilePath, string descDirectoryPath)
        {
            //获取源文件的名称
            var sourceFileName = GetFileName(sourceFilePath);

            if (IsExistDirectory(descDirectoryPath))
            {
                //如果目标中存在同名文件,则删除
                if (IsExistFile(descDirectoryPath + "\\" + sourceFileName))
                    DeleteFile(descDirectoryPath + "\\" + sourceFileName);
                //将文件移动到指定目录
                File.Move(sourceFilePath, descDirectoryPath + "\\" + sourceFileName);
            }
        }


        /// <summary>
        ///     将流读取到缓冲区中
        /// </summary>
        /// <param name="stream">原始流</param>
        public static byte[] StreamToBytes(Stream stream)
        {
            try
            {
                //创建缓冲区
                var buffer = new byte[stream.Length];

                //读取流
                stream.Read(buffer, 0, int.Parse(stream.Length.ToString()));

                //返回流
                return buffer;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return null;
            }
            finally
            {
                stream.Close(); //关闭流
            }
        }


        /// <summary>
        ///     向文本文件中写入内容,默认UTF8编码
        /// </summary>
        /// <param name="filePath">文件的绝对路径</param>
        /// <param name="content">写入的内容</param>
        /// <param name="encoding"></param>
        public static void WriteText(string filePath, string content, Encoding encoding = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            File.WriteAllText(filePath, content, encoding); //向文件写入内容
        }

        #endregion

        /// <summary>   
        /// 重命名文件夹内的所有子文件夹   ============================
        /// </summary>   
        /// <param name="directoryName">文件夹名称</param>   
        /// <param name="newDirectoryName">新子文件夹名称格式字符串</param>   
        public static bool RenameDirectories(string directoryName, string newDirectoryName)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryName);
            if (!directoryInfo.Exists)
            {
                return false;
            }
            try
            {
                int i = 1;

                //string[] sDirectories = Directory.GetDirectories(directoryName);
                foreach (var sDirectory in directoryInfo.GetDirectories())
                {
                    string sNewDirectoryName = string.Format(newDirectoryName, i++);
                    string sNewDirectory = Path.Combine(directoryName, sNewDirectoryName);
                    sDirectory.MoveTo(sNewDirectory);
                    // Directory.Move(sDirectory, sNewDirectory);
                }
                return true;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
                return false;
            }
        }

        /// <summary>
        /// 文件重命名
        /// </summary>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        public static bool FileRename(string filePath, string newFileName)
        {
            FileInfo fileInfo = new FileInfo(filePath); // 列表中的原始文件全路径名
            if (!fileInfo.Exists)
            {
                return false;
            }
            // fileInfo.DirectoryName 


            // fileInfo.MoveTo(Path.Combine(newStr));// 改名方法

            return true;// 新文件名
        }
        public static string Base64ToImg(string filePath, string base64)
        {
            try
            {
                int len = base64.IndexOf("base64,") + 7;
                int len1 = base64.IndexOf("data:") + 5;
                string ext = base64.Substring(len1, len - len1 - 8);
                base64 = base64.Remove(0, len);
                byte[] arr = Convert.FromBase64String(base64);
                MemoryStream ms = new MemoryStream(arr);
                Bitmap bmp = new Bitmap(ms);
                bmp.Save(filePath, ext.ToLower().Contains("png") ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);
                ms.Close();
                return filePath;
            }
            catch (Exception exception)
            {
                LogHelper.WriteLog(exception.Message);
            }
            return "";
        }
        public static string FileToBase64(string filePath)
        {
            string base64Str = "";
            try
            {
                FileStream filestream = new FileStream(filePath, FileMode.Open);
                byte[] bt = new byte[filestream.Length];
                //调用read读取方法
                filestream.Read(bt, 0, bt.Length);
                base64Str = Convert.ToBase64String(bt);
                filestream.Close();
                return "data:image/png;base64," + base64Str;
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog(ex.Message);
                return "";
            }
        }
    }
}
