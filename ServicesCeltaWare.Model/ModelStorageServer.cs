using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelStorageServer
    {
        public ModelStorageServer()
        {
            ModelServer Server = new ModelServer();
        }

        [Key]
        public int StorageServerId { get; set; }
        public string TargetName { get; set; }
        public string Portal { get; set; }
        public int ServersId { get; set; }
        public ModelServer Server { get; set; }
    }
}
