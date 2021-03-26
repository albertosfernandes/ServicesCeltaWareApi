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
            try
            {
                var teste = _repository.Get();
                return await teste.
                    Include(c => c.CustomerProduct).
                    Where(x => x.CustomersProductsId == id).
                    FirstOrDefaultAsync();
            }
            catch(Exception err)
            {
                throw err;
            }
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

                if(message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") 
                    || message.Contains("Erro") || message.Contains("Invalid filename")
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

        [HttpPost]
        public async Task<IActionResult> Shrink(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string result = await DatabaseServiceHelper.GenerateScriptShrink(_databaseSchedule);
                if(String.IsNullOrEmpty(result))
                {
                    return BadRequest("Falha na criação do script Shrink");
                }
                string command = $"docker exec -i {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword} -i /var/opt/mssql/{_databaseSchedule.Directory}/Shrink{_databaseSchedule.Databases.DatabaseName}.sql";
                
                string message = await DatabaseServiceHelper.Execute(command);

                if (message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") 
                    || message.Contains("Erro") || message.Contains("Invalid filename"))
                {
                    return BadRequest(message + "\n" +  $"DBCC SHRINKFILE ({_databaseSchedule.Databases.DatabaseName}, 0, TRUNCATEONLY);");
                }
                else
                {
                    return Ok(message);
                }
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRecoveryModel(ModelBackupSchedule _databaseSchedule)
        {
            // 1 = FULL
            // 3 = SIMPLE
            try
            {
                string resp = null;
                switch (_databaseSchedule.RecoveryTypeModel)
                {
                    case 1:
                        {
                            string valid = await DatabaseServiceHelper.ReturnRecoveryModelType(_databaseSchedule);
                            if (valid.Equals("1"))
                            {
                                //não faça nada pois já esta como FULL!!!
                            }
                            else
                            {
                                resp = await DatabaseServiceHelper.ChangeRecoveryModelType(_databaseSchedule, "1");
                            }
                            break;
                        }
                    case 3:
                        {
                            string valid = await DatabaseServiceHelper.ReturnRecoveryModelType(_databaseSchedule);
                            if (valid.Equals("3"))
                            {
                                //não faça nada pois já esta como SIMPLE!!!
                            }
                            else
                            {
                                resp = await DatabaseServiceHelper.ChangeRecoveryModelType(_databaseSchedule, "3");
                            }
                            break;
                        }
                    default:
                        {
                            return BadRequest($"Opção inválida:{_databaseSchedule.RecoveryTypeModel.ToString()}. 1 para FULL ou 3 para SIMPLE.");                            
                        }
                }

                if (!String.IsNullOrEmpty(resp))
                {
                    return BadRequest(resp);
                }

                return Ok();
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ValidateBackupExec(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string scriptValidate = null;
                scriptValidate = await DatabaseServiceHelper.GenerateScriptValidate(_databaseSchedule, ServicesCeltaWare.Model.Enum.ValidateType.LabelOnly);
                string message = await DatabaseServiceHelper.Execute(scriptValidate);

                if (message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") 
                   || message.Contains("Erro") || message.Contains("Invalid filename")
                   || message.Contains("is terminating abnormally"))
                {
                    return BadRequest(message + scriptValidate);
                }

                scriptValidate = await DatabaseServiceHelper.GenerateScriptValidate(_databaseSchedule, ServicesCeltaWare.Model.Enum.ValidateType.VerifyOnly);
                message += await DatabaseServiceHelper.Execute(scriptValidate);

                if (message.Contains("Sqlcmd: Error:") || message.Contains("Incorrect syntax") || message.Contains("Unknown Option") || message.Contains("Erro")
                   || message.Contains("is terminating abnormally")/* && !message.Contains("BACKUP DATABASE successfully")*/)
                {
                    return BadRequest(message + scriptValidate);
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
            string message = await Helpers.DatabaseServiceHelper.Execute(command);

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody]ModelDatabase _database)
        {
            try
            {
                int databasesId = await _repository.AddAsynch(_database);
                foreach (var d in _database.DatabaseUsers)
                {

                    d.DatabasesId = databasesId;
                }
                await _repository.UpdateAsynch(_database);

                return Ok();
            }
            catch(Exception err)
            {
                if (err.InnerException != null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdate([FromBody]ModelDatabase _database)
        {
            try
            {
                ModelDatabase data = await _repository.FindAsynch(_database.DatabasesId);
                int databasesId = 0;

                if (data == null)
                {
                    databasesId = await _repository.AddAsynch(_database);
                    //foreach (var d in _database.DatabaseUsers)
                    //{

                    //    d.DatabasesId = databasesId;
                    //}
                    //await _repository.UpdateAsynch(_database);
                }
                else
                {
                    await _repository.AddAsynch(_database);
                }
               
                return Ok(databasesId);
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            }
        }

    }
}