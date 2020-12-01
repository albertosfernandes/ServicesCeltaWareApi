using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
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
            
            return settings;
        }

   
    }
}
