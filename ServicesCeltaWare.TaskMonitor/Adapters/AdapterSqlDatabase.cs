using ServicesCeltaWare.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServicesCeltaWare.TaskMonitor.Adapters
{
    public class AdapterSqlDatabase : IAdaptersInterface
    {
        private readonly ModelTaskMonitorSettings _settings;        
        private static readonly HttpClient client = new HttpClient();
        public AdapterSqlDatabase(ModelTaskMonitorSettings settings)
        {
            _settings = settings;
        }

        public async Task<List<ModelServer>> GetServers()
        {
            var streamResult = client.GetStreamAsync(_settings.Url + "/api/Servers/GetAll");
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var servers = await JsonSerializer.DeserializeAsync<List<ModelServer>>(await streamResult, options);

            return servers;
        }

        public async Task<List<ModelDatabase>> GetDataBases(ModelServer _server)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            List<ModelDatabase> databases = new List<ModelDatabase>();

            var streamDbases = client.GetStreamAsync($"http://{_server.IpAddress}:{_server.Port}/api/DatabaseService/GetAllByServer?serverId={_server.ServersId}");
            databases = await JsonSerializer.DeserializeAsync<List<ModelDatabase>>(await streamDbases, options);

            return databases;
        }

        public async Task<List<ModelBackupSchedule>> GetDatabaseBackupSchedule(string _url, int hourNow)
        {
            try
            {
                List<ModelBackupSchedule> backupSchedules = new List<ModelBackupSchedule>();
                var options = new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                //string ip = _database.CustomerProduct.Server.IpAddress;
                //string port = _database.CustomerProduct.Server.Port.ToString();

                var streamBackupSchedule = client.GetStreamAsync($"{_url}/api/DatabaseSchedule/GetAllByTime?hourSchedule={hourNow}");
                backupSchedules = await JsonSerializer.DeserializeAsync<List<ModelBackupSchedule>>(await streamBackupSchedule, options);

                return backupSchedules;
            }
            catch(Exception err)
            {
                throw err;
            }
        }




        //recebe lote
        public async Task<List<ModelBackupSchedule>> GetItens(int hour)
        {
            var streamResult = client.GetStreamAsync(_settings.Url+ "/api/Servers/GetAll");
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var servers = await JsonSerializer.DeserializeAsync<List<ModelServer>>(await streamResult, options);
            
            List<ModelDatabase> databases = new List<ModelDatabase>();
            foreach (var server in servers)
            {
                //agora descobrir qual desses servidores possuem banco de dados!!!
                var streamDbases = client.GetStreamAsync($"http://{server.IpAddress}:{server.Port}/api/DatabaseService/GetAllByServer?serverId={server.ServersId}");
                databases = await JsonSerializer.DeserializeAsync<List<ModelDatabase>>(await streamDbases, options);
            }

            List<ModelBackupSchedule> backupSchedules = new List<ModelBackupSchedule>();
            foreach (var database in databases)
            {
                string ip = database.CustomerProduct.Server.IpAddress;
                string port = database.CustomerProduct.Server.Port.ToString();
                //agora descobrir quantos agendamentos existem nesse horario! 
                var streamBackupSchedule = client.GetStreamAsync($"http://{ip}:{port}/api/DatabaseSchedule/GetAllByTime?hourSchedule={hour}");
                backupSchedules = await JsonSerializer.DeserializeAsync<List<ModelBackupSchedule>>(await streamBackupSchedule, options);
            }

            return backupSchedules;


            //foreach (var backupSchedule in backupSchedules)
            //{
            //    TimeSpan hourCurrent = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

            //    var isExecute = hourCurrent.CompareTo(backupSchedule.DateHourExecution);


            //}

        }

        //verifica se esta na hora
    }
}
