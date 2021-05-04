using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServicesCeltaWare.TaskService.Helpers
{
    public class HelperSqlDatabase
    {
        private static HttpClient client = new HttpClient();
        private HelperGoogleDrive _helperGoogleDrive = new HelperGoogleDrive();
        private readonly IConfiguration _configuration;

        public HelperSqlDatabase()
        {
            client.Timeout = TimeSpan.FromHours(12);
        }

        public async Task<string> BackupRun(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            
            try
            {
                var isExecuted = await ExecuteDatabaseSchedule(_backupSchedule, _setting);
                if (isExecuted.ToUpperInvariant().Contains("OK"))
                {
                    var resp = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Success, _setting);

                    return isExecuted;
                    #region lixeira
                    ////backup garantido.. então inicie o upload
                    //var googleDriveFileId = await _helperGoogleDrive.UploadBackup(_backupSchedule, _setting);
                    //if (googleDriveFileId.Contains("ERRO"))
                    //{
                    //    resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, true, _setting);
                    //    _utilTelegram.SendMessage($"Falha no Upload do Backup {_backupSchedule.Databases.DatabaseName}, tipo {_backupSchedule.Type}: {googleDriveFileId}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                    //    new Exception(googleDriveFileId);
                    //}
                    //else
                    //{
                    //    _backupSchedule.GoogleDriveFileId = googleDriveFileId;
                    //    resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true, _setting);
                    //    if (!resp.Equals("sucess"))
                    //    {
                    //        _utilTelegram.SendMessage($"Falha no serviço TaskManager: Erro ao atualizar status do backup, GoogleFileId e Status. " + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                    //    }
                    //}
                    #endregion
                }

                return isExecuted;
            }
            catch (Exception err)
            {
                var isUpdated = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, _setting);

                _utilTelegram.SendMessage($"Falha no serviço TaskManager: " + err.Message + "\n" + err.StackTrace , _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                if (err.InnerException != null)
                {
                    HelperLogs.WriteLog("HelperSqlDatabase", err.Message + "\n" + err.InnerException.Message);                    
                    return (err.Message + "\n" + err.InnerException.Message);
                }
                return err.Message;
            }
        }

        public async Task<string> ExecuteUpload(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            bool isUpdated;
            try
            {
                string resp = null;
                isUpdated = await UpdateStatusDateTimeUpload(_backupSchedule, Model.Enum.BackupStatus.Uploading, _setting);
                var googleDriveFileId = await _helperGoogleDrive.UploadBackup(_backupSchedule, _setting);
                if (googleDriveFileId.ToUpperInvariant().Contains("ERRO"))
                {
                    isUpdated = await UpdateStatusDateTimeUpload(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, _setting);
                    _utilTelegram.SendMessage($"Falha no Upload do Backup {_backupSchedule.Databases.DatabaseName}, tipo {_backupSchedule.Type}: {googleDriveFileId}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                    new Exception(googleDriveFileId);
                }
                else
                {
                    _backupSchedule.GoogleDriveFileId = googleDriveFileId;
                    isUpdated = await UpdateStatusDateTimeUpload(_backupSchedule, Model.Enum.BackupStatus.Success, _setting);
                    if (!isUpdated)
                    {
                        _utilTelegram.SendMessage($"Falha ao atualizar status do backup, TaskService ExecUpload. \n" + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                    }
                }

                return resp;
            }
            catch(Exception err)
            {
                isUpdated = await UpdateStatusDateTimeUpload(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, _setting);
                if (!isUpdated)
                {
                    _utilTelegram.SendMessage($"Falha ao atualizar status do backup, TaskService ExecUpload. \n", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                }
                if (err.InnerException != null)
                {
                    HelperLogs.WriteLog("HelperSqlDatabase", err.Message + "\n" + err.InnerException.Message);
                    return (err.Message + "\n" + err.InnerException.Message);
                }
                HelperLogs.WriteLog("HelperSqlDatabase", err.Message);
                return err.Message;
            }
        }
            

        public async Task<string> ExecuteDatabaseSchedule(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            bool isUpdated;
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
                else
                {
                    _utilTelegram.SendMessage($"Falha na execução de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);

                    isUpdated = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, _setting);
                    return "Arquivo corrompido.";
                }

                if (response)
                {
                    if (_setting.IsDebug)
                    {
                        _utilTelegram.SendMessage($"Backup Executado com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    }
                    return "OK";
                }
                _utilTelegram.SendMessage($"Falha ao validar arquivo backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);

                isUpdated = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, _setting);

                return "Arquivo corrompido.";

            }
            catch (Exception err)
            {
                isUpdated = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, _setting);
                _utilTelegram.SendMessage($"Falha no sistema de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} - { err.Message}", _setting.UidTelegramDestino);

                if (err.InnerException != null)
                {
                    HelperLogs.WriteLog("HelperSqlDatabase", err.Message + "\n" + err.InnerException.Message);
                    return (err.Message + "\n" + err.InnerException.Message);
                }
                HelperLogs.WriteLog("HelperSqlDatabase", err.Message);              
                return err.Message;
            }
        }

        public async Task<bool> ExecuteBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            
            try
            {                
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");


                var streamResult = await client.PostAsync(url + "/api/DatabaseService/BackupExec", stringContent);

                if (streamResult.IsSuccessStatusCode)
                {                
                    return true;
                }
                else
                {
                    var errorResponse = await streamResult.Content.ReadAsStringAsync();
                    var isUpdated = await UpdateStatusDateTimeBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, _setting);
                    _utilTelegram.SendMessage($"Falha na execução de Backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}", _setting.UidTelegramDestino);
                    return false;
                }
            }
            catch (Exception err)
            {
                _utilTelegram.SendMessage($"Falha na execução de Backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n {err.Message}", _setting.UidTelegramDestino);
                return false;
            }
        }

        public async Task<string> UpdateStatusBackup(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, bool bkpIsRunning, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                _backupSchedule.BackupStatus = bkpStatus;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PutAsync(url + "/api/DatabaseSchedule/UpdateStatus", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                {
                    return await streamResult.Content.ReadAsStringAsync();
                }

                return "sucess";
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha na atualização Status: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n {err.Message}", _setting.UidTelegramDestino);
                return "false";
            }           
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

        public async Task<bool> UpdateStatusDateTimeBackup(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, ModelTaskServiceSettings _setting)
        {
            try
            {
                bool isValid = true;
                switch (bkpStatus)
                {
                    case Model.Enum.BackupStatus.Success:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.BackupExecDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Failed:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.BackupExecDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Runing:
                        {
                            _backupSchedule.BackupExecDateHourStart = DateTime.Now;
                            var respUpd = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Runing, false, _setting);
                            if (respUpd != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Corrupted:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.BackupExecDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.None:
                        {
                            break;
                        }
                    case Model.Enum.BackupStatus.OutOfDate:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.BackupExecDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                }

                return isValid;
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        private async Task<bool> UpdateStatusDateTimeUpload(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, ModelTaskServiceSettings _setting)
        {
            try
            {
                bool isValid = true;
                switch (bkpStatus)
                {
                    case Model.Enum.BackupStatus.Success:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.UploadDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Failed:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.UploadDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Runing:
                        {
                            _backupSchedule.UploadDateHourStart = DateTime.Now;
                            var respUpd = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Runing, false, _setting);
                            if (respUpd != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.Corrupted:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.UploadDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                    case Model.Enum.BackupStatus.None:
                        {
                            break;
                        }
                    case Model.Enum.BackupStatus.OutOfDate:
                        {
                            //TimeSpan? allTime = null;
                            //allTime = (_backupSchedule.BackupExecDateHourStart - DateTime.Now);
                            //_backupSchedule.BackupExecTotalTime = Convert.ToInt32(allTime);
                            _backupSchedule.UploadDateHourFinish = DateTime.Now;
                            var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, true, _setting);

                            if (resp != "sucess")
                                isValid = false;
                            break;
                        }
                }

                return isValid;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private async Task<bool> ExecuteValidateBackup(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
                
                var streamResult = await client.PutAsync(url + "/api/DatabaseService/ValidateBackupExec", stringContent);                

                if (!streamResult.IsSuccessStatusCode)
                {
                    var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true, _setting);

                    var read = await streamResult.Content.ReadAsStringAsync();                                       

                    _utilTelegram.SendMessage($"Falha na validação do backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} \n" + read, _setting.UidTelegramDestino);
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> ExecShrink(Model.ModelBackupSchedule _backupSchedule, Model.Enum.BackupStatus bkpStatus, bool bkpIsRunning, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                _backupSchedule.BackupStatus = bkpStatus;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PostAsync(url + "/api/DatabaseService/Shrink", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                {
                    return await streamResult.Content.ReadAsStringAsync();
                }

                return "sucess";
            }
            catch (Exception err)
            {
                _utilTelegram.SendMessage($"Falha na atualização Status: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n {err.Message}", _setting.UidTelegramDestino);
                return "false";
            }
        }
        
        private async Task<string> ExecChangeRecoveyModelType(ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting, int _recoveryTypeModel)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string url = "http://" + _backupSchedule.CustomerProduct.Server.IpAddress + ":" + _backupSchedule.CustomerProduct.Server.Port;
                if (_recoveryTypeModel == 1)
                    _backupSchedule.RecoveryTypeModel = 1;
                else
                    _backupSchedule.RecoveryTypeModel = 3;

                var json = JsonConvert.SerializeObject(_backupSchedule);
                var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");

                var streamResult = await client.PostAsync(url + "/api/DatabaseService/ChangeRecoveryModel", stringContent);

                if (!streamResult.IsSuccessStatusCode)
                {
                    return await streamResult.Content.ReadAsStringAsync();
                }

                return "sucess";
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha alterar tipo de recuperação: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n {err.Message}", _setting.UidTelegramDestino);
                return "false";
            }
        } 
    }
}
