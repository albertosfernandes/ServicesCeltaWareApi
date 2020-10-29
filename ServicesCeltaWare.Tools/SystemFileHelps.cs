using System;
using System.Collections.Generic;
using System.IO;
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
    }
}
