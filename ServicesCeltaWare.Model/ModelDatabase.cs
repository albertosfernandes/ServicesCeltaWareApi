using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelDatabase
    {
        public ModelDatabase()
        {
            ModelCustomerProduct CustomerProduct = new ModelCustomerProduct();
        }
        [Key]
        public int DatabasesId { get; set; }
        public int StorageServerId { get; set; }
        public ModelStorageServer StorageServer { get; set; }
        public string  ConteinerName { get; set; }
        public string DatabaseName { get; set; }
        public int CustomersProductsId { get; set; }
        public ModelCustomerProduct CustomerProduct { get; set; }
        public int MemoryRam { get; set; }
        public string Directory { get; set; }
        public string MapperName { get; set; }
        public int StorageLenght { get; set; }
    }
}
