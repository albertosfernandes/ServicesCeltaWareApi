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
    public class ConcentratorController : ControllerBase
    {
        private readonly IRepository<ModelConcentrator> _repository;

        public ConcentratorController(IRepository<ModelConcentrator> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> Add(ModelConcentrator _appConc)
        {
            try
            {
                return Ok(await _repository.AddAsynch(_appConc));
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            }
        }
    }
}