using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class ProductController : ControllerBase
    {
        private readonly IRepository<ModelProduct> _repository;

        public ProductController(IRepository<ModelProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IList<ModelProduct> GetAll()
        {
            return _repository.GetAll();
        }

        [HttpPost]
        public void Add(ModelProduct product)
        {
            _repository.Add(product);
        }
    }
}