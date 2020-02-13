using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.DAL.Customer;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
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

        [HttpGet]
        public IList<ModelCustomer> GetAll()
        {
            System.Threading.Thread.Sleep(5 * 1000);
            return _repository.GetAll();
        }

        [HttpPost]
        public void Add(ModelCustomer customer)
        {
            _repository.Add(customer);
        }
    }
}