using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaware.ServerAPI.Helpers;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.Tools;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class DatabaseServiceController : ControllerBase
    {
        private IRepository<ModelDatabase> _repository;
        public DatabaseServiceController(IRepository<ModelDatabase> repository)
        {
            this._repository = repository;
        }

        [HttpGet]
        public async Task<ModelDatabase> GetByCustomerId(int id)
        {
            var teste = _repository.Get();
            return await teste.
                Include(c => c.CustomerProduct).
                Where(x => x.CustomersProductsId == id).
                FirstOrDefaultAsync();
        }

        [HttpGet]
        public List<ModelDatabase> GetAllByServer(int serverId)
        {
            try
            {
                return _repository.Get()
                    .Include(c => c.CustomerProduct)
                    .Where(s => s.CustomerProduct.ServersId == serverId)
                    .ToList();

            }
            catch (Exception err)
            {
                throw err;
            }
        }
      

        [HttpPost]
        public async Task<IActionResult> BackupExec(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string scriptBackup = await DatabaseServiceHelper.GenerateScriptBackup(_databaseSchedule);

                if (scriptBackup.Contains("back"))
                {
                    scriptBackup = await DatabaseServiceHelper.GenerateScriptBackup(_databaseSchedule);
                }

                string message = await DatabaseServiceHelper.Execute(scriptBackup);

                if(message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") || message.Contains("Erro") 
                    && !message.Contains("BACKUP DATABASE successfully") )
                {
                    return BadRequest(message + scriptBackup);
                }
                else
                {
                    // 1- testar se backup está integro
                    // 2- Gravar ultima execução
                    return Ok(message);
                }
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ValidateBackupExec(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string scriptValidate = null;
                scriptValidate = await DatabaseServiceHelper.GenerateScriptValidate(_databaseSchedule, ServicesCeltaWare.Model.Enum.ValidateType.LabelOnly);
                string message = await DatabaseServiceHelper.Execute(scriptValidate);

                if (message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") || message.Contains("Erro")
                    /* && !message.Contains("BACKUP DATABASE successfully")*/)
                {
                    return BadRequest(message + scriptValidate);
                }            
                else
                {
                    scriptValidate = await DatabaseServiceHelper.GenerateScriptValidate(_databaseSchedule, ServicesCeltaWare.Model.Enum.ValidateType.VerifyOnly);
                    message += await DatabaseServiceHelper.Execute(scriptValidate);
                    if (message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") || message.Contains("Erro")
                    /* && !message.Contains("BACKUP DATABASE successfully")*/)
                    {
                        return BadRequest(message + scriptValidate);
                    }                    
                }
                return Ok(message);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Restart(ModelDatabase _database)
        {
            string command = $"docker restart {_database.ConteinerName}";
            string message = " ";
            message = await Helpers.DatabaseServiceHelper.Execute(command);

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelDatabase _database)
        {
            //List<ModelBackupSchedule> lstbkp = new List<ModelBackupSchedule>();
            //ModelBackupSchedule bkpSchedule = new ModelBackupSchedule();
            //ModelCustomerProduct cp = new ModelCustomerProduct();
            //_database.BackupSchedule = bkpSchedule;
            //_database.CustomerProduct = cp;
            //_database.BackupsSchedules = lstbkp;
            _repository.Add(_database);
            return Ok();
        }
    }
}