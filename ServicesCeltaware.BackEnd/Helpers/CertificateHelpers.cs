using Microsoft.Extensions.FileProviders;
using ServicesCeltaware.BackEnd.Tools;
using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServicesCeltaware.BackEnd.Helpers
{
    public class CertificateHelpers
    {
        public static bool CreateFileRepositorie(string _directoryName)
        {
            try
            {   if(!Directory.Exists(_directoryName))             
                    Directory.CreateDirectory(_directoryName);

                return Directory.Exists(_directoryName);                
            }
            catch(Exception err)
            {
                return false;
            }
        }

        public static DateTime GetNotAfter(ModelCertificate _certificate)
        {
            try
            {
                //certutil -dump -p Baz@r25 bazar25.pfx | find "NotAfter"
                //string argument = $" -dump -p {password} \"{fileNameFullPath}\" | findstr \"NotAfter:\"";
                string path = $"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\";
                string fileNameFullPath = path + "\\" + _certificate.FileRepositorie + "\\" + _certificate.FileName;
                string argument = $"getNotAfter.bat {_certificate.Password} \"{fileNameFullPath}\"";
                string date = null;
                string error = null;
                string dateFormated = null;
                if (!File.Exists(fileNameFullPath))
                    return DateTime.MinValue;

                date = CommandWin32.ExecuteBatchWithResponse(path, argument, out error);

                if(!String.IsNullOrEmpty(error))
                return DateTime.MinValue;
                if (date.Contains("decodificar objeto: A senha de rede especificada"))
                    new Exception("Senha incorreta");

                // validar se veio mais de uma linha
                var count = date.Split("NotAfter:");
                for (int i = 0; i < count.Length; i++)
                {
                    if (i == count.Length - 1)
                    {
                        //é o ultimo
                        string valid = count[i];
                        if (valid.Contains("NotAfter:"))
                        {
                            dateFormated = valid.Substring(10, 16);
                        }
                        else
                        {
                            dateFormated = valid.Substring(1, 16);
                        }
                    }
                }

                //dateFormated = date.Substring(date.IndexOf("NotAfter:"), 26);

                DateTime parsedDate = DateTime.Parse(dateFormated);

                return parsedDate;
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        public static string GetHashCert(ModelCertificate _certificate)
        {
            try
            {
                // -dump -p 120663 "villar.pfx" | find "Hash Cert(sha1)"
                string path = $"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\";
                string fileNameFullPath = path + "\\" + _certificate.FileRepositorie + "\\" + _certificate.FileName;
                string argument = $"getHashCert.bat {_certificate.Password} \"{fileNameFullPath}\"";
                string message = null;
                string error = null;
                string messageFormated = null;
                if (!File.Exists(fileNameFullPath))
                    return "Arquivo inexistente";
                message = CommandWin32.ExecuteBatchWithResponse(path, argument, out error);

                if (!String.IsNullOrEmpty(error))
                    return "Arquivo inexistente";

                // validar se veio mais de uma linha
                /*Hash Cert(sha1): 0e182e25504c1c6edfe9598900f5d173055de696
                  Hash Cert(sha1): 4acadab14b74bf4fba7bace64b91801c44b8cc66
                  Hash Cert(sha1): c462286dc14f62447882666dcbbc391ad431910c
                  Hash Cert(sha1): 3b137112f00f1a4de8001fd7bc381b94545e6aa3*/
                var count = message.Split("Hash Cert(sha1):");
                for (int i = 0; i < count.Length; i++)
                {
                    if(i == count.Length-1)
                    {
                        //é o ultimo
                        string valid = count[i];
                        if(valid.Contains("Hash Cert(sha1):"))
                        {
                            messageFormated = valid.Substring(17, 40);
                        }
                        else
                        {
                            messageFormated = valid.Substring(1, 40);
                        }
                    }
                }
                //sempre pegar a ultima e tirar um espaço no inicio que esta vindo tambem tirar \r \n
                //messageFormated = message.Substring(message.IndexOf("Hash Cert(sha1):"), 58);
                return messageFormated;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        public static string InstallCert(ModelCertificate _certificate)
        {
            try
            {
                /*-f -p 1234 -importpfx "c:\Celta Business Solutions\AlbertoTeste\BS\certificadosLoja03\17376877_out.pfx" NoProtect,FriendlyName="Seane Ipelandia""*/
                //string argument = $"installCertificate.bat {_certificate.Password} \"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\{_certificate.FileRepositorie}\\{_certificate.FileName}\" \"{_certificate.FriendlyName}\"";
                //string path = $"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\";                
                string message = null;
                string error = null;
                //int resultCode;

                string argument = $"-f -p {_certificate.Password} -importpfx \"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\BSF\\certificados\\{_certificate.FileRepositorie}\\{_certificate.FileName}\" NoProtect,FriendlyName=\"{_certificate.FriendlyName}\"";
                //resultCode = CommandWin32.ExecuteBatch(path, argument, out error);
                message = CommandWin32.ExecuteSynch(@"c:\windows\system32\", "certutil.exe ", argument, out error);

                if (!message.Contains("-importPFX : comando concluído com êxito."))
                {
                    string e = message; //deu erro
                }

                return error;
            }
            catch(Exception err)
            {
                throw err;
            }
        }

        public static string Remove(ModelCertificate _certificate)
        {
            try
            {
                string argument = $"-delstore My {_certificate.HashCert}";
                string message = null;
                string error = null;
                int resultCode;

                message = CommandWin32.ExecuteSynch(@"c:\windows\system32\", "certutil.exe ", argument, out error);

                if (!message.Contains("comando concluÝdo com Ûxito."))
                {
                    string e = message; //deu erro
                }

                if (!String.IsNullOrEmpty(error))
                    return error;
                //   CommandWin32.Copy(@"c:\Celta Business Solutions\Empty\", @"c:\Celta Business Solutions\" + customer.RootDirectory, true, true);
                // Salvar uma copia de backup para diretório lixeira
                CommandWin32.Copy(@"C:\Celta Business Solutions\" + _certificate.Customer.RootDirectory + @"\BSF\Certificados\" + _certificate.FileRepositorie,
                                  @"C:\Celta Business Solutions\" + _certificate.Customer.RootDirectory + @"\BSF\Certificados\Lixeira",
                                  true,
                                  true);

                // Excluir o arquivo                
                string path = $"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\";
                argument = $"removeCertificateFile.bat \"C:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\BSF\\Certificados\\{_certificate.FileRepositorie}\\{_certificate.FileName}\"";
                resultCode = CommandWin32.ExecuteBatch(path, argument, out error);
                return message;
            }
            catch (Exception err)
            {
                throw err;
            }
        }

        // certutil.exe -delstore My 552c782ae9b10ad4d976f555fc7be7c8be1998dd

        public static string ChangePermissions(ModelCertificate _certificate)
        {
            try
            {             
                string argument = $"changePermissionCert.bat \"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\AddUsertToCertificate.ps1\" {_certificate.HashCert}";
                string path = $"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\";
                int resultCode;
                string error = null;

                resultCode = CommandWin32.ExecuteBatch(path, argument, out error);

                if (!String.IsNullOrEmpty(error))
                    return error;
                argument = $"changePermissionOwner.bat \"c:\\Celta Business Solutions\\{_certificate.Customer.RootDirectory}\\bsf\\certificados\\changePermissionOwner.ps1\" {_certificate.HashCert}";
                resultCode = CommandWin32.ExecuteBatch(path, argument, out error);

                return error;
            }
            catch(Exception err)
            {
                throw err;
            }
        }        

    }
}
