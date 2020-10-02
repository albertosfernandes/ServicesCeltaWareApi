using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using ServicesCeltaware.BackEnd.Helpers;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [EnableCors("BasePolicy")]
    public class CertificateController : ControllerBase
    {
        private readonly IRepository<ModelCertificate> _repository;

        public CertificateController(IRepository<ModelCertificate> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IList<ModelCertificate> GetAll(int id)
        {
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).                
                Where(x => x.CustomerId == id).
                ToList();
        }

        [HttpGet]
        public ModelCertificate Get(int id)
        {
            var teste = _repository.Get();
            return teste.Include(c => c.Customer).                
                Where(x => x.CertificateId == id).
                First();
        }

        [HttpPost]
        public IActionResult Add(ModelCertificate certificate)
        {
            try
            {                
                _repository.Add(certificate);
                var certificateProductId = certificate.CertificateId;                
                var certificateCustomer = _repository.Get().Include(c => c.Customer).Where(c => c.CertificateId == certificateProductId).First();                
                
                //Adicionar o diretório na estrutura de arquivos !!! e depois fazer o upload do arquivo!!                
                if (CertificateHelpers.CreateFileRepositorie(Path.Combine(@"c:\Celta Business Solutions\" + certificateCustomer.Customer.RootDirectory + @"\bsf\certificados", certificateCustomer.FileRepositorie)))
                {
                    //faça o upload
                }
                else
                {
                    return NotFound();
                }
                
                return Ok(certificateProductId);
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormCollection certificateFile)
        {
            try
            {                
                var id = certificateFile.First();

                var certificate = _repository.Get()
                            .Include(c => c.Customer).
                                Where(x => x.CertificateId == Convert.ToInt32(id.Value[0])).
                                First();
                string fullPath = Path.Combine(@"c:\celta business Solutions\" + certificate.Customer.RootDirectory + @"\bsf\certificados\", certificate.FileRepositorie);

                // vai vir um array de objetos um file e um como key text .. validar isso!!
                foreach (var file in certificateFile.Files)
                {
                    if (file.Length <= 0)
                        return NotFound("Arquivo não subiu corretamente tente novamente!");
                    var filePath = Path.Combine(fullPath, file.FileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    certificate.FileName = file.FileName;
                    _repository.Update(certificate);
                }
                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }       

        [HttpPost]
        public IActionResult InstallCertificate([FromBody] int certificateIdForInstall)
        {
            try
            {
                var certificate = _repository.Get()
                           .Include(c => c.Customer).
                               Where(x => x.CertificateId == certificateIdForInstall).
                               First();
                //string fileName = certificate.FileName;
                //string fullpath = @"C:\Celta Business Solutions\" + certificate.Customer.RootDirectory + @"\bsf\certificados\" + certificate.FileRepositorie;

                //1- Pegar data de Expiração, NotAfter:
                var dateOfExpiration = CertificateHelpers.GetNotAfter(certificate);
                //2- Pegar Impressão Digital Thumbprint HashCert
                var certHash = CertificateHelpers.GetHashCert(certificate);
                //3- Atualizar o model
                certificate.DateHourExpiration = dateOfExpiration;
                certificate.HashCert = certHash;
                certificate.IsInstalled = true;
                _repository.Update(certificate);
                //4- Agora é instalar mesmo!!!
                var responseInstallCert = CertificateHelpers.InstallCert(certificate);
                if (!String.IsNullOrEmpty(responseInstallCert)) 
                {
                    //deu erro!
                }
                //5- Atribuir permissão todos 
                var responseChangerPermission = CertificateHelpers.ChangePermissions(certificate);
                if (!String.IsNullOrEmpty(responseChangerPermission))
                {
                    //deu erro!!
                }
                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }        

        [HttpGet]
        public IActionResult GetCertificateFile(int id)
        {
            var certificate = _repository.Get()
                           .Include(c => c.Customer).
                               Where(x => x.CertificateId == id).
                               First();
            string path = $"C:\\Celta Business Solutions\\{certificate.Customer.RootDirectory}\\BSF\\certificados\\{certificate.FileRepositorie}\\";
            string filePathFull = Path.Combine(path,certificate.FileName);


            if (SystemFilesHelper.FileExist(filePathFull))
            {
                IFileProvider provider = new PhysicalFileProvider(path);
                IFileInfo fileInfo = provider.GetFileInfo(certificate.FileName);
                var readStream = fileInfo.CreateReadStream();
                // "application/octet-stream"
                return File(readStream, "application/octet-stream", certificate.FileName);
            }
            return NotFound("arquivo não encontrado");
        }

        [HttpGet]
        public IActionResult Remove(int id)
        {
            try
            {
                var certificate = _repository.Get()
                                  .Include(c => c.Customer)
                                  .Where(x => x.CertificateId == id).
                                  First();
                
                var response = CertificateHelpers.Remove(certificate);

                _repository.Delete(certificate);
                return Ok();
            }
            catch(Exception err)
            {
                throw err;
            }
        }
    }
}