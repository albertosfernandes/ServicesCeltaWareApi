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
    public class ConcentratorController : ControllerBase
    {
        private readonly IRepository<ModelConcentrator> _repository;

        public ConcentratorController(IRepository<ModelConcentrator> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelConcentrator _appConc)
        {
            try
            {
                return Ok(await _repository.AddAsynch(_appConc));
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
        public async Task<IActionResult> AddUpdate(ModelConcentrator _appConcentrator)
        {
            try
            {
                ModelConcentrator newAppConcentrator = await _repository.FindAsynch(_appConcentrator.ConcentratorsId);

                if (newAppConcentrator == null)
                {
                    await _repository.AddAsynch(_appConcentrator);
                    return Ok(_appConcentrator.ConcentratorsId);
                }
                else
                {
                    newAppConcentrator.AddressName = _appConcentrator.AddressName;
                    newAppConcentrator.ConcentratorsId = _appConcentrator.ConcentratorsId;
                    newAppConcentrator.CustomersProductsId = _appConcentrator.CustomersProductsId;
                    newAppConcentrator.InstallDirectory = _appConcentrator.InstallDirectory;
                    newAppConcentrator.IpAddress = _appConcentrator.IpAddress;
                    newAppConcentrator.IsCreated = _appConcentrator.IsCreated;
                    newAppConcentrator.Port = _appConcentrator.Port;

                    await _repository.UpdateAsynch(newAppConcentrator);
                    return Ok(newAppConcentrator.ConcentratorsId);
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