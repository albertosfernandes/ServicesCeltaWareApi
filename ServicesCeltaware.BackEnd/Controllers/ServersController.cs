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
    public class ServersController : ControllerBase
    {
        private IRepository<ModelServer> _repository;
        public ServersController(IRepository<ModelServer> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ModelServer Get(int id)
        {
            var teste = _repository.Get();
            return teste.Where(x => x.ServersId == id).First();
        }

        [HttpGet]
        public List<ModelServer> GetAll()
        {
            return _repository.GetAll();
        }

        [HttpGet]
        public ActionResult<List<ModelServer>> GetAllTeste()
        {
            return _repository.GetAll();
        }

        [HttpPost]
        public IActionResult Add(ModelServer _database)
        {
            _repository.Add(_database);
            return Ok();
        }
    }
}