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
    public class UsersController : ControllerBase
    {
        private readonly IRepository<ModelUser> _repository;

        public UsersController(IRepository<ModelUser> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IList<ModelUser> GetAll()
        {
            return _repository.GetAll();
        }       

        [HttpGet]
        public IActionResult ChangeStatus(int id)
        {
            var user = _repository.Find(id);

            user.IsConnected = !user.IsConnected;

            _repository.Update(user);
            return Ok();
        }

    }
}