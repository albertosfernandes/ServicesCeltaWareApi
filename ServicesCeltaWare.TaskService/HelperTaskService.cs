using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.TaskService
{
    public class HelperTaskService
    {
        public static ModelTaskServiceSettings LoadSetting(string ServiceName, IConfiguration configuration)
        {
            ModelTaskServiceSettings settings = new ModelTaskServiceSettings();

            settings.IsActive = Convert.ToBoolean(configuration.GetSection("Services").GetSection(ServiceName).GetSection("IsActive").Value);
            settings.Name = configuration.GetSection("Services").GetSection(ServiceName).GetSection("Name").Value;
            settings.Url = configuration.GetSection("Services").GetSection(ServiceName).GetSection("Url").Value;
            settings.UpdateInterval = Convert.ToInt32(configuration.GetSection("Services").GetSection(ServiceName).GetSection("UpdateInterval").Value);
            settings.IsDebug = Convert.ToBoolean(configuration.GetSection("Services").GetSection("IsDebug").Value);
            settings.UidTelegramToken = configuration.GetSection("Services").GetSection("UidTelegramToken").Value;
            settings.UidTelegramDestino = configuration.GetSection("Services").GetSection("UidTelegramDestino").Value;

            return settings;
        }

        public static List<ModelTaskServiceSettings> LoadSettings(IConfiguration configuration)
        {
            List<ModelTaskServiceSettings> listOfSettings = new List<ModelTaskServiceSettings>();

            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("SqlDatabase").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskService.LoadSetting("SqlDatabase", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("MysqlDatabase").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskService.LoadSetting("MysqlDatabase", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("UploadGoogleDrive").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskService.LoadSetting("UploadGoogleDrive", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("CeltaBSSynch").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskService.LoadSetting("CeltaBSSynch", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("OpenVpn").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskService.LoadSetting("OpenVpn", configuration));
            }

            return listOfSettings;
        }
    }
}
