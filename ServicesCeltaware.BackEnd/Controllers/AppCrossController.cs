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

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class AppCrossController : ControllerBase
    {
        private readonly IRepository<ModelAppCross> _repository;

        public AppCrossController(IRepository<ModelAppCross> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelAppCross _appCross)
        {
            try
            {
                return Ok(await _repository.AddAsynch(_appCross));
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

        [HttpGet]
        public async Task<IActionResult> GetByCustomersProducts(int customersProductsId)
        {
            try
            {
                var result = await _repository.Get()
                                .Include(cp => cp.CustomerProduct).ThenInclude(s => s.Server)
                                .Include(cp => cp.CustomerProduct).ThenInclude(c => c.Customer)
                                .Include(cp => cp.CustomerProduct).ThenInclude(p => p.Product)
                                .Where(cp => cp.CustomerProduct.CustomersProductsId == customersProductsId)
                                .FirstOrDefaultAsync();

                return Ok(result);
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

        [HttpPost]
        public async Task<IActionResult> AddUpdate(ModelAppCross _appCross)
        {
            try
            {
                ModelAppCross newAppCross = await _repository.FindAsynch(_appCross.AppCrossId);

                if (newAppCross == null)
                {
                    await _repository.AddAsynch(_appCross);
                    return Ok(_appCross.AppCrossId);
                }
                else
                {
                    newAppCross.AddressName = _appCross.AddressName;
                    newAppCross.AppCrossId = _appCross.AppCrossId;
                    newAppCross.CustomersProductsId = _appCross.CustomersProductsId;
                    newAppCross.InstallDirectory = _appCross.InstallDirectory;
                    newAppCross.IpAddress = _appCross.IpAddress;
                    newAppCross.IsCreated = _appCross.IsCreated;                    
                    newAppCross.Port = _appCross.Port;

                    await _repository.UpdateAsynch(newAppCross);
                    return Ok(newAppCross.AppCrossId);
                }
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