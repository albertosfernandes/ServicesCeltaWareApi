using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.ServerAPI.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class DatabaseUserController : ControllerBase
    {
        private IRepository<ModelDatabaseUser> _repository;

        public DatabaseUserController(IRepository<ModelDatabaseUser> repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdate([FromBody]List<ModelDatabaseUser> _database)
        {
            try
            {
                int databasesId = 0;

                if (_database.Count > 0)
                {
                    foreach (var d in _database)
                    {
                        ModelDatabaseUser data = await _repository.FindAsynch(d.DatabasesId);
                        
                        if (data == null)
                        {
                            databasesId = await _repository.AddAsynch(d);
                        }
                        else
                        {
                            await _repository.UpdateAsynch(d);
                        }
                    }
                    return Ok(databasesId);
                }
                else
                {
                    return BadRequest("DatabaseUser é nulo.");
                }
                
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