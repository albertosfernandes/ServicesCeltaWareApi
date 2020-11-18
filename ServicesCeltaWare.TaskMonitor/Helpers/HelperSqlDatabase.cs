using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.TaskMonitor.Helpers
{
    public class HelperSqlDatabase
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<bool> ExecuteDatabaseSchedule(Model.ModelBackupSchedule _backupSchedule, ModelTaskMonitorSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                //TimeSpan T_hours = TimeSpan.FromHours(DateTime.Now.Hour);
                //TimeSpan t_minutes = TimeSpan.FromMinutes(DateTime.Now.Minute);

                bool response = false;
                int isExecute = _backupSchedule.DateHourExecution.CompareTo(DateTime.Now);
                int lastExecution = _backupSchedule.DateHourLastExecution.CompareTo(DateTime.Now.Date);
                if ((isExecute == 0 && lastExecution == -1) || (isExecute == -1 && lastExecution == -1) || !_backupSchedule.BackupStatus.Equals(Model.Enum.BackupStatus.Runing))
                {
                    response = await ExecuteBackup(_backupSchedule);
                    if (response)
                    {                       
                        bool res = await ExecuteValidateBackup(_backupSchedule);
                        if(!res)
                        {                            
                            response = false;
                        }
                    }                       
                }

                if (response)
                {
                    if (_setting.IsDebug)
                    {
                        _utilTelegram.SendMessage($"Backup Executado com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()}", _setting.UidTelegramDestino);
                    }
                    return true;
                }

                _utilTelegram.SendMessage($"Falha ao validar backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()}", _setting.UidTelegramDestino);
                return false;
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha no sistema de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} - { err.Message}", _setting.UidTelegramDestino);
                throw err;
            }
        }

        public static async Task<bool> ExecuteBackup(Model.ModelBackupSchedule _backupSchedule)
        {
            try
            {
                bool resp;
                // Marcar como backup status 2 Runing
                resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Runing, false);

                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PostAsync(url + "/api/DatabaseService/BackupExec", stringContent);

                if (streamResult.IsSuccessStatusCode)
                {
                    resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true);
                    return true;
                }

                resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true);
                return false;
            }
            catch(Exception err)
            {
                return false;
            }
        }

        private static async Task<bool> UpdateStatusBackup(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, bool bkpIsRunning)
        {
            string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
            _backupSchedule.BackupStatus = bkpStatus;

            var json = JsonConvert.SerializeObject(_backupSchedule);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            var streamResult = await client.PutAsync(url + "/api/DatabaseSchedule/UpdateStatus", stringContent);

            if (!streamResult.IsSuccessStatusCode)
                return false;


            if (_backupSchedule.BackupStatus.Equals(Model.Enum.BackupStatus.Success) && bkpIsRunning)
            {
                var resul = await UpdateDateHourLastExecution(_backupSchedule);
                if (!resul)
                    return false;
            }

            return true;
        }

        private static async Task<bool> UpdateDateHourLastExecution(Model.ModelBackupSchedule _backupSchedule)
        {
            string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;

            var json = JsonConvert.SerializeObject(_backupSchedule);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            var streamResult = await client.PutAsync(url + "/api/DatabaseSchedule/UpdateDateHourLastExecution", stringContent);

            if (streamResult.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        private static async Task<bool> ExecuteValidateBackup(Model.ModelBackupSchedule _backupSchedule)
        {
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PutAsync(url + "/api/DatabaseService/ValidateBackupExec", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                    return false;

                return true;
            }
            catch(Exception err)
            {
                return false;
            }
        }
    }
}
