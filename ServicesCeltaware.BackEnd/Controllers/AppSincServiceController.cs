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
    public class AppSincServiceController : ControllerBase
    {
        private readonly IRepository<ModelAppSincService> _repository;

        public AppSincServiceController(IRepository<ModelAppSincService> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelAppSincService _appSincService)
        {
            try
            {
                return Ok(await _repository.AddAsynch(_appSincService));
            }
            catch (Exception err)
            {
                if(err.InnerException != null)
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
        public async Task<IActionResult> AddUpdate(ModelAppSincService _appSincService)
        {
            try
            {
                ModelAppSincService newAppSincService = await _repository.FindAsynch(_appSincService.AppSincServicesId);

                if (newAppSincService == null)
                {
                    await _repository.AddAsynch(_appSincService);
                    return Ok(_appSincService.AppSincServicesId);
                }
                else
                {
                    newAppSincService.AddressName = _appSincService.AddressName;
                    newAppSincService.AppSincServicesId = _appSincService.AppSincServicesId;
                    newAppSincService.CustomersProductsId = _appSincService.CustomersProductsId;
                    newAppSincService.InstallDirectory = _appSincService.InstallDirectory;
                    newAppSincService.IpAddress = _appSincService.IpAddress;
                    newAppSincService.IsCreated = _appSincService.IsCreated;
                    newAppSincService.SynchronizerServiceName = _appSincService.SynchronizerServiceName;
                    newAppSincService.Port = _appSincService.Port;

                    await _repository.UpdateAsynch(newAppSincService);
                    return Ok(newAppSincService.AppSincServicesId);
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