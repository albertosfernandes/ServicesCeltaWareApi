using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DatabaseServiceController : ControllerBase
    {
        private IRepository<ModelCustomerProduct> _repository;
        public DatabaseServiceController(IRepository<ModelCustomerProduct> repository)
        {
            this._repository = repository;
        }

        [HttpGet]
        public ModelCustomerProduct Get(int id)
        {
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).
                Include(p => p.Product).
                Where(x => x.CustomersProductsId == id).
                First();
        }
    }
}