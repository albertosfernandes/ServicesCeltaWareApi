using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class CustomerProductsController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public CustomerProductsController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdate(ModelCustomerProduct _customerProduct)
        {
            try
            {
                ModelCustomerProduct newCustomerProduct = await _repository.FindAsynch(_customerProduct.CustomersProductsId);
                if (newCustomerProduct == null)
                {
                    await _repository.AddAsynch(_customerProduct);
                    return Ok(_customerProduct.CustomersProductsId);
                }
                else
                {
                    newCustomerProduct.AddressName = _customerProduct.AddressName;
                    newCustomerProduct.ProductId = _customerProduct.ProductId;
                    newCustomerProduct.ServersId = _customerProduct.ServersId;
                    await _repository.UpdateAsynch(newCustomerProduct);
                    return Ok(_customerProduct.CustomersProductsId);
                }
            }
            catch(Exception err)
            {
                if (err.InnerException == null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            } 
        }
    }
}