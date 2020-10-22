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
                    //msg = CommandWin32.ExecuteSynch(_dirPath, _command, args, out _error);
                    msg = await CommandWin32.ExecuteTeste(_dirPath, _command, args);

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

        //public async static Task<string> Repair(string _dirPath, string _command, string args)
        //{
        //    try
        //    {                
        //        var msg = await CommandWin32.ExecuteSynch(_dirPath, _command, args);
        //        if (msg.Contains("There is already an object named") || msg.Contains("lixo.lix"))
        //        {
        //            for (int i = 0; i < 2; i++)
        //            {
        //                System.Threading.Thread.Sleep(9 * 1000);
        //                var msg2 = Create(_dirPath, _command, args);
        //                if (msg2.Equals(msg))
        //                {
        //                    i = 3;
        //                }
        //            }
        //        }
        //        return msg;
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

        //public async static Task<string> RepairNew(string _dirPath, string _command, string args)
        //{
        //    try
        //    {                
        //        var msg = await CommandWin32.ExecuteSynch(_dirPath, _command, args);
        //        if (msg.Contains("ERROR"))
        //        {
        //            //deu erro, vms tratar!                    
        //            string msgLoop = null;
        //            string errorLoop = msg;
        //            for (int i = 0; i < 2; i++)
        //            {
        //                // melhor aguardar um pouco antes de executar novamente!!!
        //                System.Threading.Thread.Sleep(9 * 1000);
        //                msgLoop = await CommandWin32.ExecuteSynch(_dirPath, _command, args);

        //                if (errorLoop.Equals(msgLoop))
        //                {
        //                    //então é erro mesmo adiciona a msg para retornar o erro
        //                    i = 3;
        //                    msg += msgLoop;
        //                }
        //                // atualiza o retorno da execução
        //                msg = msgLoop;
        //            }

        //        }
        //        return msg;
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

        //public async static Task<string> Create(string _dirPath, string _command, string args)
        //{
        //    try
        //    {                
        //        var msg = await CommandWin32.ExecuteSynch(_dirPath, _command, args);
        //        string msgLoop = null;
        //        if (msg.Contains("There is already an object named") || msg.Contains("lixo.lix"))
        //        {
        //            for (int i = 0; i < 2; i++)
        //            {
        //                System.Threading.Thread.Sleep(9 * 1000);
        //                msgLoop = await Create(_dirPath, _command, args);
        //                if (msgLoop.Equals(msg))
        //                {
        //                    i = 3;
        //                }
        //            }                                                           
        //        }
        //        if(msgLoop != null)
        //        {
        //            msg = msgLoop;
        //        }                
        //        return msg;
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}


        //public static string CreateNew(string _dirPath, string _command, string args)
        //{
        //    try
        //    {
        //        string errorScript;
        //        var msg = CommandWin32.ExecuteSynch(_dirPath, _command, args, out errorScript);
        //        if (!String.IsNullOrEmpty(errorScript))
        //        {
        //            //deu erro, vms tratar!                    
        //            string msgLoop = null;
        //            string errorLoop = errorScript;                    

        //            for (int i = 0; i < 2; i++)
        //            {
        //                // melhor aguardar um pouco antes de executar novamente!!!
        //                System.Threading.Thread.Sleep(9 * 1000);
        //                msgLoop = CommandWin32.ExecuteSynch(_dirPath, _command, args, out errorScript);                        
                        
        //                if (errorLoop.Equals(errorScript))
        //                {
        //                    //então é erro mesmo adiciona a msg para retornar o erro
        //                    i = 3;
        //                    msg += errorScript;
        //                }
        //                // atualiza o retorno da execução
        //                msg = msgLoop;
        //            }

        //        }
        //        return msg;               
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

        //public static string CreateBsMessages(string _dirPath, string _command, string args)
        //{
        //    try
        //    {
        //        string errorScript;
        //        var msg = CommandWin32.ExecuteSynch(_dirPath, _command, args, out errorScript);
        //        if (!String.IsNullOrEmpty(errorScript))
        //        {
        //            //deu erro, vms tratar!                    
        //            string msgLoop = null;
        //            System.Threading.Thread.Sleep(9 * 1000);

        //            for (int i = 0; i < 2; i++)
        //            {
        //                msgLoop = CommandWin32.ExecuteSynch(_dirPath, _command, args, out errorScript);

        //                if (msgLoop.Equals(errorScript))
        //                {
        //                    i = 3;
        //                }
        //                System.Threading.Thread.Sleep(9 * 1000);
        //            }

        //        }
        //        return msg;
        //    }
        //    catch (Exception err)
        //    {
        //        return err.Message;
        //    }
        //}

      
    }
}
