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
        private static HttpClient client = new HttpClient();

        public HelperSqlDatabase()
        {
            client.Timeout = TimeSpan.FromHours(4);
        }

        public async Task<bool> ExecuteDatabaseSchedule(Model.ModelBackupSchedule _backupSchedule, ModelTaskMonitorSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {

                bool response = false;

                response = await ExecuteBackup(_backupSchedule, _setting);
                if (response)
                {
                    bool res = await ExecuteValidateBackup(_backupSchedule, _setting);
                    if (!res)
                    {
                        response = false;
                    }
                }

                if (response)
                {
                    if (_setting.IsDebug)
                    {
                        _utilTelegram.SendMessage($"Backup Executado com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\brTipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    }
                    return true;
                }
                _utilTelegram.SendMessage($"Falha ao validar arquivo backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\brTipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true);
                return false;

            }
            catch(Exception err)
            {
                var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true);
                _utilTelegram.SendMessage($"Falha no sistema de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} - { err.Message}", _setting.UidTelegramDestino);
                throw err;
            }
        }

        public async Task<bool> ExecuteBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskMonitorSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
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
                    //resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true);
                    return true;
                }

                
                resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true);
                //_backupSchedule.DateHourLastExecution = DateTime.Now;
                //await UpdateDateHourLastExecution(_backupSchedule);
                _utilTelegram.SendMessage($"Falha na execução de Backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}", _setting.UidTelegramDestino);
                return false;
            }
            catch(Exception err)
            {
                return false;
            }
        }

        public async Task<bool> UpdateStatusBackup(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, bool bkpIsRunning)
        {
            string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
            _backupSchedule.BackupStatus = bkpStatus;

            var json = JsonConvert.SerializeObject(_backupSchedule);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");            

            var streamResult = await client.PutAsync(url + "/api/DatabaseSchedule/UpdateStatus", stringContent);

            if (!streamResult.IsSuccessStatusCode)
                return false;            

            return true;
        }

   


        private async Task<bool> UpdateDateHourLastExecution(Model.ModelBackupSchedule _backupSchedule)
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

        private async Task<bool> ExecuteValidateBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskMonitorSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                //TimeSpan t = new TimeSpan(4, 00, 00);
                //client.Timeout = t;

                var streamResult = await client.PutAsync(url + "/api/DatabaseService/ValidateBackupExec", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                {
                    var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true);
                    _utilTelegram.SendMessage($"Falha na validação do backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()}", _setting.UidTelegramDestino);
                    return false;
                }
                
                return true;
            }
            catch(Exception err)
            {
                return false;
            }
        }


    }
}
