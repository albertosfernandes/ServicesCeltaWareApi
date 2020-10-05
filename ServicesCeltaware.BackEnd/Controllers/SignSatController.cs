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
    [Route("api/[controller]")]
    [ApiController]
    public class SignSatController : ControllerBase
    {
        private readonly IRepository<ModelSignSat> _repository;

        public SignSatController(IRepository<ModelSignSat> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Find(string cnpjCustomer)
        {
            //validar se string contem . \ ou - retirar deixar apenas numeros para fazer a consulta

            //procura se existe assinatura para o cnpj em questão
            var result = _repository.Get().Where(s => s.CnpjCustomer == cnpjCustomer);
            return Ok();
        }
    }
}