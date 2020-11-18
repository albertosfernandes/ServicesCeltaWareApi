using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.TaskMonitor
{
    public class HelperTaskMonitor
    {
        
        public static ModelTaskMonitorSettings LoadSetting(string ServiceName, IConfiguration configuration)
        {
            ModelTaskMonitorSettings settings = new ModelTaskMonitorSettings();

            settings.IsActive = Convert.ToBoolean(configuration.GetSection("Services").GetSection(ServiceName).GetSection("IsActive").Value);
            settings.Name = configuration.GetSection("Services").GetSection(ServiceName).GetSection("Name").Value;
            settings.Url = configuration.GetSection("Services").GetSection(ServiceName).GetSection("Url").Value;
            settings.UpdateInterval = Convert.ToInt32(configuration.GetSection("Services").GetSection(ServiceName).GetSection("UpdateInterval").Value);
            settings.IsDebug = Convert.ToBoolean(configuration.GetSection("Services").GetSection("IsDebug").Value);
            settings.UidTelegramToken = configuration.GetSection("Services").GetSection("UidTelegramToken").Value;
            settings.UidTelegramDestino = configuration.GetSection("Services").GetSection("UidTelegramDestino").Value;

            return settings;
        }

        public static List<ModelTaskMonitorSettings> LoadSettings(IConfiguration configuration)
        {
            List<ModelTaskMonitorSettings> listOfSettings = new List<ModelTaskMonitorSettings>();

            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("SqlDatabase").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskMonitor.LoadSetting("SqlDatabase", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("MysqlDatabase").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskMonitor.LoadSetting("MysqlDatabase", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("CeltaBSSynch").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskMonitor.LoadSetting("CeltaBSSynch", configuration));
            }
            if (Convert.ToBoolean(configuration.GetSection("Services").GetSection("OpenVpn").GetSection("IsActive").Value))
            {
                listOfSettings.Add(HelperTaskMonitor.LoadSetting("OpenVpn", configuration));
            }

            return listOfSettings;
        }
    }
}
