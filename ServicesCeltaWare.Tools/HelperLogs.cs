using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServicesCeltaWare.Tools
{
    public class HelperLogs
    {
        private async static void WriteLog(string _msg)
        {
            StreamWriter sw = null;
            sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\ServicesCeltaWareLog.txt", true);
            sw.WriteLine(DateTime.Now.ToString() + ": Services CeltaWare - " + _msg);
            sw.Flush();
            sw.Close();
        }
    }
}
