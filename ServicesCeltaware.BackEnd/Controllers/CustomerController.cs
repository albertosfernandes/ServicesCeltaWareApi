using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<List<ModelCustomer>> Find(string valueSearch)
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

                var _customers = await  _repository.GetAllAsynch();

                if (valueSearch.Equals("all") || String.IsNullOrEmpty(valueSearch))
                {
                    return _customers.OrderBy(s => s.FantasyName).ToList();   
                }

                return (from customer in _customers
                            where customer.FantasyName.ToUpperInvariant().Contains(valueSearch.ToUpperInvariant()) ||
                           customer.Cnpj.Contains(valueSearch) ||
                           customer.CodeCeltaBs == codeBS
                           orderby customer.FantasyName
                           select customer).ToList();

            }
            catch(Exception err)
            {
                throw err;
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
                ServicesCeltaWare.Tools.CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                var message = CustomerHelpers.CreateSite(customer);
                string messagePool = await CustomerHelpers.CreatePool(customer);
                string messageChangePool = await CustomerHelpers.ChangePool(customer, ServicesCeltaWare.Model.Enum.ProductName.None);

                customer.IsCloud = true;
                await _repository.UpdateAsynch(customer);
                
                return Ok(message);
            }
            catch(Exception err)
            {
                if(err.InnerException == null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormCollection _file)
        {
            try
            {
                var id = _file.First();

                var customer = _repository.Get()
                                .Where(x => x.CustomerId == Convert.ToInt32(id.Value[0])).
                                First();
                string fullPath = Path.Combine(@"c:\celta business Solutions\" + customer.RootDirectory + @"\Upload\");

                // vai vir um array de objetos um file e um como key text .. validar isso!!
                foreach (var file in _file.Files)
                {
                    if (file.Length <= 0)
                        return BadRequest("Arquivo não subiu corretamente tente novamente!");

                    var filePath = Path.Combine(fullPath, file.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }                    
                }
                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }
    }
}