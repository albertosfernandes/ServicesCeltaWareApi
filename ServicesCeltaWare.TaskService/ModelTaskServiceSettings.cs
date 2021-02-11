using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.TaskService
{
    public class ModelTaskServiceSettings
    {
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string Url { get; set; }
        public int UpdateInterval { get; set; }
        public bool IsDebug { get; set; }
        public string UidTelegramToken { get; set; }
        public string UidTelegramDestino { get; set; }
    }
}
