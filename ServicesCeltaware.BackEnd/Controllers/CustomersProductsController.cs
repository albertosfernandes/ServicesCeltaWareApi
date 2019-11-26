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
    public class CustomersProductsController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public CustomersProductsController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        public IList<ModelCustomerProduct> Get(int id)
        {            
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).                
                Include(p => p.Product).
                Where(x => x.CustomerId == id).
                ToList();                                    
        }
    }
}