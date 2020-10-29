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
            var teste = _repository.Get();
            return teste.
                Include(c => c.CustomerProduct).
                ThenInclude(s => s.Server).
                Where(x => x.CustomersProductsId == id).
                First();
        }

        [HttpGet]
        public async Task<ModelBackupSchedule> GetByCustomerProduct(int customerProductId)
        {
            return await _repository.Get()
             .Include(c => c.CustomerProduct)
             .Include(s => s.Databases)
             .Where(b => b.CustomersProductsId == customerProductId)
             .FirstOrDefaultAsync();
        }

        [HttpGet]
        public async Task<List<ModelBackupSchedule>> GetAllByCustomerProduct(int customerProductId)
        {
            return await _repository.Get()
                .Include(c => c.CustomerProduct)
                .Include(s => s.Databases)
                .Where(b => b.CustomersProductsId == customerProductId)
                .ToListAsync();
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
        public IActionResult Add(ModelBackupSchedule backupSchedule)
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
    }
}