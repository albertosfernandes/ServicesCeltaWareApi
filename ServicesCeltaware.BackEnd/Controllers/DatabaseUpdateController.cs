using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ServicesCeltaware.BackEnd.Helpers;
using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class DatabaseUpdateController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public DatabaseUpdateController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> RepairDatabase(int customerSettingsid)
        {
            try
            {
                var _customerSettings = _repository.Get()
               .Include(c => c.Customer)
               .Include(p => p.Product)
               .Where(x => x.CustomersProductsId == customerSettingsid).First();

                string message = null;
                string dirPath = @"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\temp\cip\database\";
                string command = "CeltaWare.CBS.CAT.ExecuteSqlScripts.exe ";
                string arg1 = "\"" + _customerSettings.IpAddress + "," + _customerSettings.Port + "\""
                            + " " + _customerSettings.SynchronizerServiceName
                            + " " + _customerSettings.LoginUser
                            + " " + _customerSettings.LoginPassword
                            + " \"Repair\" 1";

                message = await UpdateDatabase.Execute(dirPath, command, arg1);
                
                if (message.Contains("Error in file") || message.Contains("Error: Unknown"))
                {
                    return NotFound(message);
                }
                return Ok(message);
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateDatabase(int customerSettingsid, string element)
        {
            try
            {
                var _customerSettings = _repository.Get()
               .Include(c => c.Customer)
               .Include(p => p.Product)
               .Where(x => x.CustomersProductsId == customerSettingsid).First();

                string message = null;                
                string dirPath = @"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\temp\cip\database\";
                string command = "CeltaWare.CBS.CAT.ExecuteSqlScripts.exe ";
                string arg1 = "\"" + _customerSettings.IpAddress + "," + _customerSettings.Port + "\""
                            + " " + _customerSettings.SynchronizerServiceName
                            + " " + _customerSettings.LoginUser
                            + " " + _customerSettings.LoginPassword
                            + " " + element + " 0";
                
                message = await UpdateDatabase.Execute(dirPath, command, arg1);

                if (message.Contains("Error in file") || message.Contains("Error: Unknown"))
                {                    
                    return NotFound(message);
                }
                return Ok(message);
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpGet]
        public IActionResult GetKeys(int customerSettingsid)
        {
            try
            {
                var _customerSettings = _repository.Get()
               .Include(c => c.Customer)
               .Include(p => p.Product)
               .Where(x => x.CustomersProductsId == customerSettingsid).First();

                string filePathFull = Path.Combine(@"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\BSF\bin\ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
                string path = Path.Combine(@"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\BSF\bin\ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
               

                if (SystemFilesHelper.FileExist(filePathFull))
                {                    
                    string _path = Path.Combine(@"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\BSF\bin\");
                    IFileProvider provider = new PhysicalFileProvider(_path);
                    IFileInfo fileInfo = provider.GetFileInfo("ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
                    var readStream = fileInfo.CreateReadStream();

                    return File(readStream, "application/x-zip", "ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
                }
                else
                {
                    if(SystemFilesHelper.CompressFileOrFolder(@"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\BSF\bin\", _customerSettings.Customer.RootDirectory))
                    {
                        //baixar o arquivo então
                        string _path = Path.Combine(@"C:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\BSF\bin\");
                        IFileProvider provider = new PhysicalFileProvider(_path);
                        IFileInfo fileInfo = provider.GetFileInfo("ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
                        var readStream = fileInfo.CreateReadStream();

                        return File(readStream, "application/x-zip", "ParChaves" + _customerSettings.Customer.RootDirectory + ".zip");
                    }
                    else
                    {
                        return NotFound("Ocorreu um erro na criação do arquivo compactado.");
                    }
                }
                
            }
            catch(Exception err)
            {
                return NotFound(err.Message);
            }
        }        

        [HttpGet]
        public IActionResult TesteDatabase(int n)
        {
            try
            {
                string mensagem = "CorreioTipoFrete_EmpresasOnInsert.TRG"
                                    + "\br CorreioTipoFrete_EmpresasOnInsteadOfDelete.TRG"
                                    + "\br CorreioTipoFrete_EmpresasOnUpdate.TRG"
                                    + "\br CotacoesCompraOnInsert.TRG"
                                    + "\br DadosEstatisticosProdutosSazonais_EmpresasOnUpdate.TRG"
                                    + "\br DadosEstatisticosProdutos_Empresas_EmbalagensOnDelete.TRG";
                                    
                if(n > 5)
                {
                    return NotFound("Error in file: GetCodeOfInternalExitStockJustificationForDevolution.UDF");
                }
               
                return Ok(mensagem);
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }       
       
    }    
}