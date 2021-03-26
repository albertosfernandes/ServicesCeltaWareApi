using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaware.BackEnd.Helpers;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class DatabaseCreateController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public DatabaseCreateController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> CreateConfigs(int id)
        {
            try
            {
                var customerProduct = _repository.Get()
                                        .Include(c => c.Customer)
                                        .Include(p => p.Product)
                                        .Where(x => x.CustomersProductsId == id)
                                        .First();

                await DatabaseHelpers.Create(customerProduct);
                return Ok();
            }
            catch(Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpGet]
        public IActionResult UpdateConnectionString(int id, string celtaBSUserPassword)
        {
            try
            {
                var customerProduct = _repository.Get()
                                        .Include(c => c.Customer)
                                        .Include(p => p.Product)
                                        .Where(x => x.CustomersProductsId == id)
                                        .First();
                
                DatabaseHelpers.GenerateConnectionString(customerProduct, celtaBSUserPassword);

                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

    }
}