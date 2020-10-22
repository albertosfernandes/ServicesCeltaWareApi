using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
      

        [HttpGet]
        public async Task<IActionResult> BackupExec(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                string commandBackup = $"docker exec -it {_databaseSchedule.Databases.ConteinerName} /opt/mssql-tools/bin/sqlcmd -L localhost -U sa -P {_databaseSchedule.CustomerProduct.LoginPassword} -Q ";
                commandBackup += $"'BACKUP DATABASE [{_databaseSchedule.Databases.DatabaseName}] TO DISK = N'";
                if (_databaseSchedule.Type == ServicesCeltaWare.Model.Enum.BackuypType.Full)
                    commandBackup += $"'/var/opt/mssql/backup/{_databaseSchedule.Databases.DatabaseName}BackupFull.bak'";
                else
                    commandBackup += $"'/var/opt/mssql/backup/{_databaseSchedule.Databases.DatabaseName}BackupDiff{_databaseSchedule.DateHourExecution.ToString()}.bak'";

                commandBackup += $" WITH NOFORMAT, INIT, NAME = N'CeltaBS{_databaseSchedule.Databases.DatabaseName} Banco de Dados Backup', SKIP, NOREWIND, NOUNLOAD, COMPRESSION,  STATS = 10";

                string message = await Helpers.DatabaseServiceHelper.Execute(commandBackup);

                return Ok(message);
            }
            catch(Exception err)
            {
                throw err;
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