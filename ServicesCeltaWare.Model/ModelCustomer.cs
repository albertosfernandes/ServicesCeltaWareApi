using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{    
    public class ModelCustomer
    {
        [Key]
        public int CustomerId { get; set; }
        public string CompanyName { get; set; }
        public string FantasyName { get; set; }
        public string Cnpj { get; set; }
        public int CodeCeltaBs { get; set; }
        public string RootDirectory { get; set; }
        //public string CustomersProducts { get; set; }

        //public  virtual IList<ModelCustomerProduct> CustomersProducts { get; set; }
    }
}
