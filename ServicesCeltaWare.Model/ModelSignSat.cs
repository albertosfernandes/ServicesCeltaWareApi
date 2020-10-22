using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelSignSat
    {
        [Key]
        public int SignSatId { get; set; }

        public string CnpjCustomer { get; set; }

        public string SignKey { get; set; }
    }
}
