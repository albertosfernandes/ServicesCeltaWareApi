using ServicesCeltaWare.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCeltaware.ServerAPI.Helpers
{
    public class DatabaseServiceHelper
    {
        public static async Task<string> Restart(string _command)
        {
            try
            {
                string _error = "iniciando";
                string msg = null;

                msg = CommandBash.Execute(_command, out _error);

                if (_error != "iniciando")
                    return _error;
                else
                    return msg;
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        public static async Task<string> Execute(string _command)
        {
            try
            {
                string _error = "iniciando";
                string msg = null;

                msg = CommandBash.Execute(_command, out _error);
                if (_error != "iniciando")
                    return _error;
                else
                    return msg;

            }
            catch (Exception err)
            {
                throw err;
            }
        }
    }
}
