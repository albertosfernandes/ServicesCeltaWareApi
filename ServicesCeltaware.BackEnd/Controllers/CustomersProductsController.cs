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
using static ServicesCeltaware.BackEnd.Enum;

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

        public IList<ModelCustomerProduct> GetAll(int id)
        {            
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).                
                Include(p => p.Product).
                Where(x => x.CustomerId == id).
                ToList();                                    
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

        [HttpPost]
        public IActionResult Add(ModelCustomerProduct _customerProduct)
        {
            try
            {
                _repository.Add(_customerProduct);
                var customerProductId = _customerProduct.CustomersProductsId;
                return Ok(customerProductId);
            }
            catch(Exception err)
            {
                return Ok(err.Message);
            }
        }

        [HttpPost]
        public IActionResult CreateCustomerProduct(ModelCustomerProduct _customerProduct)
        {
            try
            {
                var customerProduct = _repository.Get()
                    .Include(c => c.Customer)
                    .Where(cp => cp.CustomersProductsId == _customerProduct.CustomersProductsId)
                    .First();

                string error;
                ProductName product = ProductName.BSF;
                switch (_customerProduct.ProductId)
                {
                    case 1: product = ProductName.BSF; break;
                    case 2: product = ProductName.CCS; break;
                    case 3: product = ProductName.CSS; break;
                    case 4: product = ProductName.Concentrador; break;
                    case 5: product = ProductName.SynchronizerService; break;
                    case 6: product = ProductName.Database; break;
                    case 7: product = ProductName.CertificadoA1; break;
                }
                string message = CustomerProductHelpers.CreateProducts(product, customerProduct, out error);
                if (!String.IsNullOrEmpty(error))
                {
                    return NotFound(error);
                }
                return Ok();
            }
            catch(Exception err)
            {
                //return BadRequest(err.Message);
                throw err;
            }
        }
    }
}