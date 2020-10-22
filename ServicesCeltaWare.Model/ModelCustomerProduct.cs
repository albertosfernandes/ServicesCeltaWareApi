using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelCustomerProduct
    {
        public ModelCustomerProduct()
        {
           
        }

        [Key]
        public int CustomersProductsId { get; set; }
        
        public int CustomerId { get; set; }
        public ModelCustomer Customer { get; set; }
        public int ProductId { get; set; }
        public ModelProduct Product { get; set; }
        public int ServersId { get; set; }
        public ModelServer Server { get; set; }        
        public string AddressName { get; set; }
        public string IpAddress { get; set; }
        public string Port { get; set; }
        public string LoginUser { get; set; }
        public string LoginPassword { get; set; }
        public string InstallDirectory { get; set; }
        public string SynchronizerServiceName { get; set; }
        public bool IsCreated { get; set; }
    }
}
