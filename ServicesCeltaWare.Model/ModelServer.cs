using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelServer
    {
        [Key]
        public int ServersId { get; set; }        
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public string Hostname { get; set; }
        //public ICollection<ModelCustomerProduct> CustomerProducts { get; set; }
    }
}
