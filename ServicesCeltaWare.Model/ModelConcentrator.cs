using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelConcentrator
    {
        [Key]
        public int ConcentratorsId { get; set; }
        public int CustomersProductsId { get; set; }
        public ModelCustomerProduct CustomerProduct { get; set; }
        public string AddressName { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string InstallDirectory { get; set; }
        public bool IsCreated { get; set; }
    }
}
