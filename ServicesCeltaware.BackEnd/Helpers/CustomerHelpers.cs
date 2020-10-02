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
        public static string CreateSite(ModelCustomer customer)
        {
            try
            {
                string DefaultDir = @"c:\Celta Business Solutions\" + customer.RootDirectory;
                string _error = null;
                DirectoryInfo dirSource = new DirectoryInfo(DefaultDir);
                if (!dirSource.Exists)
                {
                    try
                    {
                        CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                    }
                    catch(Exception err)
                    {
                        throw new DirectoryNotFoundException(
                        "Não foi possível criar o diretório: " + DefaultDir);
                    }                    
                }                // 
                string message = CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" add site /name:{customer.RootDirectory + "-CeltaBS"} /physicalPath:" + "\"" + DefaultDir + "\"" + $" /bindings:http/*:{customer.CodeCeltaBs}:",
                            out _error);
                return message;
            }
            catch (Exception err)
            {
                return err.Message;
            }
        }

        public static string CreatePool(ModelCustomer customer)
        {
            try
            {
                string _error = null;
                string message = CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" add apppool /name:{customer.RootDirectory}-CeltaBS", out _error);
                
                message += CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                "appcmd.exe",
                            $" set apppool /apppool.name:\"{customer.RootDirectory}-CeltaBS\" /enable32bitapponwin64:true", out _error);

                return message;
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }

        public static string ChangePool(ModelCustomer customer, Enum.ProductName _productName)
        {
            try
            {
                string message = null;
                string _error = null;
                switch (_productName)
                {
                    case Enum.ProductName.None:
                        {
                            message = CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/'].applicationPool:{customer.RootDirectory}-CeltaBS", out _error);
                            break;
                        }
                    case Enum.ProductName.BSF:
                        { 
                            message = CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/BSF'].applicationPool:{customer.RootDirectory}-CeltaBS", out _error);
                            break;
                        }
                    case Enum.ProductName.CCS:
                        {                            
                            message = CommandWin32.ExecuteTeste(@"C:\Windows\System32\inetsrv\",
                                                            "appcmd.exe",
                                        $" set site /site.name:{customer.RootDirectory}-CeltaBS /[path='/CCS'].applicationPool:{customer.RootDirectory}-CeltaBS", out _error);
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
