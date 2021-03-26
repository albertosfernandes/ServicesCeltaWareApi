using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelAppSincWeb : IApps
    {
        [Key]
        public int AppSincWebsId { get; set; }
        public int CustomersProductsId { get; set; }
        public ModelCustomerProduct CustomerProduct { get; set; }
        public string AddressName { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string InstallDirectory { get; set; }
        public bool IsCreated { get; set; }

        [NotMapped]
        public string UserName { get; set; }
        [NotMapped]
        public string Password { get; set; }
    }
}
