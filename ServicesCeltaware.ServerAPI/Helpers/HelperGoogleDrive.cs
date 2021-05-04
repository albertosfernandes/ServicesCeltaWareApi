using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicesCeltaWare.Model;
using ServicesCeltaWare.Tools;
using ServicesCeltaWare.UtilitariosInfra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ServicesCeltaware.ServerAPI.Helpers
{
    public class HelperGoogleDrive
    {
        public static ModelGoogleDrive LoadSetting(IConfiguration configuration)
        {
            ModelGoogleDrive settings = new ModelGoogleDrive();

            
            settings.GoogleDriveAccountMail = configuration.GetSection("Settings").GetSection("GoogleDriveAccountMail").Value;
            settings.FolderId = configuration.GetSection("Settings").GetSection("FolderId").Value;
            settings.CredentialFileName = configuration.GetSection("Settings").GetSection("CredentialFileName").Value;
            settings.GoogleAccountId = configuration.GetSection("Settings").GetSection("GoogleAccountId").Value;
            settings.ApiKey = configuration.GetSection("Settings").GetSection("ApiKey").Value;

            return settings;
        }

        public async static Task<string> UploadFromLinux(string credentialFileName, string backupFileName, string path, string folderId)
        {
            try
            {              
                string script = $"/servicesCeltaWare/consolegoogle/ServicesCeltaWare.UtilitariosInfra.ConsoleGoogleApi 1 {credentialFileName} \"{backupFileName}\" {path} {folderId}";

                string msg = await CommandBash.Execute(script);
                return msg;
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }

        public async static Task<string> ValidFolderUpload(string credentialFileName, string folderName)
        {
            try
            {

                string script = $"/servicesCeltaWare/consolegoogle/ServicesCeltaWare.UtilitariosInfra.ConsoleGoogleApi 2 {credentialFileName} {"backupFileName"} {"path"} {folderName}";
                string msg = await CommandBash.Execute(script);
                if(msg.Contains("0"))
                {
                    script = $"/servicesCeltaWare/consolegoogle/ServicesCeltaWare.UtilitariosInfra.ConsoleGoogleApi 3 {credentialFileName} {"backupFileName"} {"path"} {folderName}";
                    msg = await CommandBash.Execute(script);
                }

                return msg;
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }


        public async static Task<string> UpdateCredential(string credentialFileName)
        {
            try
            {
                string script = $"/servicesCeltaWare/consolegoogle/ServicesCeltaWare.UtilitariosInfra.ConsoleGoogleApi 5 {credentialFileName} \"destinationName\" \"url\" \"folderId\" ";
                string msg = await CommandBash.Execute(script);
                if (!msg.ToUpperInvariant().Contains("OK"))
                {
                    return msg;
                }
                return msg;
                //string url = "http://192.168.100.115:8080/file/TokenResponse-user.TokenResponse-user";
                //string destino = "/servicesCeltaWare/consolegoogle/credential/Google.Apis.Auth.OAuth2.Responses.TokenResponse-user";
                //Uri siteUri = new Uri(url);
                //WebClient client = new WebClient();
                //await client.DownloadFileTaskAsync(siteUri, destino);
                //return "ok";
            }
            catch (Exception err)
            {
                if (err.InnerException != null)
                {
                    HelperLogs.WriteLog("HelperGoogleDrive UpdateCredential", err.Message + "\n" + err.InnerException.Message);
                    return (err.Message + "\n" + err.InnerException.Message);
                }
                HelperLogs.WriteLog("HelperGoogleDrive UpdateCredential", err.Message + "\n" + err.InnerException.Message);
                return (err.Message);
            }
        }

    }
}
