using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class SystemFilesHelper
    {
        public static bool FileExist(string fileNameFull)
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
            catch(Exception err)
            {
                return false;
            }
        }

        public static bool CompressFileOrFolder(string pathName, string fileName)
        {
            try
            {
                //string newPathName = pathName.Substring(1, pathName.IndexOf("Celtapublic.csk")-1);
                using (ZipArchive zip = ZipFile.Open(pathName + @"\ParChaves" + fileName + ".zip", ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(pathName + @"celtapublic.csk", "celtapublic.csk");
                    zip.CreateEntryFromFile(pathName + @"celtaprivate.csk", "celtaprivate.csk");
                }

                
                //ZipFile.CreateFromDirectory(pathName, pathName + @"\ParChaves" + fileName + ".zip");
                return true;
            }
            catch(Exception err)
            {
                throw err;
            }
        }
    }
}
