using Newtonsoft.Json;
using ServicesCeltaWare.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.TaskService.Helpers
{
    public class HelperGoogleDrive
    {
        private static readonly HttpClient client = new HttpClient();
        private Helpers.HelperSqlDatabase _helperSqlDatabase = new HelperSqlDatabase();

        public HelperGoogleDrive()
        {
            client.Timeout = TimeSpan.FromHours(4);
        }       

        public async Task<string> UploadBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
              
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                Worker.WriteLog("Iniciando Upload de:" + _backupSchedule.Databases.DatabaseName + " as " + DateTime.Now);
                var streamResult = await client.PostAsync(url + "/api/GoogleDriveService/Upload", stringContent);
                Worker.WriteLog("Upload finalizado:" + streamResult.Content.ReadAsStringAsync());

                if (!streamResult.IsSuccessStatusCode)
                {
                    _utilTelegram.SendMessage($"Falha no Upload: " + streamResult.StatusCode + " " + streamResult.Content.ReadAsStringAsync(),  _setting.UidTelegramDestino);
                    return "ERRO: " + await streamResult.Content.ReadAsStringAsync();
                }

                if (streamResult.IsSuccessStatusCode)
                {                    
                    if (_setting.IsDebug)
                    {
                         _utilTelegram.SendMessage($"Backup armazenado no GDrive com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\rTipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    }
                }

                return await streamResult.Content.ReadAsStringAsync();
            }
            catch (Exception err)
            {
                var resp = await _helperSqlDatabase.UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, true, _setting);
                _utilTelegram.SendMessage($"Falha durante armazenamento no GDrive: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} \rMensagem: { err.Message}", _setting.UidTelegramDestino);
                throw err;
            }
        }

        private async static Task<bool> CopyCredentialtoServer(Model.ModelBackupSchedule _backupSchedule)
        {
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                string path = Directory.GetCurrentDirectory();
                path += @"\credential\";
                string filePath = path + "Google.Apis.Auth.OAuth2.Responses.TokenResponse-user";

                using (var httpClient = new HttpClient())
                {
                    using (var form = new MultipartFormDataContent())
                    {
                        using (var fs = File.OpenRead(filePath))
                        {
                            using (var streamContent = new StreamContent(fs))
                            {
                                using (var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync()))
                                {
                                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                                    // "file" parameter name should be the same as the server side input parameter name
                                    form.Add(fileContent, "credentialFile", Path.GetFileName(filePath));
                                    HttpResponseMessage response = await httpClient.PostAsync(url + "/api/GoogleDriveService/UploadFile", form);

                                    if (response.IsSuccessStatusCode)
                                        return true;
                                    else
                                        return false;
                                }
                            }
                        }
                    }
                }                
            }
            catch (Exception err)
            {
                return false;
            }
        }
    }
}
