using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class StorageServerController : ControllerBase
    {
        private readonly IRepository<ModelStorageServer> _repository;

        public StorageServerController(IRepository<ModelStorageServer> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ModelStorageServer> Get(int storageServerId)
        {
            try
            {
                return await _repository.FindAsynch(storageServerId);
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        [HttpGet]
        public async Task<List<ModelStorageServer>> GetAll(int serverId)
        {
            try
            {
                return await _repository.Get()
                                .Include(s => s.Server)
                                .Where(s => s.ServersId == serverId)
                                .ToListAsync();

            }
            catch(Exception err)
            {
                throw err;
            }
        }
        
        [HttpGet]
        public async Task<IActionResult> GetallServers()
        {
            try
            {
                var resp = await _repository.Get()
                            .Include(s => s.Server)
                            .GroupBy(s => s.ServersId)
                            .Select(t => t.Key)
                            .ToListAsync();

                return Ok(resp);
            }
            catch(Exception err)
            {
                if (err.InnerException != null)
                {
                    return BadRequest(err.Message + "\n" + err.InnerException.Message);
                }
                return BadRequest(err.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdate(ModelStorageServer _storageServer)
        {
            try
            {                
                ModelStorageServer storage = await _repository.FindAsynch(_storageServer.StorageServerId);

                if(storage == null)
                {
                    int resp = await _repository.AddAsynch(_storageServer);
                    return Ok(resp);
                }
                else
                {
                    storage.Portal = _storageServer.Portal;
                    storage.TargetName = _storageServer.TargetName;
                    await _repository.UpdateAsynch(storage);
                    return Ok(storage.StorageServerId);
                }
            }
            catch(Exception err)
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