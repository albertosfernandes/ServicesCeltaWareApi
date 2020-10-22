using Microsoft.Extensions.FileProviders;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using static ServicesCeltaware.BackEnd.Enum;

namespace ServicesCeltaware.BackEnd.Tools
{
    public class CommandWin32
    {
        
        public async static Task<string> ExecuteTeste(string path, string command, string args)
        {
            try
            {
                string fileNameFull = Path.Combine(path, command);
                string message = null;
                string error = null;

                using (Process p1 = new Process())
                {
                    p1.StartInfo.FileName = "\"" + fileNameFull + "\"";
                    p1.StartInfo.Arguments = args;
                    p1.StartInfo.CreateNoWindow = true;
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.RedirectStandardError = true;
                    p1.StartInfo.WorkingDirectory = path;

                    p1.Start();

                    var idProcess = p1.Id;

                    message = p1.StandardOutput.ReadToEnd();
                    error = p1.StandardError.ReadToEnd();
                    p1.WaitForExit((1 * 1000) * 60);
 

                    p1.Close();

                    p1.Dispose();
                }

                if (!String.IsNullOrEmpty(error))
                {
                    message += error;
                }

                return message;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static string Execute(string path, string command, string args)
        {
            try
            {
                string filenameFull = Path.Combine(path, command);
                string message = null;
                string _error = null;
                if (!String.IsNullOrEmpty(command))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = "\"" + filenameFull + "\"";
                        p1.StartInfo.Arguments = args;
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.StartInfo.WorkingDirectory = path;

                        p1.Start();

                        //p1.WaitForExit(120000);
                        if (p1.Start())
                        {
                            p1.WaitForExit();
                            message = p1.StandardOutput.ReadToEnd();
                            _error = p1.StandardError.ReadToEnd();
                        }
                    }
                }
                return message;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

       

        public async static Task<string> ExecuteSynch(string path, string command, string args)
        {
            try
            {
                string filenameFull = Path.Combine(path, command);
                string message = null;
                string _error = null;
                if (!String.IsNullOrEmpty(command))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = "\"" + filenameFull + "\"";
                        p1.StartInfo.Arguments = args;
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.StartInfo.WorkingDirectory = path;

                        var idProcess = p1.Id;

                        message = p1.StandardOutput.ReadToEnd();
                        _error = p1.StandardError.ReadToEnd();
                        p1.WaitForExit((1 * 1000) * 60);


                        p1.Close();

                        p1.Dispose();
                    }
                }
                if (!String.IsNullOrEmpty(_error))
                    message = _error;

                return message;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async static Task<int> ExecuteBatch(string batchPath, string arg)
        {
            try
            {
                string _error = null;
                int exitCode;
                ProcessStartInfo processInfo;
                Process process;
                
                processInfo = new ProcessStartInfo("cmd.exe", "/c " + arg);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;                
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;                
                processInfo.WorkingDirectory = batchPath; 
                process = Process.Start(processInfo);
                
                string output = process.StandardOutput.ReadToEnd();
                _error = process.StandardError.ReadToEnd();
                exitCode = process.ExitCode;
                
                process.WaitForExit((1 * 1000) * 60);
                
                process.Close();
                process.Dispose();

                return exitCode;
            }

            catch (Exception err)
            {
                throw err;
            }
        }

        public static string ExecuteBatchWithResponse(string batchPath, string arg, out string _error)
        {
            try
            {
                _error = null;
                int exitCode;
                ProcessStartInfo processInfo;
                Process process;

                processInfo = new ProcessStartInfo("cmd.exe", "/c " + arg);
                processInfo.CreateNoWindow = true;
                processInfo.UseShellExecute = false;
                // *** Redirect the output ***
                processInfo.RedirectStandardError = true;
                processInfo.RedirectStandardOutput = true;               
                processInfo.WorkingDirectory = batchPath;
                process = Process.Start(processInfo);
                process.WaitForExit();

                // *** Read the streams ***
                // Warning: This approach can lead to deadlocks, see Edit #2
                string output = process.StandardOutput.ReadToEnd();
                _error = process.StandardError.ReadToEnd();

                exitCode = process.ExitCode;

                process.Close();

                return output;
            }

            catch (Exception err)
            {
                throw err;
            }
        }

        public async static void Copy(string source, string destination, bool isOverwrite, bool copyWebConfig)
        {
            try
            {
                //Obter os diretórios de origem e destino
                DirectoryInfo dirSource = new DirectoryInfo(source);
                DirectoryInfo dirDestination = new DirectoryInfo(destination);

                if (!dirSource.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Origem não encontrado: " + source);
                }

                if (!dirDestination.Exists)
                {
                    dirDestination.Create();
                }

                // Receber todos arquivos da origem e então copiar para destino
                FileInfo[] files = dirSource.GetFiles();
                foreach (FileInfo file in files)
                {
                    string filename = file.Name.ToString().ToLower();
                    if (copyWebConfig)
                    {
                        string temppath = Path.Combine(destination, file.Name);
                        file.CopyTo(temppath, isOverwrite);
                    }
                    else
                    {
                        if (!filename.Contains("web.config"))
                        {
                            string temppath = Path.Combine(destination, file.Name);
                            file.CopyTo(temppath, isOverwrite);
                        }
                    }
                }

                DirectoryInfo[] dirs = dirSource.GetDirectories();
                foreach (DirectoryInfo subdir in dirs)
                {
                    string subdirName = subdir.Name;

                    Directory.CreateDirectory(Path.Combine(destination, subdirName));
                    if (copyWebConfig)
                    {
                        Copy(subdir.FullName, Path.Combine(destination, subdirName), true, true);
                    }
                    else
                    {
                        Copy(subdir.FullName, Path.Combine(destination, subdirName), true, false);
                    }
                }                
            }
            catch (Exception err)
            {                
                throw err;
            }
        }

        public async static void Copy(string file, string fileDestinationName, bool isOverwrite)
        {
            try
            {                
                FileInfo fileFull = new FileInfo(file);
                
                fileFull.CopyTo(fileDestinationName, isOverwrite);                
            }
            catch (Exception err)
            {                
                throw err;
            }
        }

        public static bool Compress(string folder, string fileName)
        {
            try
            {             
                DirectoryInfo dirSource = new DirectoryInfo(folder);
                FileInfo file = new FileInfo(fileName);
                if (file.Exists)
                {
                    File.Delete(fileName);
                }

                FileInfo[] files = dirSource.GetFiles();

                ZipFile.CreateFromDirectory(folder, fileName);
                
                //foreach (var file in files)
                //{
                    
                //    zip.CreateEntryFromFile(fileName, "celtabsclient.zip", CompressionLevel.Optimal);
                //}
                
                //zip.Dispose();

                return true;
            }
            catch(Exception err)
            {
                throw err;
            }
        }


        //public static string ExecuteSynch(string path, string command, string args, out string _error)
        //{
        //    try
        //    {
        //        string filenameFull = Path.Combine(path, command);
        //        string message = null;
        //        _error = null;
        //        if (!String.IsNullOrEmpty(command))
        //        {
        //            using (Process p1 = new Process())
        //            {
        //                p1.StartInfo.FileName = "\"" + filenameFull + "\"";
        //                p1.StartInfo.Arguments = args;
        //                p1.StartInfo.CreateNoWindow = true;
        //                p1.StartInfo.UseShellExecute = false;
        //                p1.StartInfo.RedirectStandardOutput = true;
        //                p1.StartInfo.RedirectStandardError = true;
        //                p1.StartInfo.WorkingDirectory = path;

        //                p1.Start();

        //                //p1.WaitForExit(120000);
        //                if (p1.Start())
        //                {
        //                    message = p1.StandardOutput.ReadToEnd();
        //                    _error = p1.StandardError.ReadToEnd();
        //                }
        //            }
        //        }
        //        return message;
        //    }
        //    catch (Exception err)
        //    {
        //        throw err;
        //    }
        //}

    }
}


