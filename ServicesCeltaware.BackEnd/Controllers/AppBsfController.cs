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
using static ServicesCeltaWare.Model.Enum;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class AppBsfController : ControllerBase
    {
        private readonly IRepository<ModelAppBsf> _repository;

        public AppBsfController(IRepository<ModelAppBsf> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var result = await _repository.Get()
                                .Include(cp => cp.CustomerProduct).ThenInclude(s => s.Server)
                                .Include(cp => cp.CustomerProduct).ThenInclude(c => c.Customer)
                                .Include(cp => cp.CustomerProduct).ThenInclude(p => p.Product)
                                .Where(bsf => bsf.AppBsfsId == id)
                                .FirstAsync();
                
                return Ok(result);
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

        [HttpGet]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            try
            {
                var result = await _repository.Get()
                                .Include(cp => cp.CustomerProduct).ThenInclude(s => s.Server)
                                .Include(cp => cp.CustomerProduct).ThenInclude(c => c.Customer)
                                .Include(cp => cp.CustomerProduct).ThenInclude(p => p.Product)
                                .Where(cp => cp.CustomerProduct.CustomerId == customerId && cp.CustomerProduct.ProductId == 1)
                                .ToListAsync();

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
        public async Task<IActionResult> Add(ModelAppBsf _appBsf)
        {
            try
            {
                await _repository.AddAsynch(_appBsf);
                return Ok(_appBsf.AppBsfsId);
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
        public async Task<IActionResult> AddUpdate(ModelAppBsf _appBsf)
        {
            try
            {
                ModelAppBsf newAppBsf = await _repository.FindAsynch(_appBsf.AppBsfsId);

                if (newAppBsf == null)
                {
                    await _repository.AddAsynch(_appBsf);
                    return Ok(_appBsf.AppBsfsId);
                }
                else
                {
                    newAppBsf.AddressName = _appBsf.AddressName;
                    newAppBsf.AppBsfsId = _appBsf.AppBsfsId;
                    newAppBsf.CustomersProductsId = _appBsf.CustomersProductsId;
                    newAppBsf.InstallDirectory = _appBsf.InstallDirectory;
                    newAppBsf.IpAddress = _appBsf.IpAddress;
                    newAppBsf.IsCreated = _appBsf.IsCreated;
                    newAppBsf.Password = _appBsf.Password;
                    newAppBsf.Port = _appBsf.Port;
                    newAppBsf.UserName = _appBsf.UserName;                    

                    await _repository.UpdateAsynch(newAppBsf);
                    return Ok(newAppBsf.AppBsfsId);
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

        [HttpPost]
        public async Task<IActionResult> CreateInCloud(ModelAppBsf _appBsf)
        {
            try
            {
                ProductName product = ProductName.BSF;
                string message = await CustomerProductHelpers.CreateProducts(product, _appBsf.CustomerProduct);
                if (message.Contains("ERROR"))
                {
                    return BadRequest(message);
                }
                return Ok(message);
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
    }
}