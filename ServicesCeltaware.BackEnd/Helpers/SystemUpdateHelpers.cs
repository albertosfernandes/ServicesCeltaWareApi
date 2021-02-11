using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class SystemUpdateHelpers
    {
        public async static Task<string> UpdateBsfFull(ModelCustomerProduct customerProduct)
        {
            try
            {
                string message = await GenerateConfig(customerProduct.Customer.RootDirectory);
                message += await DeleteConfigSection(customerProduct.Customer.RootDirectory);
                message += await InsertConfigInSection(customerProduct.Customer.RootDirectory);
                message += await ExecuteCatUpdateVersionPackage(customerProduct.Customer.RootDirectory);
                message += await GenerateClient(customerProduct);
                return message;
            }
            catch(Exception err)
            {
                return err.Message;
            }            
        }

        private async static Task<String> GenerateConfig(string directory)
        {
            try
            {                
                string path = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\";
                string command = "CeltaWare.CBS.CAT.WellknownServiceType.exe ";
                string argFull = " config.txt"; // @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\config.txt";

                return await ServicesCeltaWare.Tools.CommandWin32.Execute(path, command, argFull);                              
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private async static Task<string> ExecuteCatUpdateVersionPackage(string directory)
        {
            try
            {                
                string path = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\";
                string command = "CeltaWare.CBS.CAT.UpdatedVersionPackage.exe ";
                string argFull = " ";

                return await ServicesCeltaWare.Tools.CommandWin32.Execute(path, command, argFull);
                
            }
            catch (Exception err)
            {
                return err.Message;
                throw err;
            }
        }

        private async static Task<string> DeleteConfigSection(string directory)
        {
            try
            {
                string xmlPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\web.config";
                XElement xml = XElement.Load(xmlPath);
                xml.Element("system.runtime.remoting").Element("application").Element("service").Elements().ToList().Remove();
                xml.Save(xmlPath);
                return "Seção webconfig deletado com sucesso";
            }
            catch(Exception err) 
            {
                return err.Message;
            }

        }

        private async static Task<string> InsertConfigInSection(string directory)
        {
            try
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
                return "Seção do webconfig gravada com sucesso";
            }
            catch(Exception err)
            {
                return err.Message;
            }            
        }

        private async static Task<bool> GenerateClient(ModelCustomerProduct customerProduct)
        {
            //copy celtapublic to UpdatedVersion
            string source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaPublic.csk";
            string destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\UpdatedVersion\\CeltaPublic.csk";
            ServicesCeltaWare.Tools.CommandWin32.Copy(source, destination, true);
            //copy updatedVersion to Client
            source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\UpdatedVersion";
            destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\Client";
            ServicesCeltaWare.Tools.CommandWin32.Copy(source, destination, true, false);
            //CompactFile
            source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\Client";
            destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\ClientFiles\\celtabsclient.zip";
            await ServicesCeltaWare.Tools.CommandWin32.Compress(source, destination);
            ////Copy celtabsclient.zip to clientfiles 
            //source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\Client\\celtabsclient.zip";
            //destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\ClientFiles\\celtabsclient.zip";
            //CommandWin32.Copy(source, destination, true);

            return true;
        }

        public static void MarkVersionFile(string pathFileVersion)
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
    }
}
