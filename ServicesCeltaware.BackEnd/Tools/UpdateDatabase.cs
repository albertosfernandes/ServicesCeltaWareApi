using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServicesCeltaware.BackEnd.Tools;

namespace ServicesCeltaware.BackEnd.Tools
{
    public class UpdateDatabase
    {
        public static async Task<string> Execute(string _dirPath, string _command, string args)
        {
            try
            {
                string _error= "iniciando";
                string msg = null;
                string errorLoop = null;
                while (!String.IsNullOrEmpty(_error))
                {
                    _error = null;
                    msg = await ServicesCeltaWare.Tools.CommandWin32.Execute(_dirPath, _command, args);

                    if (msg.Contains("ERROR"))
                    {
                        //deu erro, vms tratar!                                            
                        _error = msg;
                        if (errorLoop.Equals(msg))
                        {
                            //então é erro mesmo adiciona a msg para retornar o erro e sai do laço
                            msg += msg;
                            break;
                        }
                        errorLoop = msg;
                        // melhor aguardar um pouco antes de executar novamente!!!
                        System.Threading.Thread.Sleep(9 * 1000);
                    }
                }
                
                return msg;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }       

      
    }
}
