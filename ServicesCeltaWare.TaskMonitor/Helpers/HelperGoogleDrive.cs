using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.TaskMonitor.Helpers
{
    public class HelperGoogleDrive
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<bool> UploadBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskMonitorSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PostAsync(url + "/api/GoogleDriveService/Upload", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                    return false;

                if(streamResult.IsSuccessStatusCode)
                {
                    if (_setting.IsDebug)
                    {
                        _utilTelegram.SendMessage($"Backup armazenado no GDrive com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\brTipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    }                    
                }
                return true;
            }
            catch (Exception err)
            {
                _utilTelegram.SendMessage($"Falha durante armazenamento no GDrive: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} \brMensagem: { err.Message}", _setting.UidTelegramDestino);
                throw err;
            }
        }
    }
}
