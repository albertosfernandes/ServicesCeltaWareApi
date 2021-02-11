using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.Tools
{
    public class SystemFileHelps
    {
        public async static Task<bool> FileExist(string fileNameFull)
        {
            try
            {
                FileInfo file = null;
                if (!String.IsNullOrEmpty(fileNameFull))
                {
                    file = new FileInfo(fileNameFull);
                }

                if (file.Exists)
                    return true;
                else
                    return false;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        public async static Task<bool> CreateFile(string fileNameFull, string content, bool isOverwrite)
        {
            if (isOverwrite) 
            {
                using (StreamWriter script = File.CreateText(fileNameFull))
                {
                    script.WriteLine(content);
                }
            }
            else
            {
                using (StreamWriter script = File.AppendText(fileNameFull))
                {
                    script.WriteLine(content);
                }
            }
            

            if (await FileExist(fileNameFull))
                return true;
            else
                return false;                                
        }

        public static bool FileIsUpdate(string file1, string file2, string path)
        {
            try
            {
                string readFile1 = File.ReadAllText(Path.Combine(path, file1));
                string readFile2 = File.ReadAllText(Path.Combine(path, file2));
                
                if (!readFile1.Equals(readFile2))
                {
                    File.Copy(Path.Combine(path, file1), Path.Combine(path, file2), true);
                    return true;
                }
                else
                    return false;
            }
            catch(Exception err)
            {
                return false;
            }
        }

        private static string GetHashMd5(string fileNameFull)
        {
            MD5 md5 = MD5.Create();
            byte[] bytes;
            string resultado = "";

            using (FileStream stream = File.OpenRead(fileNameFull))
            {
                bytes = md5.ComputeHash(stream);
            }
            
            foreach (byte b in bytes)
            {
                resultado += b.ToString("x2");
            }
            return resultado;
        }

    }
}
