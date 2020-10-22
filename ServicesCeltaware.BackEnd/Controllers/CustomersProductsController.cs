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
                Include(s => s.Server).
                Where(x => x.CustomerId == id).
                ToList();                                    
        }

        [HttpGet]
        public ModelCustomerProduct Get(int id)
        {
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).
                Include(p => p.Product).
                Include(s => s.Server).
                Where(x => x.CustomersProductsId == id).
                First();
        }

        [HttpGet]
        public async Task<List<ModelCustomerProduct>> GetAllDatabases(int serverId)
        {
            return await _repository.Get()
                    .Include(c => c.Customer)
                    .Include(s => s.Server)
                    .Include(p => p.Product)
                    .Where(prodId => prodId.ProductId == 6 && prodId.ServersId == serverId)
                    .OrderBy(cp => cp.Customer.FantasyName)
                    .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelCustomerProduct _customerProduct)
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
        public async Task<IActionResult> CreateCustomerProduct(ModelCustomerProduct _customerProduct)
        {
            try
            {
                var customerProduct = _repository.Get()
                    .Include(c => c.Customer)
                    .Include(s => s.Server)
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
                string message = await CustomerProductHelpers.CreateProducts(product, customerProduct);
                if (message.Contains("ERROR"))
                {
                    return NotFound(message);
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