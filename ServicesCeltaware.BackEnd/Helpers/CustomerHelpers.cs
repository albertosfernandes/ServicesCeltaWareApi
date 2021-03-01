using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class CustomerHelpers
    {       
        public async static Task<string> CreateSite(ModelCustomer customer)
        {
            try
            {
                string DefaultDir = @"c:\Celta Business Solutions\" + customer.RootDirectory;                
                DirectoryInfo dirSource = new DirectoryInfo(DefaultDir);
                if (!dirSource.Exists)
                {
                    try
                    {
                        ServicesCeltaWare.Tools.CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                    }
                    catch(Exception)
                    {
                        throw new DirectoryNotFoundException(
                        "Não foi possível criar o diretório: " + DefaultDir);
                    }                    
                }                // 
                string message = await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" add site /name:{customer.RootDirectory + "-CeltaBS"} /physicalPath:" + "\"" + DefaultDir + "\"" + $" /bindings:http/*:{customer.CodeCeltaBs}:");
                return message;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public async static Task<string> CreatePool(ModelCustomer customer)
        {
            try
            {
                string message = await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" add apppool /name:{customer.RootDirectory}-CeltaBS");
                
                message +=  await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" set apppool /apppool.name:\"{customer.RootDirectory}-CeltaBS\" /enable32bitapponwin64:true");

                return message;
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }

        public async static Task<string> ChangePool(ModelCustomer customer, ServicesCeltaWare.Model.Enum.ProductName _productName)
        {
            try
            {
                string message = null;
                switch (_productName)
                {
                    case ServicesCeltaWare.Model.Enum.ProductName.None:
                        {
                            message = await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/'].applicationPool:{customer.RootDirectory}-CeltaBS");
                            break;
                        }
                    case ServicesCeltaWare.Model.Enum.ProductName.BSF:
                        { 
                            message = await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/BSF'].applicationPool:{customer.RootDirectory}-CeltaBS");
                            break;
                        }
                    case ServicesCeltaWare.Model.Enum.ProductName.CCS:
                        {                            
                            message = await ServicesCeltaWare.Tools.CommandWin32.Execute(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/CCS'].applicationPool:{customer.RootDirectory}-CeltaBS");
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

        public static bool contemLetras(string texto)
        {
            if (texto.Where(c => char.IsLetter(c)).Count() > 0)
                return true;
            else
                return false;
        }

        public static bool contemNumeros(string texto)
        {
            if (texto.Where(c => char.IsNumber(c)).Count() > 0)
                return true;
            else
                return false;
        }      
    }
}
