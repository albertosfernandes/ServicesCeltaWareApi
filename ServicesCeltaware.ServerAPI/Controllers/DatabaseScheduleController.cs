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

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class DatabaseScheduleController : ControllerBase
    {
        private IRepository<ModelBackupSchedule> _repository;

        public DatabaseScheduleController(IRepository<ModelBackupSchedule> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ModelBackupSchedule Get(int id)
        {
            try
            {
                var teste = _repository.Get();
                return teste.
                    Include(c => c.CustomerProduct).
                    ThenInclude(s => s.Server).
                    Where(x => x.CustomersProductsId == id).
                    First();
            }
            catch(Exception err)
            {
                throw err;
            }            
        }

        [HttpGet]
        public async Task<ModelBackupSchedule> GetByCustomerProduct(int customerProductId)
        {
            try
            {
                return await _repository.Get()
             .Include(c => c.CustomerProduct)
             .Include(s => s.Databases)
             .Where(b => b.CustomersProductsId == customerProductId)
             .FirstOrDefaultAsync();
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        [HttpGet]
        public async Task<List<ModelBackupSchedule>> GetAllByCustomerProduct(int customerProductId)
        {
            try
            {
                return await _repository.Get()
               .Include(c => c.CustomerProduct).ThenInclude(cps => cps.Server)
               .Include(s => s.Databases)
               .Where(b => b.CustomersProductsId == customerProductId)
               .ToListAsync();
            }
            catch(Exception err)
            {
                throw err;
            }           
        }

        [HttpGet]
        public async Task<List<ModelBackupSchedule>> GetAllByTime(int hourSchedule)
        {
            try
            {                
                return await _repository.Get()
                        .Include(c => c.CustomerProduct).ThenInclude(cps => cps.Server)
                        .Include(s => s.Databases)
                        .Where(t => t.DateHourExecution.Hour == hourSchedule && t.DateHourLastExecution.Date != DateTime.Now.Date)
                        .ToListAsync();
            }
            catch(Exception err)
            {
                throw err;
            }            
        }

        [HttpGet]
        public List<ModelBackupSchedule> GetAll(int id)
        {
            try
            {
                return _repository.Get()
                            .Include(cp => cp.CustomerProduct)
                                .ThenInclude(se => se.Server)
                            .Where(c => c.CustomersProductsId == id)
                            .ToList();

            }
            catch(Exception err)
            {
                throw err;
            }
        }

        [HttpGet]
        public List<ModelBackupSchedule> GetAllDatabases(int serverId)
        {
            var result = _repository.Get()
                        .Include(cp => cp.CustomerProduct)
                            .ThenInclude(c => c.Customer)
                        // .Include(s => s.CustomerProduct)
                        .Include(d => d.Databases);

            var test = result
                        .Where(x => x.CustomerProduct.ServersId == serverId).ToList();

            return test;
        }

        [HttpPost]
        public ActionResult Add([FromBody]ModelBackupSchedule backupSchedule)
        {
            try
            {
                _repository.Add(backupSchedule);
                return Ok();
            }
            catch(Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpPut]
        public IActionResult UpdateStatus(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                var bkpSchedule = _repository.Find(_databaseSchedule.BackupScheduleId);
                bkpSchedule.BackupStatus = _databaseSchedule.BackupStatus;
                bkpSchedule.GoogleDriveFileId = _databaseSchedule.GoogleDriveFileId;
                bkpSchedule.DateHourLastExecution = DateTime.Now;
                _repository.Update(bkpSchedule);
                return Ok();
            }
            catch(Exception err)
            {
                return BadRequest(err.Message + "\n" + err.InnerException.Message);
            }
        }

        [HttpPut]
        public IActionResult UpdateDateHourLastExecution(ModelBackupSchedule _databaseSchedule)
        {
            try
            {
                var bkpSchedule = _repository.Find(_databaseSchedule.BackupScheduleId);
                bkpSchedule.DateHourLastExecution = DateTime.Now;
                _repository.Update(bkpSchedule);
                return Ok();
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> ValidateBackupExec(ModelBackupSchedule _databaseSchedule)
        //{
        //    try
        //    {
        //        var backupValidate = _repository.Find(_databaseSchedule.BackupScheduleId);


        //        return Ok();
        //    }
        //    catch(Exception err)
        //    {
        //        return BadRequest(err.Message);
        //    }
        //}

    }
}