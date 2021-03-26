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
            client.Timeout = TimeSpan.FromHours(4);
        }

        public async Task<string> BackupRun(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_setting.UidTelegramToken);
            try
            {
                string resp = null;
                // backup is run!! 
                // Marcar como backup status 2 Runing
                var respUpd = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Runing, false, _setting);
                if (respUpd != "sucess")
                    return respUpd;

                //Antes de efetuar BackupFULL, tem que realizar Shrink
                //if (_backupSchedule.Type == Model.Enum.BackuypType.Full)
                //{
                //    resp = await ExecChangeRecoveyModelType(_backupSchedule, _setting, 3);
                //    if(resp == "sucess")
                //    {
                //        // blz deu certo faz o shrink agora!!
                //        resp = await ExecShrink(_backupSchedule, Model.Enum.BackupStatus.Runing, true, _setting);
                //        if(resp == "sucess")
                //        {
                //            //ok .. tudo certo volte para modelType FULL
                //            resp = await ExecChangeRecoveyModelType(_backupSchedule, _setting, 1);
                //            if(resp == "sucess")
                //            {
                //                //ok .. tudo certo pode fazer backup
                //            }
                //            else
                //            {
                //                _utilTelegram.SendMessage($"Falha ao alterar mode recovery para Completo: " + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                //                return resp;
                //            }
                //        }
                //        else
                //        {
                //            _utilTelegram.SendMessage($"Falha no Shrink: " + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                //            return resp;
                //        }
                //    }
                //    else
                //    {
                //        var respUpdate = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, false, _setting);
                //        if (respUpdate != "sucess")
                //            return respUpdate;

                //        _utilTelegram.SendMessage($"Falha ao alterar mode recovery para Simples: " + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                //        return resp;
                //    }
                //}                

                var isExecuted = await ExecuteDatabaseSchedule(_backupSchedule, _setting);
                if (isExecuted)
                {
                    //backup garantido.. então inicie o upload
                    var googleDriveFileId = await _helperGoogleDrive.UploadBackup(_backupSchedule, _setting);
                    if (googleDriveFileId.Contains("ERRO"))
                    {
                        resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.OutOfDate, true, _setting);
                        _utilTelegram.SendMessage($"Falha no Upload do Backup: {googleDriveFileId}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                        new Exception(googleDriveFileId);
                    }
                    else
                    {
                        _backupSchedule.GoogleDriveFileId = googleDriveFileId;
                        resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Success, true, _setting);
                        if (!resp.Equals("sucess"))
                        {
                            _utilTelegram.SendMessage($"Falha no serviço TaskManager: Erro ao atualizar status do backup, GoogleFileId e Status. " + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                        }
                        //else
                        //{
                        //    resp = await ExecShrink(_backupSchedule, Model.Enum.BackupStatus.Runing, true, _setting);
                        //    if(!resp.Equals("sucess"))
                        //        _utilTelegram.SendMessage($"Falha no serviço TaskManager: Erro ao realizar Shrink database: " + _backupSchedule.Databases.DatabaseName + "\n" + resp, _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                        //}
                    }
                }
                return resp;
            }
            catch (Exception err)
            {
                _utilTelegram.SendMessage($"Falha no serviço TaskManager: " + err.Message + "\n" + err.StackTrace , _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                return err.Message;
            }
        }
            

        public async Task<bool> ExecuteDatabaseSchedule(Model.ModelBackupSchedule _backupSchedule, ModelTaskServiceSettings _setting)
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
                else
                {
                    _utilTelegram.SendMessage($"Falha na execução de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    var responseUpdate = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true, _setting);
                    return false;
                }

                if (response)
                {
                    if (_setting.IsDebug)
                    {
                        _utilTelegram.SendMessage($"Backup Executado com sucesso: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                    }
                    return true;
                }
                _utilTelegram.SendMessage($"Falha ao validar arquivo backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}\n Tipo: {_backupSchedule.Type}", _setting.UidTelegramDestino);
                var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Corrupted, true, _setting);
                return false;

            }
            catch (Exception err)
            {
                var resp = await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true, _setting);
                _utilTelegram.SendMessage($"Falha no sistema de backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToString()} - { err.Message}", _setting.UidTelegramDestino);
                throw err;
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
                    new Exception(errorResponse);
                }


                await UpdateStatusBackup(_backupSchedule, Model.Enum.BackupStatus.Failed, true, _setting);
                
                _utilTelegram.SendMessage($"Falha na execução de Backup: {_backupSchedule.Databases.DatabaseName}:{_backupSchedule.DateHourExecution.ToShortTimeString()}", _setting.UidTelegramDestino);
                return false;
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
