using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServicesCeltaWare.Model;

namespace ServicesCeltaWare.TaskMonitor
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private ModelTaskMonitorSettings settings = new ModelTaskMonitorSettings();
        private List<ModelTaskMonitorSettings> listOfSettings = new List<ModelTaskMonitorSettings>();
        private List<ModelBackupSchedule> backupSchedules = new List<ModelBackupSchedule>();

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            UtilitariosInfra.UtilTelegram _utilTelegram = new UtilitariosInfra.UtilTelegram(_configuration.GetSection("Services").GetSection("UidTelegramToken").Value);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    WriteLog("Serviço iniciado.");
                    // obter os tipos de monitoramento!
                    listOfSettings = HelperTaskMonitor.LoadSettings(_configuration);

                    foreach (var setting in listOfSettings)
                    {

                        if (setting.IsActive && setting.Name.Contains("SqlDatabase"))
                        {
                            //executa este setting de backup SQL
                            Adapters.AdapterSqlDatabase sqlDatabase = new Adapters.AdapterSqlDatabase(setting);
                           
                            var schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour);

                            while (schedules.Count > 0)
                            {
                                for (int i = 0; i <= schedules.Count; i++)
                                {
                                    int isExecute = schedules[i].DateHourExecution.Minute.CompareTo(DateTime.Now.Minute);
                                    int lastExecution = schedules[i].DateHourLastExecution.Date.CompareTo(DateTime.Now.Date);
                                    if ((isExecute == 0 && lastExecution == -1) || (isExecute == -1 && lastExecution == -1) && !schedules[i].BackupStatus.Equals(Model.Enum.BackupStatus.Runing))
                                    {
                                        // está dentro do horario então executa!!!
                                        var isExecuted = await Helpers.HelperSqlDatabase.ExecuteDatabaseSchedule(schedules[i], setting);
                                        if (isExecuted)
                                        {
                                            //backup garantido.. então inicie o upload
                                            var respUpload = await Helpers.HelperGoogleDrive.UploadBackup(schedules[i], setting);
                                            var resp = await Helpers.HelperSqlDatabase.UpdateStatusBackup(schedules[i], Model.Enum.BackupStatus.Success, true);
                                        }

                                        schedules.Remove(schedules[i]);
                                    }
                                }
                            }
                            await Task.Delay((setting.UpdateInterval * 60) * 1000, stoppingToken);                            
                        }
                    }

                }
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha no serviço TaskManager: {err.Message}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                WriteLog(err.Message);
            }           
        }

       

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            WriteLog("Serviço parado.");
            await base.StopAsync(stoppingToken);
        }

        private async static void WriteLog(string _msg)
        {
            StreamWriter sw = null;
            sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFileCeltaTaskMonitor.txt", true);
            sw.WriteLine(DateTime.Now.ToString() + ": Celta Task Monitor - " + _msg);
            sw.Flush();
            sw.Close();
        }
    }
}
