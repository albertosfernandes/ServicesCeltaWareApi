using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCeltaware.ServerAPI.Helpers
{
    public class HelperLog
    {
        public async static Task<bool> WriteLog(string origrin, string _msg)
        {
            StreamWriter sw = null;
            sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + $"\\{origrin}.txt", true);
            await sw.WriteAsync(DateTime.Now.ToString() + $": {origrin} - " + _msg);
            sw.Flush();
            sw.Close();
            return true;
        }
    }
}
