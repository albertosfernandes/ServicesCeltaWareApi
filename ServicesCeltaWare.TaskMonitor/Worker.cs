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

        private Helpers.HelperSqlDatabase _helperSqlDatabase = new Helpers.HelperSqlDatabase();
        private Helpers.HelperGoogleDrive _helperGoogleDrive = new Helpers.HelperGoogleDrive();

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
                    WriteLog("Servi�o iniciado.");
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
                                for (int i = 0; i < schedules.Count; i++)
                                {
                                    int isExecute = schedules[i].DateHourExecution.Minute.CompareTo(DateTime.Now.Minute);
                                    int lastExecution = schedules[i].DateHourLastExecution.Date.CompareTo(DateTime.Now.Date);
                                    if ((isExecute == 0 && lastExecution == -1) || (isExecute == -1 && lastExecution == -1) && !schedules[i].BackupStatus.Equals(Model.Enum.BackupStatus.Runing))
                                    {
                                        // est� dentro do horario ent�o executa!!!
                                        var isExecuted = await _helperSqlDatabase.ExecuteDatabaseSchedule(schedules[i], setting);
                                        if (isExecuted)
                                        {
                                            //backup garantido.. ent�o inicie o upload
                                            var googleDriveFileId = await _helperGoogleDrive.UploadBackup(schedules[i], setting);
                                            if(googleDriveFileId.Contains("ERRO:"))
                                            {
                                                var resp = await _helperSqlDatabase.UpdateStatusBackup(schedules[i], Model.Enum.BackupStatus.OutOfDate, true);
                                                _utilTelegram.SendMessage($"Falha no Upload do Backup: {googleDriveFileId}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                                                new Exception(googleDriveFileId);
                                            }
                                            else
                                            {
                                                schedules[i].GoogleDriveFileId = googleDriveFileId;
                                                var resp = await _helperSqlDatabase.UpdateStatusBackup(schedules[i], Model.Enum.BackupStatus.Success, true);
                                                if (!resp)
                                                {
                                                    _utilTelegram.SendMessage($"Falha no servi�o TaskManager: Erro ao atualizar status do backup, GoogleFileId e Status", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                                                }                                                
                                            }
                                            
                                        }

                                    }
                                }
                                // schedules.Remove(schedules[i]);
                                schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour);
                            }
                            await Task.Delay((setting.UpdateInterval * 60) * 1000, stoppingToken);                            
                        }
                    }

                }
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha no servi�o TaskManager: {err.Message}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                WriteLog(err.Message);
            }           
        }

       

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            WriteLog("Servi�o parado.");
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
