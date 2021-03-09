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

                msg = await CommandBash.Execute(_command);

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
                string msg = await CommandBash.Execute(_command);

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
                string fileNameFullScript = _databaseSchedule.Databases.Directory + @"/" +_databaseSchedule.Directory +@"/"
                + _databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString() + ".sql";
                
                
                string backupName;
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupFull.bak";
                else if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Diferential)
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.Hour.ToString()+ _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";
                else
                    backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupIncremetalLog{_databaseSchedule.DateHourExecution.Hour.ToString() + _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";


                string commandBackup = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword}  -i /var/opt/mssql/{_databaseSchedule.Directory}/{_databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString()}.sql";

                string scriptBackup; 
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                {
                    scriptBackup = $"BACKUP DATABASE [{_databaseSchedule.Databases.DatabaseName}] TO  DISK = N\'/var/opt/mssql/{_databaseSchedule.Directory}/{backupName}\'";
                    scriptBackup += $" WITH NOFORMAT, INIT,  NAME = N'{backupName}-Banco de Dados Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION,  STATS = 10";
                }

                else if(_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Diferential)
                {                  
                    scriptBackup = $"BACKUP DATABASE [{_databaseSchedule.Databases.DatabaseName}] TO  DISK = N\'/var/opt/mssql/{_databaseSchedule.Directory}/{backupName}\'";
                    scriptBackup += $" WITH DIFFERENTIAL, NOFORMAT, INIT,  NAME = N'{backupName}-Banco de Dados Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION,  STATS = 10";
                }

                else
                {
                    scriptBackup = $"BACKUP LOG [{_databaseSchedule.Databases.DatabaseName}] TO  DISK = N\'/var/opt/mssql/{_databaseSchedule.Directory}/{backupName}\'";
                    scriptBackup += $" WITH NOFORMAT, INIT,  NAME = N'{backupName}-Banco de Dados Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION,  STATS = 10";
                }
                
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
                string ScriptFileNameFull = ReturnScriptName(_databaseSchedule, true);

                string backupName = ReturnBackupName(_databaseSchedule);

                string commandValidate = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword}  -i /var/opt/mssql/{_databaseSchedule.Directory}/Validate{_databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString()}.sql";



                bool res = await CreateFile(_validateType, backupName, ScriptFileNameFull, _databaseSchedule);
                while (!res)
                {
                    res = await CreateFile(_validateType, backupName, ScriptFileNameFull, _databaseSchedule);
                }

                return commandValidate;

            }
            catch (Exception err)
            {
                return "Error: " + err.Message;
            }
        }

        public async static Task<string> GenerateScriptShrink(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string _fileNameFullScript = null;
                _fileNameFullScript = _databaseSchedule.Databases.Directory + @"/" + _databaseSchedule.Directory + @"/" + "Shrink" + _databaseSchedule.Databases.DatabaseName + ".sql";
                string script = $"USE [{_databaseSchedule.Databases.DatabaseName}]" + "\n";
                script += $"DBCC SHRINKFILE (CeltaBSYoguty_log, 0, TRUNCATEONLY);";

                if (await SystemFileHelps.FileExist(_fileNameFullScript))
                {
                    return _fileNameFullScript;
                }
                else
                {
                    if(await SystemFileHelps.CreateFile(_fileNameFullScript, script, false))
                        return _fileNameFullScript;
                }
                return _fileNameFullScript;
            }
            catch (Exception err)
            {
                return "Error: " + err.Message;
            }
        }

        public static string ReturnBackupName(ModelBackupSchedule _databaseSchedule)
        {
            string backupName;
            if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupFull.bak";
            else if(_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Incremental)
                backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupIncremetalLog{_databaseSchedule.DateHourExecution.Hour.ToString() + _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";
            else
            {
                backupName = $"{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.Hour.ToString() + _databaseSchedule.DateHourExecution.Minute.ToString()}.bak";
            }
            return backupName; 
        }   

        private static string ReturnScriptName(ModelBackupSchedule _databaseSchedule, bool isValidType)
        {
            string fileNameFullScript = null;

            if (isValidType)
            {
                fileNameFullScript = _databaseSchedule.Databases.Directory + @"/" + _databaseSchedule.Directory + @"/Validate" + _databaseSchedule.Databases.DatabaseName + _databaseSchedule.DateHourExecution.ToShortTimeString() + ".sql";
            }
            return fileNameFullScript;
        }

        public static async Task<string> ReturnRecoveryModelType(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string _command = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.Databases.CustomerProduct.LoginPassword} -Q \"SELECT recovery_model_desc FROM sys.databases WHERE name = '{_databaseSchedule.Databases.DatabaseName}'\"; ";
                string msg = await CommandBash.Execute(_command);

                if (msg.Contains("FULL"))
                    return "1";
                else if (msg.Contains("SIMPLE"))
                    return "3";
                else if(msg.Contains("(0 rows affected)"))
                {
                    return "0 rows affected";
                }
                else
                    return msg;
            }
            catch (Exception err)
            {
                if (!String.IsNullOrEmpty(err.InnerException.Message))
                {
                    return err.Message + "\n" + err.InnerException.Message;
                }
                return err.Message;
            }
        }

        public static async Task<string> ChangeRecoveryModelType(ModelBackupSchedule _databaseSchedule, string recoveryModeValue)
        {
            try
            {
                string _command = null;
                string script = null;
                if (recoveryModeValue == "1")
                {
                    script = $"ALTER DATABASE { _databaseSchedule.Databases.DatabaseName} SET RECOVERY FULL WITH NO_WAIT";
                    await SystemFileHelps.CreateFile($"{_databaseSchedule.Databases.Directory}/{_databaseSchedule.Directory}/setRecoveryFull{_databaseSchedule.Databases.DatabaseName}.sql", script, false);
                    _command = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.Databases.CustomerProduct.LoginPassword} -i /var/opt/mssql/{_databaseSchedule.Directory}/setRecoveryFull{_databaseSchedule.Databases.DatabaseName}.sql";
                }
                else if (recoveryModeValue == "3")
                {
                    script = $"ALTER DATABASE {_databaseSchedule.Databases.DatabaseName} SET RECOVERY SIMPLE WITH NO_WAIT";
                    await SystemFileHelps.CreateFile($"{_databaseSchedule.Databases.Directory}/{_databaseSchedule.Directory}/setRecoverySimple{_databaseSchedule.Databases.DatabaseName}.sql", script, false);
                    _command = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.Databases.CustomerProduct.LoginPassword} -i /var/opt/mssql/{_databaseSchedule.Directory}/setRecoverySimple{_databaseSchedule.Databases.DatabaseName}.sql";
                }
                else
                    return $"Opção inválida{recoveryModeValue}(DatabaseServiceHelper/ChangeRecoveryModelType). 1 para FULL e 3 para SIMPLE.";

                return await CommandBash.Execute(_command);
                
            }
            catch(Exception err)
            {
                if (!String.IsNullOrEmpty(err.InnerException.Message))
                {
                    return err.Message + "\n" + err.InnerException.Message;
                }
                return err.Message;
            }
        }

        private static async Task<bool> CreateFile(ServicesCeltaWare.Model.Enum.ValidateType _validateType, string backupName, string _fileNameFullScript, ModelBackupSchedule _backupSchedule)
        {
            string scriptBackup = null;
            if (_validateType == ServicesCeltaWare.Model.Enum.ValidateType.LabelOnly)
            {
                scriptBackup = $"Restore LabelOnly from Disk =  \'/var/opt/mssql/{_backupSchedule.Directory}/{backupName}\'";
            }
            else if (_validateType == ServicesCeltaWare.Model.Enum.ValidateType.VerifyOnly)
            {
                scriptBackup = $"Restore VerifyOnly from Disk =  \'/var/opt/mssql/{_backupSchedule.Directory}/{backupName}\'";
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
        }
    }
}
