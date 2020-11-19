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
        public static async Task<bool> Upload(Model.ModelBackupSchedule _backupSchedule)
        {
            string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
            var json = JsonConvert.SerializeObject(_backupSchedule);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

            var streamResult = await client.PostAsync(url + "/api/GoogleDriveService/Upload", stringContent);

            if (!streamResult.IsSuccessStatusCode)
            {
                //resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true);
                return false;
            }

            return true;
        }
    }
}
