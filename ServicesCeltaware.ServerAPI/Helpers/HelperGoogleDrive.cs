using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServicesCeltaWare.Tools;
using ServicesCeltaWare.UtilitariosInfra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
                string script = $"/servicesCeltaWare/consolegoogle/ServicesCeltaWare.UtilitariosInfra.ConsoleGoogleApi 1 {credentialFileName} {backupFileName} {path} {folderId}";

                string msg = await CommandBash.Execute(script);
                return msg;
            }
            catch (Exception err)
            {
                return err.Message;
            }

        }

   
    }
}
