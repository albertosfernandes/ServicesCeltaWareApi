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
    public class AppSincWebController : ControllerBase
    {
        private readonly IRepository<ModelAppSincWeb> _repository;

        public AppSincWebController(IRepository<ModelAppSincWeb> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelAppSincWeb _appSincWeb)
        {
            try
            {
                return Ok(await _repository.AddAsynch(_appSincWeb));
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
        public async Task<IActionResult> AddUpdate(ModelAppSincWeb _appSincWeb)
        {
            try
            {
                ModelAppSincWeb newAppSincWeb = await _repository.FindAsynch(_appSincWeb.AppSincWebsId);

                if (newAppSincWeb == null)
                {
                    await _repository.AddAsynch(_appSincWeb);
                    return Ok(_appSincWeb.AppSincWebsId);
                }
                else
                {
                    newAppSincWeb.AddressName = _appSincWeb.AddressName;
                    newAppSincWeb.AppSincWebsId = _appSincWeb.AppSincWebsId;
                    newAppSincWeb.CustomersProductsId = _appSincWeb.CustomersProductsId;
                    newAppSincWeb.InstallDirectory = _appSincWeb.InstallDirectory;
                    newAppSincWeb.IpAddress = _appSincWeb.IpAddress;
                    newAppSincWeb.IsCreated = _appSincWeb.IsCreated;
                    newAppSincWeb.Port = _appSincWeb.Port;

                    await _repository.UpdateAsynch(newAppSincWeb);
                    return Ok(newAppSincWeb.AppSincWebsId);
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