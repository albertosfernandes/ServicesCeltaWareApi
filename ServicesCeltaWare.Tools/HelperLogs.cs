using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServicesCeltaWare.Tools
{
    public class HelperLogs
    {
        public async static void WriteLog(string origin, string _msg)
        {
            try
            {
                StreamWriter sw = null;
                sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + $"\\{origin}Log.txt", true);
                await sw.WriteLineAsync(DateTime.Now.ToString() + $": {origin} - " + _msg);
                sw.Flush();
                sw.Close();
            }
            catch(Exception err)
            {
                throw err;
            }
        }
    }
}
