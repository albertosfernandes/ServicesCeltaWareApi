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

namespace ServicesCeltaWare.TaskService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private ModelTaskServiceSettings settings = new ModelTaskServiceSettings();
        private List<ModelTaskServiceSettings> listOfSettings = new List<ModelTaskServiceSettings>();
        private List<ModelBackupSchedule> backupSchedules = new List<ModelBackupSchedule>();

        private static Helpers.HelperSqlDatabase _helperSqlDatabase = new Helpers.HelperSqlDatabase();


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
               await WriteLog("Serviço iniciado.");
                while (!stoppingToken.IsCancellationRequested)
                {
                    // _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    // carregar as configurações
                    listOfSettings = HelperTaskService.LoadSettings(_configuration);
                    bool[] arrayActive = new bool[5];

                    foreach (var setting in listOfSettings)
                    {
                        #region BackupRun
                        if (setting.IsActive && setting.Name.Contains("SqlDatabase"))
                        {
                            arrayActive[0] = true;
                        }
                        #endregion

                        #region BackupUpload
                        if (setting.IsActive && setting.Name.Contains("UploadGoogleDrive"))
                        {
                            arrayActive[4] = true;
                        }
                        #endregion
                    }

                    Task t0 = null;
                    Task t4 = null;

                    if (arrayActive[0])
                    {
                        t0 = Task.Run(() =>
                        {
                            ExecuteSqlDatabase(listOfSettings.Find(s => s.Name.Contains("SqlDatabase")));
                        });
                    }
                        
                    // = ExecuteSqlDatabase(listOfSettings.Find(s => s.Name.Contains("SqlDatabase")));

                    if (arrayActive[4])
                    {
                        t4 = Task.Run( () => 
                        {
                            ExecuteUploadGoogleDrive(listOfSettings.Find(s => s.Name.Contains("UploadGoogleDrive")));
                        });
                    }
                        
                    if(arrayActive[0] && arrayActive[4])
                    {
                        while (!t0.IsCompleted || !t4.IsCompleted)
                        {

                        }
                    }

                    while(!t0.IsCompleted)
                    {

                    }

                    await Task.Delay((2 * 60) * 1000);
                }
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha no serviço TaskManager: {err.Message}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                await WriteLog(err.Message);
            }            
        }


        private static async Task<string> ExecuteSqlDatabase(ModelTaskServiceSettings setting)
        {
            try
            {
                Adapters.AdapterSqlDatabase sqlDatabase = new Adapters.AdapterSqlDatabase(setting);
                var schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour, false);

                //foreach (var bkpSchedule in schedules)
                //{
                    
                //}

                if (setting.IsDebug)
                {
                    foreach (var sch in schedules)
                    {
                        await WriteLog("Iniciando processo backup de: " + sch.Databases.DatabaseName + " " + sch.DateHourExecution.Hour);
                    }

                }

                for (int i = 0; i < schedules.Count;)
                {
                    int isExecute = schedules[i].DateHourExecution.Minute.CompareTo(DateTime.Now.Minute);
                    int lastExecution = schedules[i].DateHourLastExecution.Date.CompareTo(DateTime.Now.Date);
                    if ((isExecute == 0 && lastExecution == -1) || (isExecute == -1 && lastExecution == -1) && !schedules[i].BackupStatus.Equals(Model.Enum.BackupStatus.Runing))
                    {
                        var isUpdated = await _helperSqlDatabase.UpdateStatusDateTimeBackup(schedules[i], Model.Enum.BackupStatus.Runing, setting);
                        var resp = _helperSqlDatabase.BackupRun(schedules[i], setting);
                        i++;
                    }
                }

                await Task.Delay((setting.UpdateInterval * 60) * 1000);

                return "OK";
            }
            catch(Exception err)
            {
                await WriteLog(err.Message);
                return "OK";
            }
        }

        private static async Task<string> ExecuteUploadGoogleDrive(ModelTaskServiceSettings setting)
        {
            try
            {
                Adapters.AdapterSqlDatabase sqlDatabase = new Adapters.AdapterSqlDatabase(setting);

                var schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour, true);
                for (int i = 0; i < schedules.Count; i++)
                {
                    var resp = _helperSqlDatabase.ExecuteUpload(schedules[i], setting);
                }
                await Task.Delay((setting.UpdateInterval * 60) * 1000);

                return "OK";
            }
            catch(Exception err)
            {
                await WriteLog(err.Message);
                return "OK";
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await WriteLog("Serviço parado.");
            await base.StopAsync(stoppingToken);
        }

        public async static Task<bool> WriteLog(string _msg)
        {
            StreamWriter sw = null;
            sw = new StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "\\LogFileCeltaTaskService.txt", true);
            await sw.WriteLineAsync(DateTime.Now.ToString() + ": Celta Task Service - " + _msg);
            sw.Flush();
            sw.Close();
            return true;
        }
    }
}
