using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaware.BackEnd.Helpers;
using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.DAL.Customer;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[EnableCors("BasePolicy")]
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
            var list = _repository.GetAll();
            var studentsInDescOrder = list.OrderBy(s => s.FantasyName);
            return studentsInDescOrder.ToList();
        }

        [HttpGet]
        public ModelCustomer Get(int _customerId)
        {
            return _repository.Find(_customerId);               
        }

        [HttpGet]
        public ModelCustomer Find(string valueSearch)
        {
            try
            {
                int codeBS = 0;
                string pattern = @"^\d{4,6}$";
                Regex defaultRegex = new Regex(pattern);
                bool valid = defaultRegex.IsMatch(valueSearch);

                if (valid)
                {
                    codeBS = Convert.ToInt32(valueSearch);
                }
            
                return _repository.Get()
                    .Where(x => x.CodeCeltaBs == codeBS ||
                                x.Cnpj == valueSearch ||
                                x.FantasyName.Contains(valueSearch) )                
                    .First();            
            }
            catch(Exception err)
            {
                ModelCustomer customerError = new ModelCustomer();
                return customerError;
            }
        }

        [HttpPost]
        public IActionResult Add(ModelCustomer customer)
        {
            try
            {                
                var customerValue = _repository.Find(customer.CustomerId);
                if(customerValue == null)
                {
                    _repository.Add(customer);                    
                    return Ok(customer.CustomerId.ToString());
                }
                else
                {
                    customerValue.Cnpj = customer.Cnpj;
                    customerValue.CodeCeltaBs = customer.CodeCeltaBs;
                    customerValue.CompanyName = customer.CompanyName;
                    customerValue.FantasyName = customer.FantasyName;
                    customerValue.RootDirectory = customer.RootDirectory;
                    _repository.Update(customerValue);                                        
                    return NoContent();
                }
            }
            catch(Exception err)
            {
                return BadRequest(err);
            }
        }

        [HttpPost]
        public async Task<IActionResult> StartCloud(ModelCustomer customer)
        {
            try
            {
                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                var message = CustomerHelpers.CreateSite(customer);
                string messagePool = await CustomerHelpers.CreatePool(customer);
                string messageChangePool = await CustomerHelpers.ChangePool(customer, Enum.ProductName.None);
                
                return Ok(message);
            }
            catch(Exception err)
            {
                throw err;
            }
        }
       
    }
}