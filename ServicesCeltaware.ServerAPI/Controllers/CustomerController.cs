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
    public class CustomerController : ControllerBase
    {
        private readonly IRepository<ModelCustomer> _repository;

        public CustomerController(IRepository<ModelCustomer> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> StartCloud(ModelCustomer customer)
        {
            try
            {
                ServicesCeltaWare.Tools.CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                var message = ServicesCeltaWare.Tools.Helpers.CustomerHelpers.CreateSite(customer);
                string messagePool = await ServicesCeltaWare.Tools.Helpers.CustomerHelpers.CreatePool(customer);
                string messageChangePool = await ServicesCeltaWare.Tools.Helpers.CustomerHelpers.ChangePool(customer, ServicesCeltaWare.Model.Enum.ProductName.None);

                customer.IsCloud = true;
                await _repository.UpdateAsynch(customer);

                return Ok(customer.CustomerId);
            }
            catch (Exception err)
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