using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ServicesCeltaWare.Tools
{
    public class CommandBash
    {
        public static string Execute(string args, out string error)
        {
            try
            {
                string message = null;
                error = null;

                using (Process p1 = new Process())
                {
                    p1.StartInfo.FileName = "bash";
                    p1.StartInfo.Arguments = $"-c \"{args}\"";
                    p1.StartInfo.CreateNoWindow = false;
                    p1.StartInfo.UseShellExecute = false;
                    p1.StartInfo.RedirectStandardOutput = true;
                    p1.StartInfo.RedirectStandardError = true;
                    // p1.StartInfo.WorkingDirectory = path;

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
    }
}
