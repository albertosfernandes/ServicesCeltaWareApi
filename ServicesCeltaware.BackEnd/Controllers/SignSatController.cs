using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
//using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
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

        [HttpGet]
        public async Task<IActionResult> GenerateKey(string cnpjCustomer)
        {
            try
            {
                string cnpjFull = "05865503000143" + cnpjCustomer;
                byte[] entrada = Encoding.ASCII.GetBytes(cnpjFull);
                X509Store store = new X509Store(StoreLocation.LocalMachine);

                string keyCripted = null;
                // RSA _rsa = null;


                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certs = store.Certificates.Find(
                    X509FindType.FindBySerialNumber
                    , "0fa03a28ec37b5ef5e2cfb3ab87b7cd2"
                    , true);
                store.Close();
                X509Certificate2 x509Cert = new X509Certificate2(certs[0]);

                using (var rsa = x509Cert.GetRSAPrivateKey())
                {
                    // byte[] encryptedDataBuffer = rsa.Encrypt(entrada, RSAEncryptionPadding.OaepSHA256);
                    var sig = rsa.SignData(entrada, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    keyCripted = Convert.ToBase64String(sig);
                }

                return Ok(keyCripted);
            }
            catch (Exception err)
            {
                return BadRequest(err.Message);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GenerateKeyTeste(string cnpj)
        {
            try
            {
                string cnpjFull = "05865503000143" + cnpj;
                byte[] entrada = Encoding.ASCII.GetBytes(cnpjFull);
                X509Store store = new X509Store(StoreLocation.LocalMachine);

                string keyCripted = null;
                // RSA _rsa = null;


                store.Open(OpenFlags.ReadOnly);

                X509Certificate2Collection certs = store.Certificates.Find(
                    X509FindType.FindBySerialNumber
                    , "0fa03a28ec37b5ef5e2cfb3ab87b7cd2"
                    , true);
                store.Close();
                X509Certificate2 x509Cert = new X509Certificate2(certs[0]);

                using (var rsa = x509Cert.GetRSAPrivateKey())
                {
                    // byte[] encryptedDataBuffer = rsa.Encrypt(entrada, RSAEncryptionPadding.OaepSHA256);
                    var sig = rsa.SignData(entrada, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    keyCripted = Convert.ToBase64String(sig);
                }
                 
                return Ok(keyCripted);
            }
            catch(Exception err)
            {
                return BadRequest(err.Message);
            }
        }
        
    }
}