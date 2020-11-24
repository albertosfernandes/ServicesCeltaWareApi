using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.UtilitariosInfra;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class GoogleDriveServiceController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(ModelBackupSchedule _backupSchedule)
        {
            try
            {
                string filename = _backupSchedule.Databases.Directory + "\\backup\\";
                if(_backupSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                {
                    //full
                    filename += _backupSchedule.Databases.DatabaseName + "BackupFull.bak";
                }
                else
                {
                    filename += $"{_backupSchedule.Databases.DatabaseName}BackupDiff{ _backupSchedule.DateHourExecution.Hour.ToString() + _backupSchedule.DateHourExecution.Minute.ToString()}.bak";
                    //diff
                }
                //var res = CallManager.ListFolders("teste");  
                await CallManager.Upload(filename);
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
                var result = await ServicesCeltaWare.Security.GoogleToken.Get();
                return Ok(result);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

    }
}