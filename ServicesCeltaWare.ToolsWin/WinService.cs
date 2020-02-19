using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ServicesCeltaWare.ToolsWin
{
    public class WinService
    {
        public string Status(string serviceName)
        {
            try
            {
                string message = null;
                if (!String.IsNullOrEmpty(serviceName))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = @"C:\Windows\System32\wbem\wmic.exe";
                        p1.StartInfo.Arguments = $"service" + serviceName + "get state ";
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.Start();

                        if (p1.Start())
                        {
                            p1.WaitForExit();
                            message = p1.StandardOutput.ReadToEnd();

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
    }
}
