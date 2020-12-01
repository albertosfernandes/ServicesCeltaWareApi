using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.UtilitariosInfra;
using ServicesCeltaWare.UtilitariosInfra.GoogleApiStandard;

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

        [HttpPost]
        public async Task<IActionResult> Upload(ModelBackupSchedule _backupSchedule)
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);
                string fileName;

                string path = _backupSchedule.Databases.Directory + "\\backup\\";
                if(_backupSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                {
                    fileName = _backupSchedule.Databases.DatabaseName + "BackupFull.bak";
                }
                else
                {
                    fileName = $"{_backupSchedule.Databases.DatabaseName}BackupDiff{ _backupSchedule.DateHourExecution.Hour.ToString() + _backupSchedule.DateHourExecution.Minute.ToString()}.bak";
                }
                await CallManager.Upload(fileName, path, googleDrive, googleDrive.FolderId);
                return Ok();
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFolders()
        {
            try
            {
                ModelGoogleDrive googleDrive = new ModelGoogleDrive();
                googleDrive = Helpers.HelperGoogleDrive.LoadSetting(_configuration);

                var result = ServicesCeltaWare.UtilitariosInfra.GoogleApiStandard.CallManager.GetAllFoldersService(googleDrive);
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

                var result = ServicesCeltaWare.UtilitariosInfra.GoogleApiStandard.CallManager.ListFilesInFolder(googleDrive);
                return Ok(result);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> TesteGetTokenGoogle()
        {
            try
            {
                var result = await ServicesCeltaWare.Security.GoogleToken.GetWithRSA256();
                return Ok(result);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

    }
}