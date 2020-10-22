using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class DatabaseHelpers
    {
        public async static Task<bool> Create(ModelCustomerProduct customerProduct)
        {
            try
            {
                // 1- Executar GenerateKeys.exe
                string _error = null;
                string path = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\";
                string command = "CeltaWare.CBS.CAT.GenerateKeys.exe";
                string args = " ";
                await CommandWin32.ExecuteTeste(path, command, args);

                // 2- Copiar as chaves para diretórios ccs e windowsService e updateVersion
                string file = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaPublic.csk";
                string destination = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\CCS\\bin\\CeltaPublic.csk";
                CommandWin32.Copy(file, destination, true); //CeltaPrivate.csk
                file = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaPrivate.csk";
                destination = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\CCS\\bin\\CeltaPrivate.csk";
                CommandWin32.Copy(file, destination, true); //CeltaPublic.csk

                file = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaPublic.csk";
                destination = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\CSS\\WindowsService\\CeltaPublic.csk";
                CommandWin32.Copy(file, destination, true); //CeltaPublic.csk

                // 3 - Executar ResetPassword.exe <DBSERVERNAME>  <DATABASENAME>  <DBUSERNAME>  <DBUSERPASSWORD>  <PRIVATEKEY>  <PUBLICKEY>
                command = "CeltaWare.CBS.CAT.ResetPasswords.exe ";
                args = $" {customerProduct.IpAddress},{customerProduct.Port} {customerProduct.SynchronizerServiceName} {customerProduct.LoginUser} {customerProduct.LoginPassword} CeltaPrivate.csk CeltaPublic.csk";
                await CommandWin32.ExecuteTeste(path, command, args);

                // 4 - Executar ChangeAdminpassword.exe - <DBSERVERNAME>  <DATABASENAME>  <DBUSERNAME>  <DBUSERPASSWORD>  <ADMINNEWPASSWORD>  <PUBLICKEY>
                command = "CeltaWare.CBS.CAT.ChangeAdminPassword.exe ";
                args = $" {customerProduct.IpAddress},{customerProduct.Port} {customerProduct.SynchronizerServiceName} {customerProduct.LoginUser} {customerProduct.LoginPassword} CeltaBusinessSolutions+=123 CeltaPublic.csk ";
                await CommandWin32.ExecuteTeste(path, command, args);

                // 5 - Executar GenerateEventLogCategorys.exe
                command = "CeltaWare.CBS.CAT.GenerateEventLogCategorys.exe";
                args = " ";
                await CommandWin32.ExecuteTeste(path, command, args);
                                
                return true;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public async static void GenerateConnectionString(ModelCustomerProduct customerProduct, string celtaBSUserPassword)
        {            
            string path = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\";
            string command = "CeltaWare.CBS.CAT.GenerateConnectionString.exe ";
            string args = $" server={customerProduct.IpAddress},{customerProduct.Port};database={customerProduct.SynchronizerServiceName};uid=CeltaBSUser;pwd={celtaBSUserPassword} server=192.168.1.6,9980;database=CeltaBSCep;uid=sa;pwd=Celta@123 server={customerProduct.IpAddress},{customerProduct.Port};database={customerProduct.SynchronizerServiceName};uid={customerProduct.LoginUser}pwd={customerProduct.LoginPassword}";            

            await CommandWin32.ExecuteTeste(path, command, args);

            // 2 - Copiar CeltaWare.CBS.Common.dll.config para diretórios ccs e windowsService*/
            string file = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\BSF\\Bin\\CeltaWare.CBS.Common.dll.config";
            string destination = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\CCS\\bin\\CeltaWare.CBS.Common.dll.config";
            CommandWin32.Copy(file, destination, true);
            destination = $"C:\\Celta Business Solutions\\{customerProduct.Customer.RootDirectory}\\CSS\\WindowsService\\CeltaWare.CBS.Common.dll.config";
            CommandWin32.Copy(file, destination, true);
        }
    }
}
