using ServicesCeltaWare.Model;
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

                return msg;

            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public async static Task<string> GenerateScriptBackup(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string fileNameFullScript = _databaseSchedule.Databases.Directory + @"/backup/" + _databaseSchedule.Databases.DatabaseName + ".sql";
                string backupName;
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupFull.bak";
                else
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.ToString()}.bak";

                string commandBackup = $"docker exec -it {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword}  -i /var/opt/mssql/backup/{_databaseSchedule.Databases.DatabaseName}.sql";
                string scriptBackup = $"BACKUP DATABASE [{_databaseSchedule.Databases.DatabaseName}] TO  DISK = N\'/var/opt/mssql/backup/{backupName}\'";
                scriptBackup += $" WITH NOFORMAT, INIT,  NAME = N'{backupName}-Banco de Dados Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION,  STATS = 10";
                
                if (await SystemFileHelps.FileExist(fileNameFullScript))
                { 
                    return commandBackup;
                }
                else
                {
                    await SystemFileHelps.CreateFile(fileNameFullScript, scriptBackup, false);
                        return "back";                    
                }

               
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }
    }
}
