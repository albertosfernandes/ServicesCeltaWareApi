using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SystemCreateController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public SystemCreateController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Create()
        {
            try
            {
                return Ok();
            }
            catch(Exception err)
            {
                throw err;
            }
        }
    }
}