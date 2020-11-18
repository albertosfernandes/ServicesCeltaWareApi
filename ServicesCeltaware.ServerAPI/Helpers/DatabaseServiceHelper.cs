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
                string fileNameFullScript = _databaseSchedule.Databases.Directory + @"/backup/" + _databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString() + ".sql";
                string backupName;
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupFull.bak";
                else
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.Hour.ToString()+ _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";

                string commandBackup = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword}  -i /var/opt/mssql/backup/{_databaseSchedule.Databases.DatabaseName}.sql";
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
                return "Error" + err.Message;
            }
        }

        public async static Task<string> GenerateScriptValidate(ModelBackupSchedule _databaseSchedule, ServicesCeltaWare.Model.Enum.ValidateType _validateType)
        {
            try
            {

                //string fileNameFullScript = _databaseSchedule.Databases.Directory + @"/backup/Validate" + _databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString() + ".sql";
                string ScriptFileNameFull = ReturnScriptName(_databaseSchedule, true);

                string backupName = ReturnBackupName(_databaseSchedule);

                string commandValidate = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword}  -i /var/opt/mssql/backup/Validate{_databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString()}.sql";



                bool res = await CreateFile(_validateType, backupName, ScriptFileNameFull);
                while (!res)
                {
                    res = await CreateFile(_validateType, backupName, ScriptFileNameFull);
                }

                return commandValidate;

                //if (await SystemFileHelps.FileExist(fileNameFullScript))
                //{
                //    return commandValidate;
                //}
                //else
                //{
                //    await SystemFileHelps.CreateFile(fileNameFullScript, scriptBackup, false);
                //    return commandValidate;
                //}
            }
            catch (Exception err)
            {
                return "Error" + err.Message;
            }
        }

        private static string ReturnBackupName(ModelBackupSchedule _databaseSchedule)
        {
            string backupName;
            if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupFull.bak";
            else
                backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.Hour.ToString() + _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";
            return backupName;
        }

        private static string ReturnScriptName(ModelBackupSchedule _databaseSchedule, bool isValidType)
        {
            string fileNameFullScript = null;

            if (isValidType)
            {
                fileNameFullScript = _databaseSchedule.Databases.Directory + @"/backup/Validate" + _databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString() + ".sql";
            }
            return fileNameFullScript;
        }

        private static async Task<bool> CreateFile(ServicesCeltaWare.Model.Enum.ValidateType _validateType, string backupName, string _fileNameFullScript)
        {
            string scriptBackup = null;
            if (_validateType == ServicesCeltaWare.Model.Enum.ValidateType.LabelOnly)
            {
                // Restore LabelOnly from Disk = 'C:\Backup\Backup-Simples-Criptografia.bak'
                scriptBackup = $"Restore LabelOnly from Disk =  \'/var/opt/mssql/backup/{backupName}\'";
            }
            else if (_validateType == ServicesCeltaWare.Model.Enum.ValidateType.LabelOnly)
            {
                // Restore VerifyOnly from Disk = 'C:\Backup\Backup-Simples-Criptografia.bak'
                scriptBackup = $"Restore VerifyOnly from Disk =  \'/var/opt/mssql/backup/{backupName}\'";
            }


            if (await SystemFileHelps.FileExist(_fileNameFullScript))
            {
                return true;
            }
            else
            {
                await SystemFileHelps.CreateFile(_fileNameFullScript, scriptBackup, false);
                return false;
            }


            //return scriptBackup;
        }
    }
}
