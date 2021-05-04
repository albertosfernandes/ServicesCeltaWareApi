using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelDatabase
    {
        public ModelDatabase()
        {
            ModelCustomerProduct CustomerProduct = new ModelCustomerProduct();
            List<ModelDatabaseUser> DatabaseUsers = new List<ModelDatabaseUser>();
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
        public IList<ModelDatabaseUser> DatabaseUsers { get; set; }
        [NotMapped]
        public ModelDatabaseUser DatabaseUserSa
        {
            get
            {
                ModelDatabaseUser dataUserSa = new ModelDatabaseUser();
                if(DatabaseUsers == null)
                {
                    return dataUserSa;
                }
                else
                {
                    foreach (var i in DatabaseUsers)
                    {
                        if (i.Name.ToUpper().Contains("SA"))
                        {
                            dataUserSa = i;
                        }
                    }
                    return dataUserSa;
                }
            }
        }
        [NotMapped]
        public ModelDatabaseUser DatabaseUserCeltaBS
        {
            get
            {
                ModelDatabaseUser dataUserCeltaBS = new ModelDatabaseUser();
                if(DatabaseUsers == null)
                {
                    return dataUserCeltaBS;
                }
                else
                {
                    foreach (var i in DatabaseUsers)
                    {
                        if (i.Name.ToUpper().Contains("CELTABSUSER"))
                        {
                            dataUserCeltaBS = i;
                        }
                    }
                    return dataUserCeltaBS;
                }
            }
        }

    }
}
