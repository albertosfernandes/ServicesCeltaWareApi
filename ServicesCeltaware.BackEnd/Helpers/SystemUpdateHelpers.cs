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
        public static void UpdateBsfFull(ModelCustomerProduct customerProduct)
        {
            GenerateConfig(customerProduct.Customer.RootDirectory);
            DeleteConfigSection(customerProduct.Customer.RootDirectory);
            InsertConfigInSection(customerProduct.Customer.RootDirectory);
            ExecuteCatUpdateVersionPackage(customerProduct.Customer.RootDirectory);
            GenerateClient(customerProduct);
        }

        private static void GenerateConfig(string directory)
        {
            try
            {
                string _error = null;
                string path = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\";
                string command = "CeltaWare.CBS.CAT.WellknownServiceType.exe ";
                string argFull = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\config.txt";

                CommandWin32.ExecuteTeste(path, command, argFull, out _error);                              
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static void ExecuteCatUpdateVersionPackage(string directory)
        {
            try
            {
                string _error = null;
                string path = @"C:\Celta Business Solutions\" + directory + @"\BSF\Bin\";
                string command = "CeltaWare.CBS.CAT.UpdatedVersionPackage.exe ";
                string argFull = " ";

                CommandWin32.ExecuteTeste(path, command, argFull, out _error);
                
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        private static void DeleteConfigSection(string directory)
        {
            string xmlPath = @"C:\Celta Business Solutions\" + directory + @"\BSF\web.config";
            XElement xml = XElement.Load(xmlPath);
            xml.Element("system.runtime.remoting").Element("application").Element("service").Elements().ToList().Remove();
            xml.Save(xmlPath);
        }

        private static void InsertConfigInSection(string directory)
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

        private static bool GenerateClient(ModelCustomerProduct customerProduct)
        {
            //copy celtapublic to UpdatedVersion
            string source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaPublic.csk";
            string destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\UpdatedVersion\\CeltaPublic.csk";
            CommandWin32.Copy(source, destination, true);
            //copy updatedVersion to Client
            source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\UpdatedVersion";
            destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\Client";
            CommandWin32.Copy(source, destination, true, false);
            //CompactFile
            source = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\Client";
            destination = $"c:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\ClientFiles\\celtabsclient.zip";
            CommandWin32.Compress(source, destination);
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
