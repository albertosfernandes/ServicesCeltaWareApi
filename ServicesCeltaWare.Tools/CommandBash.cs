using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.Tools
{
    public class CommandBash
    {
        public async static Task<string> Execute(string args)
        {
            try
            {
                string message = null;
                string error = null;

                using (Process p1 = new Process())
                {
                    p1.StartInfo.FileName = "bash";
                    p1.StartInfo.Arguments = $"-c \"{args}\"";
                    p1.StartInfo.CreateNoWindow = true;
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.RedirectStandardError = true;
                    // p1.StartInfo.WorkingDirectory = path;

                    p1.Start();

                    var idProcess = p1.Id;
                    p1.WaitForExit();
                   
                    message = p1.StandardOutput.ReadToEnd();
                    error = p1.StandardError.ReadToEnd();
                    Console.WriteLine(message);
                    Console.WriteLine(error);
                   

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
    }
}
