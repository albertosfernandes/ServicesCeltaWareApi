using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.DAL;
using ServicesCeltaWare.Model;

namespace ServicesCeltaware.BackEnd.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    //[EnableCors("BasePolicy")]
    public class SystemUpdateController : ControllerBase
    {
        private readonly IRepository<ModelCustomerProduct> _repository;

        public SystemUpdateController(IRepository<ModelCustomerProduct> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public DateTime GetDateTimeFileDeploy(int customersettingsId, int productId)
        {
            FileInfo file;
            try
            {
                var _customerSettings = _repository.Get()
                     .Include(c => c.Customer)
                     .Where(x => x.CustomersProductsId == customersettingsId).First();
                if(productId == 4)
                {
                    file = new FileInfo(@"c:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\temp\ReleaseConcentrador.zip");
                }
                else
                {
                    file = new FileInfo(@"c:\Celta Business Solutions\" + _customerSettings.Customer.RootDirectory + @"\temp\deployment.zip");
                }
                if (file.Exists)
                    return file.LastWriteTime;
                else
                    return DateTime.MinValue;
            }
            catch (Exception err)
            {
                return DateTime.MinValue;
            }
        }

        [HttpGet]
        public DateTime GetVersionFile(int customersettingsId)
        {
            try
            {
                var _customerSettings = _repository.Get()
                     .Include(c => c.Customer)
                     .Include(p => p.Product)
                     .Where(x => x.CustomersProductsId == customersettingsId).First();

                return VersionFile(_customerSettings);
            }
            catch (Exception err)
            {
                return DateTime.MinValue;
            }
        }

        [HttpGet]
        public IActionResult DownloadDeploy(int id, int productId)
        {
            try
            {
                var customer = _repository.Get()
                  .Include(c => c.Customer)
                  .Where(x => x.CustomerId == id).First();

                DownloadFile(customer.Customer.RootDirectory, productId);
                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpGet]
        public ActionResult<string> StatusService(string servicename)
        {
            ServicesWindows s = new ServicesWindows();
            if (servicename.ToUpper().Contains("CELTAWARE.CBS.CSS."))
            {
                return s.Status(servicename);                
            }
            else
            {
                //string a = "CeltaWare.CBS.CSS." + servicename.Remove(0, 7);
                return s.Status("CeltaWare.CBS.CSS." + servicename.Remove(0, 7));
            }                        
        }
        [HttpGet]
        public IActionResult StarStopService(bool isStart, string servicename)
        {
            try
            {
                if (isStart)
                {
                    StartSynchronizerService(servicename);
                    System.Threading.Thread.Sleep(9 * 1000);
                }
                else
                {
                    StopSynchronizerService(servicename);
                    System.Threading.Thread.Sleep(9 * 1000);
                }
                return Ok();
            }
            catch (Exception err)
            {
                return NotFound(err.Message);
            }
        }

        [HttpPost]
        public IActionResult UpdateCeltaBS(ModelCustomerProduct customersettings)
        {
            string msgError;
            try
            {
                var _customerSettings = _repository.Get()
                .Include(c => c.Customer)
                .Include(p => p.Product)
                .Where(x => x.CustomersProductsId == customersettings.CustomersProductsId).First();

                if (_customerSettings != null)
                {
                    if (Update(_customerSettings))
                    {
                        return Ok();
                    }
                    else
                    {
                        return NotFound("erro");
                    }
                }
                else
                {
                    msgError = "Erro ao acessar cadastro de clientes";
                    return NotFound(msgError);
                }
            }
            catch (Exception err)
            {
                msgError = err.Message;
                return NotFound(msgError);
            }
        }

        public bool Update(ModelCustomerProduct customerProduct)
        {
            string msgError;
            try
            {
                if (ValidateFile(customerProduct.Customer.RootDirectory))
                {
                    switch (customerProduct.Product.ProductId)
                    {
                        case 1:
                            Copy(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\temp\CIP\Release\BSF\", @"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory, true);
                            MarkVersionFile(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");
                            UpdateBsfFull(customerProduct);
                            break;
                        case 2:
                            Copy(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\temp\CCS\Release\WebService\", @"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory, true);
                            MarkVersionFile(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");
                            break;
                        case 3:
                            Copy(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\temp\CSS\Release\webservice\", @"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory, true);
                            MarkVersionFile(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");
                            break;
                        case 4:
                            Copy(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\temp\ReleaseConcentrador\Release\", @"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory, true);
                            MarkVersionFile(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");
                            break;
                        case 5:
                            if (StopSynchronizerService(customerProduct.SynchronizerServiceName))
                            {
                                System.Threading.Thread.Sleep(9 * 1000);
                                Copy(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\temp\CSS\Release\WindowsService\", @"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory, true);
                                MarkVersionFile(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");
                                StartSynchronizerService(customerProduct.SynchronizerServiceName);
                            }
                            break;
                    }
                    msgError = "";
                    return true;
                }
                else
                    msgError = "Erro ao validar Arquivo Deployment.";
                return false;
            }
            catch (Exception err)
            {
                msgError = err.Message;
                return false;
            }
        }

        private void MarkVersionFile(string pathFileVersion)
        {
            FileInfo fileVersion = new FileInfo(pathFileVersion);
            if (fileVersion.Exists)
            {
                fileVersion.LastWriteTimeUtc = DateTime.Now;
            }
            else
            {
                fileVersion.Create();
            }
        }

        private void StartSynchronizerService(string serviceName)
        {
            try
            {
                if (!String.IsNullOrEmpty(serviceName))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = @"C:\Windows\System32\net.exe";
                        p1.StartInfo.Arguments = $"start " + serviceName;
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.Start();
                    }                    
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private bool StopSynchronizerService(string serviceName)
        {
            try
            {
                if (!String.IsNullOrEmpty(serviceName))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = @"C:\Windows\System32\net.exe";
                        p1.StartInfo.Arguments = $"stop " + serviceName;
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.Start();
                    }
                    return true;
                }
                return false;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        private static void ExtractFile(string customDirectory, int productId)
        {
            try
            {
                if (productId == 4)
                {
                    ZipFile.ExtractToDirectory(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\ReleaseConcentrador.zip", @"c:\Celta Business Solutions\" + customDirectory + @"\temp\ReleaseConcentrador", true);
                }
                else
                {
                    FileInfo file = new FileInfo(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\CCS\Release\WebService\CeltaLimitService.asmx");
                    if (!file.Exists)
                    {
                        ZipFile.ExtractToDirectory(@"c:\Celta Business Solutions\" + customDirectory + @"\temp\deployment.zip", @"c:\Celta Business Solutions\" + customDirectory + @"\temp", true);
                    }
                    else
                    {
                        DirectoryInfo dirSource = new DirectoryInfo(@"c:\Celta Business Solutions\" + customDirectory + @"\temp");
                        DirectoryInfo[] dirs = dirSource.GetDirectories();
                        foreach (DirectoryInfo subdir in dirs)
                        {
                            RecursiveDelete(subdir);
                        }
                        ExtractFile(customDirectory, productId);
                    }
                }
            }
            catch(Exception err)
            {
                throw err;
            }            
        }

        public static void RecursiveDelete(DirectoryInfo baseDir)
        {
            if (!baseDir.Exists)
                return;

            foreach (var dir in baseDir.EnumerateDirectories())
            {
                RecursiveDelete(dir);
            }
            baseDir.Delete(true);
        }

        private static void DownloadFile(string saveDir, int productId)
        {
            var wc = new System.Net.WebClient();
            if (ValidateDir(saveDir))
            {
                if (productId == 4)
                {
                    wc.DownloadFile("http://services.celtaware.com.br/downloads/lastversion/ReleaseConcentrador.zip", @"c:\Celta Business Solutions\" + saveDir + @"\temp\ReleaseConcentrador.zip");
                }
                else
                {
                    wc.DownloadFile("http://services.celtaware.com.br/downloads/lastversion/deployment.zip", @"c:\Celta Business Solutions\" + saveDir + @"\temp\deployment.zip");
                    //wc.DownloadFile("http://localhost:9092/deployment.zip", @"c:\Celta Business Solutions\" + saveDir + @"\temp\deployment.zip");      
                }
                ExtractFile(saveDir, productId);
            }
        }

        public static bool ValidateDir(string directory)
        {
            string currentDir = @"c:\Celta Business Solutions\" + directory + @"\temp";
            DirectoryInfo di = new DirectoryInfo(currentDir);
            if (di.Exists)
            {
                return true;
            }
            return false;
        }

        private static bool ValidateFile(string directory)
        {
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
                string filename = file.Name.ToString().ToLower();
                if (!filename.Contains("web.config"))
                {
                    string temppath = Path.Combine(rootDestination, file.Name);
                    file.CopyTo(temppath, isOverwrite);
                }
            }

            DirectoryInfo[] dirs = dirSource.GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                string subdirName = subdir.Name;

                Directory.CreateDirectory(Path.Combine(rootDestination, subdirName));
                Copy(subdir.FullName, Path.Combine(rootDestination, subdirName), true);

            }

        }

        public static void GenerateConfig(string directory)
        {
            try
            {
                if (!String.IsNullOrEmpty(directory))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\CeltaWare.CBS.CAT.WellknownServiceType.exe";
                        string argFull = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\config.txt";
                        p1.StartInfo.Arguments = "\"" + argFull + " \"";
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.Start();
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static void InsertConfigInSection(string directory)
        {
            var configPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\config.txt";
            var xmlPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\web.config";

            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            StringBuilder strb = new StringBuilder();

            using (var fluxoDeArquivo = new FileStream(configPath, FileMode.Open))
            using (var leitor = new StreamReader(fluxoDeArquivo))
            {
                while (!leitor.EndOfStream)
                {
                    var linha = leitor.ReadLine();
                    strb.AppendLine(linha);
                }
            }

            doc.SelectSingleNode("//service").InnerXml = strb.ToString();
            doc.Save(xmlPath);
        }
        public static void DeleteConfigSection(string directory)
        {
            string xmlPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\web.config";
            XElement xml = XElement.Load(xmlPath);
            xml.Element("system.runtime.remoting").Element("application").Element("service").Elements().ToList().Remove();
            xml.Save(xmlPath);
        }

        public static void ExecuteCatUpdateVersionPackage(string directory)
        {
            try
            {
                if (!String.IsNullOrEmpty(directory))
                {
                    using (Process p1 = new Process())
                    {
                        p1.StartInfo.FileName = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\CeltaWare.CBS.CAT.UpdatedVersionPackage.exe";
                        //string argFull = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\config.txt";                        
                        //p1.StartInfo.Arguments = "\"" + argFull + " \"";             
                        p1.StartInfo.WorkingDirectory = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\";
                        p1.StartInfo.CreateNoWindow = true;
                        p1.StartInfo.UseShellExecute = false;
                        p1.StartInfo.RedirectStandardOutput = true;
                        p1.StartInfo.RedirectStandardError = true;
                        p1.Start();
                    }
                }
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static void UpdateBsfFull(ModelCustomerProduct customerProduct)
        {
            GenerateConfig(customerProduct.Customer.RootDirectory);
            DeleteConfigSection(customerProduct.Customer.RootDirectory);
            InsertConfigInSection(customerProduct.Customer.RootDirectory);
            ExecuteCatUpdateVersionPackage(customerProduct.Customer.RootDirectory);
        }

        private DateTime VersionFile(ModelCustomerProduct customerProduct)
        {
            try
            {
                FileInfo file = new FileInfo(@"c:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory + @"\version.txt");

                if (file.Exists)
                    return file.LastWriteTime;
                else
                    return DateTime.MinValue;

            }
            catch (Exception erro)
            {
                return DateTime.MinValue;
            }
        }
    }
}