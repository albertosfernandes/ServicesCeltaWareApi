using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
    public class SystemUpdateController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public SystemUpdateController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public DateTime GetDateTimeFileDeploy(ModelCustomerProduct customersettings)
        {
            var _customerSettings = _repository.Find(customersettings.CustomersProductsId);
            string customDirectory = _customerSettings.InstallDirectory.Substring(0, _customerSettings.InstallDirectory.IndexOf("&"));
            FileInfo file = new FileInfo(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\deployment.zip");
            if (file.Exists)
                return file.LastWriteTime;
            else
                return DateTime.MinValue;
        }

        [HttpGet]
        public IActionResult DownloadDeploy(ModelCustomerProduct customersettings)
        {
            var _customerSettings = _repository.Find(customersettings.CustomersProductsId);
            string customDirectory = _customerSettings.InstallDirectory.Substring(0, _customerSettings.InstallDirectory.IndexOf("&"));
            DownloadFile(customDirectory);
            return Ok();
        }

        [HttpGet]
        public IActionResult UpdateBsf(ModelCustomerProduct customersettings)
        {
            var _customerSettings = _repository.Find(customersettings.CustomersProductsId);
            if (UpdateFiles(_customerSettings.InstallDirectory, "bsf"))
            {
                return Ok();
            }
            else
                return NotFound();
        }

        public bool UpdateFiles(string directory, string app)
        {
            string customDirectory = directory.Substring(0, directory.IndexOf("&"));

            if (ValidateFile(customDirectory))
                ExtractFile(customDirectory);
            else
                return false;

            Copy(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\CIP\Release\BSF\", @"c:\Celta Business Solutions\" + customDirectory + @"\bsf\", true);
            
            return true;
        }

        private static void ExtractFile(string customDirectory)
        {
            //extrair
            FileInfo file = new FileInfo(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\CCS\Release\WebService\CeltaLimitService.asmx");
            if(!file.Exists)
            ZipFile.ExtractToDirectory(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\deployment.zip", @"c:\Celta Business Solutions\" + customDirectory + @"\temp");
        }

        private static void DownloadFile(string saveDir)
        {            
            //baixar arquivo deployment
            var wc = new System.Net.WebClient();
            if(ValidateFile(saveDir))
                wc.DownloadFile("http://services.celtaware.com.br/downloads/lastversion/deployment.zip", @"c:\Celta Business Solutions\" + saveDir + @"\temp\deployment.zip");
        }

        private static bool ValidateFile(string directory)
        {
            //validar se existe arquivo
            string currentFile = @"c:\Celta Business Solutions\" + directory + @"\temp\deployment.zip";
            FileInfo fi = new FileInfo(currentFile);
            if (fi.Exists)
            {
                return true;
            }
            return false;
        }

        public void Copy(string rootSource, string rootDestination, bool isOverwrite)
        {
            //Obter os diretórios de origem e destino
            DirectoryInfo dirSource = new DirectoryInfo(rootSource);
            DirectoryInfo dirDestination = new DirectoryInfo(rootDestination);

            if (!dirSource.Exists || !dirDestination.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + rootSource);
            }
            
            // Receber todos arquivos da origem e então copiar para destino
            FileInfo[] files = dirSource.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(rootDestination, file.Name);
                file.CopyTo(temppath, isOverwrite);
            }

            DirectoryInfo[] dirs = dirSource.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string subdirName = subdir.Name;

                Directory.CreateDirectory(Path.Combine(rootDestination, subdirName));
                Copy(subdir.FullName, Path.Combine(rootDestination, subdirName), true);
                                
            }

        }
    }
}