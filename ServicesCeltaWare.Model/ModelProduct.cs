using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelProduct
    {
        [Key]
        public int ProductId { get; set; }
        public string Name { get; set; }

        //public virtual IList<ModelCustomerProduct> CustomersProducts { get; set; }
    }
}
