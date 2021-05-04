using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.UtilitariosInfra;
using ServicesCeltaWare.UtilitariosInfra.GoogleApiStandard;

using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using ServicesCeltaWare.DAL;
using Microsoft.AspNetCore.Authorization;
using System.IO;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class GoogleDriveServiceController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        
        public GoogleDriveServiceController(IConfiguration configuration)        
        {
            _configuration = configuration;
        }

        [HttpGet]
        //[GoogleScopedAuthorize(DriveService.ScopeConstants.DriveReadonly)]
        public async Task<IActionResult> DriveFileList([FromServices] IGoogleAuthProvider auth)
        {
            GoogleCredential cred = await auth.GetCredentialAsync();
            var service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = cred
            });
            var files = await service.Files.List().ExecuteAsync();
            var fileNames = files.Files.Select(x => x.Name).ToList();
            return Ok(fileNames);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(ModelBackupSchedule _databaseSchedule)
        {
            try
            { 
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
                string path = _databaseSchedule.Databases.Directory + "/" + _databaseSchedule.Directory + "/";
                string backupFileName = null;
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.MysqlFull)
                    backupFileName = $"{_databaseSchedule.Databases.ConteinerName}Backup.sql";
                else
                    backupFileName = Helpers.DatabaseServiceHelper.ReturnBackupName(_databaseSchedule);

                if(_databaseSchedule.GoogleDriveFolderId == null)
                {
                    return BadRequest($"{_databaseSchedule.Databases.DatabaseName} GoogleDriveFolderId nulo!");                
                }

                var resp = await Helpers.HelperGoogleDrive.UploadFromLinux(googleDrive.CredentialFileName, backupFileName, path, _databaseSchedule.GoogleDriveFolderId);

                if(resp.Contains("The access token has expired and could not be refreshed"))
                {
                    HelperLogs.WriteLog("GoogleDriveServiceController. ", resp);
                    var respUpdate = await Helpers.HelperGoogleDrive.UpdateCredential(googleDrive.CredentialFileName);
                    if (respUpdate.ToUpperInvariant().Contains("OK"))
                    {
                        var resp2 = await Helpers.HelperGoogleDrive.UploadFromLinux(googleDrive.CredentialFileName, backupFileName, path, _databaseSchedule.GoogleDriveFolderId);
                    }
                    else
                    {
                        return BadRequest("Não foi possível atualizar arquivo de credencial. \n" + respUpdate);
                    }
                }

                if (resp.Contains("Failed"))
                {
                    HelperLogs.WriteLog("GoogleDriveServiceController", resp);
                    await Task.Delay(2000);
                    var resp2 = await Helpers.HelperGoogleDrive.UploadFromLinux(googleDrive.CredentialFileName, backupFileName, path, _databaseSchedule.GoogleDriveFolderId);
                    if (resp2.Contains("ok"))
                    {
                        resp = resp2;
                    }
                    else
                    {
                        HelperLogs.WriteLog("GoogleDriveServiceController", resp2);
                        return BadRequest("Não foi possível atualizar o backup pela segunda vez!. \n" + resp2);
                    }
                }

                if (resp.Contains("ERRO: Arquivo não existe") || resp.Contains("Error:\"invalid_grant\"")                    
                    || resp.Contains("The operation was canceled"))
                {
                    HelperLogs.WriteLog("GoogleDriveServiceController", resp);
                    return Forbid();
                }

                    return Ok(resp);
            }
            catch(Exception err)
            {                
                if (err.InnerException != null)
                {
                    HelperLogs.WriteLog("GoogleDriveServiceController", err.Message + "\n" + err.InnerException.Message);
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                HelperLogs.WriteLog("GoogleDriveServiceController", err.Message);
                return BadRequest(err.Message);
            }
        }

        //[HttpPost]
        ////[Authorize]
        ////[GoogleScopedAuthorize(DriveService.ScopeConstants.DriveFile)]
        //public async Task<IActionResult> UploadFull([FromServices]IGoogleAuthProvider auth, ModelBackupSchedule _backupSchedule)
        //{
        //    try
        //    {       

        //        ModelGoogleDrive googleDrive = new ModelGoogleDrive();
        //        googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
        //        string fileName;

        //        string path = _backupSchedule.Databases.Directory + "/backup/";
        //        if (_backupSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
        //        {
        //            fileName = _backupSchedule.Databases.DatabaseName + "BackupFull.bak";
        //        }
        //        else
        //        {
        //            fileName = $"{_backupSchedule.Databases.DatabaseName}BackupDiff{ _backupSchedule.DateHourExecution.Hour.ToString() + _backupSchedule.DateHourExecution.Minute.ToString()}.bak";
        //        }

        //       var service = CallManager.TesteService();

        //        //GoogleCredential cred =  await auth.GetCredentialAsync();
        //        //var service = new DriveService(new BaseClientService.Initializer
        //        //{
        //        //    HttpClientInitializer = cred
        //        //});
        //        //service.HttpClient.Timeout = TimeSpan.FromHours(4);


        //        var files = await CallManager.UploadFull(service, fileName, path, googleDrive.FolderId);

        //        return Ok(files);
        //    }
        //    catch (Exception err)
        //    {
        //        return BadRequest(err.Message);
        //    }
           
        //}

        //[HttpPost]
        //public async Task<IActionResult> Upload(ModelBackupSchedule _backupSchedule, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        ModelGoogleDrive googleDrive = new ModelGoogleDrive();
        //        googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
        //        string fileName;

        //        string path = _backupSchedule.Databases.Directory + "/backup/";
        //        if(_backupSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
        //        {
        //            fileName = _backupSchedule.Databases.DatabaseName + "BackupFull.bak";
        //        }
        //        else
        //        {
        //            fileName = $"{_backupSchedule.Databases.DatabaseName}BackupDiff{ _backupSchedule.DateHourExecution.Hour.ToString() + _backupSchedule.DateHourExecution.Minute.ToString()}.bak";
        //        }
        //        var responseUpLoad = await CallManager.Upload(fileName, path, googleDrive, googleDrive.FolderId);

        //        if (responseUpLoad.Contains("ERRO:"))
        //            return BadRequest(responseUpLoad);

        //        return Ok(responseUpLoad);
        //    }
        //    catch(Exception err)
        //    {
        //        return BadRequest(err.Message);
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Up(ModelBackupSchedule _backupSchedule, CancellationToken cancellationToken)
        //{
        //    try
        //    {               
        //        ModelGoogleDrive googleDrive = new ModelGoogleDrive();
        //        googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
        //        string fileName;

        //        string path = _backupSchedule.Databases.Directory + "/backup/";
        //        if (_backupSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
        //        {
        //            fileName = _backupSchedule.Databases.DatabaseName + "BackupFull.bak";
        //        }
        //        else
        //        {
        //            fileName = $"{_backupSchedule.Databases.DatabaseName}BackupDiff{ _backupSchedule.DateHourExecution.Hour.ToString() + _backupSchedule.DateHourExecution.Minute.ToString()}.bak";
        //        }
        //        CallManager c = new CallManager(googleDrive);

        //        // var responseUpLoad = await c.ExemploGoogle(fileName, path, googleDrive, googleDrive.FolderId);  
        //        c.TestApiKey(googleDrive);
        //        return Ok();
        //    }
        //    catch(Exception err)
        //    {
        //        return BadRequest(err.Message);
        //    }
        //}

        [HttpGet]
        public async Task<IActionResult> GetAllFolders()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);

                var result = await CallManager.GetAllFoldersService(googleDrive);
                return Ok(result);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFilesInFolder()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);

                var result = await ServicesCeltaWare.UtilitariosInfra.GoogleApiStandard.CallManager.ListFilesInFolder(googleDrive);
                return Ok(result);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFilesFolder()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);

                //var resultToken = await ServicesCeltaWare.Security.GoogleToken.GetWithRSA256(googleDrive.CredentialFileName);

                var response = await CallManager.GetToken(googleDrive);

                string onlyToken = response.Substring(17, 219);

                var list = await CallManager.ListFilesUri(onlyToken);

                return Ok(list);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormCollection credentialFile)
        {
            try
            {
                foreach (var file in credentialFile.Files)
                {
                    if (file.Length <= 0)
                        return NotFound("Arquivo não subiu corretamente tente novamente!");
                    string fileNameFull = @"/servicesCeltaWare/consolegoogle/credential/" + file.FileName;
                    using (var fileStream = new FileStream(fileNameFull, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                return Ok();
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TesteGetTokenGoogle()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);

                var result = await ServicesCeltaWare.Security.GoogleToken.GetWithRSA256(googleDrive.CredentialFileName);
                return Ok(result);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGoogleToken()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
                var response = await CallManager.GetToken(googleDrive);

                string onlyToken = response.Substring(17, 220);

                return Ok(onlyToken);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
                throw err;
            }
        }

    }
}