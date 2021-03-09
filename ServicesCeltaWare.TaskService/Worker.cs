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

        private Helpers.HelperSqlDatabase _helperSqlDatabase = new Helpers.HelperSqlDatabase();
        // private Helpers.HelperGoogleDrive _helperGoogleDrive = new Helpers.HelperGoogleDrive();

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
                    // obter os tipos de monitoramento!
                    listOfSettings = HelperTaskService.LoadSettings(_configuration);

                    foreach (var setting in listOfSettings)
                    {

                        if (setting.IsActive && setting.Name.Contains("SqlDatabase"))
                        {
                            //executa este setting de backup SQL
                            Adapters.AdapterSqlDatabase sqlDatabase = new Adapters.AdapterSqlDatabase(setting);
                            await WriteLog("Consultando lote das:" + DateTime.Now.ToShortTimeString());
                            var schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour);

                            if (schedules.Count < 1)
                                await WriteLog("Nenhum backup agendado para este horario!");
                            while (schedules.Count > 0)
                            {
                                foreach (var sch in schedules)
                                {
                                    await WriteLog("Iniciando processo backup de: " + sch.Databases.DatabaseName + " " + sch.DateHourExecution.Hour);
                                }
                                for (int i = 0; i < schedules.Count; i++)
                                {                                    
                                    int isExecute = schedules[i].DateHourExecution.Minute.CompareTo(DateTime.Now.Minute);
                                    int lastExecution = schedules[i].DateHourLastExecution.Date.CompareTo(DateTime.Now.Date);
                                    if ((isExecute == 0 && lastExecution == -1) || (isExecute == -1 && lastExecution == -1) && !schedules[i].BackupStatus.Equals(Model.Enum.BackupStatus.Runing))
                                    {
                                        
                                        var resp = _helperSqlDatabase.BackupRun(schedules[i], setting);
                                        
                                    }
                                }
                                await WriteLog("Todos do lote foram finalizados. Procurando novos nesse intervalo de tempo: " + DateTime.Now.Hour);
                                await Task.Delay((setting.UpdateInterval * 60) * 1000, stoppingToken);
                                schedules = await sqlDatabase.GetDatabaseBackupSchedule(setting.Url, DateTime.Now.Hour);
                                if(schedules.Count < 0)
                                    await WriteLog("Nenhum backup encontrado, saindo do loop.");
                            }
                            await Task.Delay((setting.UpdateInterval * 60) * 1000, stoppingToken);
                        }
                    }
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch(Exception err)
            {
                _utilTelegram.SendMessage($"Falha no serviço TaskManager: {err.Message}", _configuration.GetSection("Services").GetSection("UidTelegramDestino").Value);
                await WriteLog(err.Message);
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
            await sw.WriteAsync(DateTime.Now.ToString() + ": Celta Task Service - " + _msg);
            sw.Flush();
            sw.Close();
            return true;
        }
    }
}
