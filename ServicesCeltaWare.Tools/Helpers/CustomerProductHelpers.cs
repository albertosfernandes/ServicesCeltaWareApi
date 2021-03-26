using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServicesCeltaWare.Model.Enum;

namespace ServicesCeltaWare.Tools.Helpers
{
    public class CustomerProductHelpers
    {
        public async static Task<string> CreateProducts(ProductName productName, IApps _apps)
        {
            try
            {
                string msgCreateSite = null;
                string directory = @"C:\Celta Business Solutions\" + _apps.CustomerProduct.Customer.RootDirectory + @"\" + _apps.InstallDirectory;
                DirectoryInfo dir = new DirectoryInfo(directory);
                switch (productName)
                {
                    case ProductName.BSF:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\BSF", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                            $" add app /site.name:{_apps.CustomerProduct.Customer.RootDirectory}-CeltaBS /path:/{_apps.InstallDirectory} /physicalPath:" + "\"" + dir + "\"");

                            ChangeDefaultHtm(_apps.CustomerProduct.Customer.RootDirectory, _apps.Port, ServicesCeltaWare.Model.Enum.ProductName.BSF);
                            break;
                        }
                    case ProductName.CCS:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\CCS", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                            $" add app /site.name:{_apps.CustomerProduct.Customer.RootDirectory}-CeltaBS /path:/{_apps.InstallDirectory} /physicalPath:" + "\"" + dir + "\"");

                            ChangeDefaultHtm(_apps.CustomerProduct.Customer.RootDirectory, _apps.Port, ProductName.CCS);
                            break;
                        }
                    case ProductName.CSS:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\CSS\WebService", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                            $" add app /site.name:{_apps.CustomerProduct.Customer.RootDirectory}-CeltaBS /path:/{ValidateNameForPathSite(_apps.InstallDirectory)} /physicalPath:" + "\"" + dir + " \"");
                            break;
                        }
                    case ProductName.Concentrador:
                        {
                            try
                            {
                                if (!dir.Exists)
                                    CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\Concentrador", dir.ToString(), true, true);
                                msgCreateSite = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                                 $" add site /name:{_apps.CustomerProduct.Customer.RootDirectory}-Concentrador /physicalPath:" + "\"" + @"C:\Celta Business Solutions\" + _apps.CustomerProduct.Customer.RootDirectory + "\"" + $" /bindings:http/*:{_apps.Port}:");

                                string messagePool = await CreatePool(_apps.CustomerProduct.Customer, ProductName.Concentrador);
                                string messageChangePool = await ChangePool(_apps.CustomerProduct.Customer, ProductName.Concentrador);
                                break;
                            }
                            catch(Exception err)
                            {
                                msgCreateSite = err.Message;
                                break;
                            }
                            
                        }
                    case ProductName.SynchronizerService:
                        {
                            try
                            {
                                if (!dir.Exists)
                                    CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\CSS\WindowsService", dir.ToString(), true, true);
                                msgCreateSite = await CommandWin32.Execute(dir.ToString(), @"\InstallUtil.exe ", _apps.CustomerProduct.Customer.RootDirectory);
                                break;
                            }
                            catch(Exception err)
                            {
                                msgCreateSite = err.Message;
                                break;
                            }
                        }
                    case ProductName.Database:
                        {
                            break;
                        }
                    case ProductName.CertificadoA1:
                        {
                            if (!dir.Exists)
                            {
                                dir.Create();
                            }
                            CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\bsf\certificados", dir.ToString(), true, true);
                            break;
                        }
                    default: break;

                }
                return msgCreateSite;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        private static string ValidateNameForPathSite(string name)
        {
            try
            {
                var a = name.IndexOf("'\'");
                string nameFormat = name;

                return nameFormat;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static void ChangeDefaultHtm(string directory, string portNumberBSF, ServicesCeltaWare.Model.Enum.ProductName _productName)
        {
            try
            {
                switch (_productName)
                {
                    case ProductName.BSF:
                        {
                            var defaultHtmPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\default.htm";

                            StringBuilder strb = new StringBuilder();

                            using (var fluxoDeArquivo = new FileStream(defaultHtmPath, FileMode.Open))
                            using (var leitor = new StreamReader(fluxoDeArquivo, Encoding.UTF8))
                            {
                                while (!leitor.EndOfStream)
                                {
                                    var linha = leitor.ReadLine();
                                    if (linha.Contains(("(????)(BSF)</span>")))
                                        linha = linha.Replace("(????)(BSF)</span>", $"({directory})(BSF)</span>");

                                    if (linha.Contains("<a href=\"http://cloud.celtaware.com.br:????/bsf/clientfiles/celtabsclient.zip\">Clique aqui para baixar o cliente.</a>"))
                                        linha = linha.Replace("<a href=\"http://cloud.celtaware.com.br:????/bsf/clientfiles/celtabsclient.zip\">Clique aqui para baixar o cliente.</a>", $"<a href=\"http://cloud.celtaware.com.br:{portNumberBSF}/bsf/clientfiles/celtabsclient.zip\">Clique aqui para baixar o cliente.</a>");
                                    strb.AppendLine(linha);
                                }
                            }

                            FileStream defaultHtmFile = new FileStream(@"C:\Celta Business Solutions\" + directory + @"\BSF\default.htm", FileMode.Create);

                            using (StreamWriter outputFile = new StreamWriter(defaultHtmFile, Encoding.UTF8))
                            {
                                outputFile.Write(strb.ToString());
                            }
                            break;
                        }
                    case ProductName.CCS:
                        {
                            var defaultHtmPath = @"C:\Celta Business Solutions\" + directory + @"\CCS\CCSDefault.htm";

                            StringBuilder strb = new StringBuilder();

                            using (var fluxoDeArquivo = new FileStream(defaultHtmPath, FileMode.Open))
                            using (var leitor = new StreamReader(fluxoDeArquivo))
                            {
                                while (!leitor.EndOfStream)
                                {
                                    var linha = leitor.ReadLine();
                                    if (linha.Contains("<span style=\"font - size: 14pt\">Celta Business Solutions"))
                                        linha.Replace("<span style=\"font - size: 14pt\">Celta Business Solutions™ (Empty)(CCS)</span>", $"<span style =\"font - size: 14pt\">Celta Business Solutions™ ({directory})(BSF)</span>");

                                    strb.AppendLine(linha);
                                }
                            }

                            FileStream defaultHtmFile = new FileStream(@"C:\Celta Business Solutions\" + directory + @"\BSF\default.htm", FileMode.Create);

                            using (StreamWriter outputFile = new StreamWriter(defaultHtmFile, Encoding.UTF8))
                            {
                                outputFile.Write(strb.ToString());
                            }
                            break;
                        }
                }
            }
            catch (Exception err)
            {
                throw err;
            }

        }

        private async static Task<string> CreatePool(ModelCustomer customer, ProductName _productName)
        {
            try
            {
                string message = null;
                switch (_productName)
                {
                    case ProductName.Concentrador:
                        {
                            message = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" add apppool /name:{customer.RootDirectory}-Concentrador");
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
                

                return message;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        private async static Task<string> ChangePool(ModelCustomer customer, ProductName _productName)
        {
            try
            {
                string message = null;
                switch (_productName)
                {
                    case ProductName.None:
                        {
                            break;
                        }
                    case ProductName.BSF:
                        {                            
                            break;
                        }
                    case ProductName.CCS:
                        {                         
                            break;
                        }
                    case ProductName.Concentrador:
                        {
                            message = await CommandWin32.Execute(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/CCS'].applicationPool:{customer.RootDirectory}-Concentrador");
                            break;
                        }
                }
                return message;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

    }
}
