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
            //List<ModelBackupSchedule> BackupsSchedules = new List<ModelBackupSchedule>();
            //ModelCustomerProduct CustomerProduct = new ModelCustomerProduct();
            //ModelBackupSchedule BackupSchedule = new ModelBackupSchedule();
        }
        [Key]
        public int DatabasesId { get; set; }
        public string  ConteinerName { get; set; }
        public string DatabaseName { get; set; }
        public int CustomersProductsId { get; set; }
        public ModelCustomerProduct CustomerProduct { get; set; }
        public int MemoryRam { get; set; }
        public string Directory { get; set; }
        //public int BackupScheduleId { get; set; }
        //public ModelBackupSchedule BackupSchedule { get; set; }
        //public List<ModelBackupSchedule> BackupsSchedules { get; set; }
    }
}
