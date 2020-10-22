using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ServicesCeltaware.BackEnd.Enum;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class CustomerProductHelpers
    {
        public async static Task<string> CreateProducts(ProductName productName, ModelCustomerProduct customerProduct)
        {
            try
            {
                string msgCreateSite = null;
                string _error = null;
                string error = null;
                string directory = @"C:\Celta Business Solutions\" + customerProduct.Customer.RootDirectory + @"\" + customerProduct.InstallDirectory;
                DirectoryInfo dir = new DirectoryInfo(directory);
                switch (productName)
                {
                    case ProductName.BSF:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\BSF", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\", "appcmd.exe",  // /physicalPath:" + "\"" + DefaultDir + " \"" +                          
                            $" add app /site.name:{customerProduct.Customer.RootDirectory}-CeltaBS /path:/{customerProduct.InstallDirectory} /physicalPath:"+"\""+dir+"\"");
                            
                            ChangeDefaultHtm(customerProduct.Customer.RootDirectory, customerProduct.Port, Enum.ProductName.BSF);
                            break;
                        }
                    case ProductName.CCS:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\CCS", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                            $" add app /site.name:{customerProduct.Customer.RootDirectory}-CeltaBS /path:/{customerProduct.InstallDirectory} /physicalPath:"+"\""+dir+"\"");
                            
                            ChangeDefaultHtm(customerProduct.Customer.RootDirectory, customerProduct.Port, Enum.ProductName.CCS);
                            break;
                        }
                    case ProductName.CSS:
                        {
                            if (!dir.Exists)
                                CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\CSS\WebService", dir.ToString(), true, true);
                            msgCreateSite = await CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\", "appcmd.exe",
                            $" add app /site.name:{customerProduct.Customer.RootDirectory}-CeltaBS /path:/{ValidateNameForPathSite(customerProduct.InstallDirectory)} /physicalPath:" + "\"" + dir +" \"");
                            break;
                        }
                    case ProductName.Concentrador:
                        {
                            break;
                        }
                    case ProductName.SynchronizerService:
                        {
                            break;
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
                // error = err.Message;
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
            catch(Exception err)
            {
                throw err;
            }
        }

        private static void ChangeDefaultHtm(string directory, string portNumberBSF, Enum.ProductName _productName)
        {
            try
            {
                switch (_productName)
                {
                    case Enum.ProductName.BSF:
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
            catch(Exception err)
            {
                throw err;
            }

            

                //return true;
        }
    }
}
