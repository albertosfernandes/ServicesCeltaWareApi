﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static ServicesCeltaWare.Model.Enum;

namespace ServicesCeltaWare.Model
{
    public class ModelBackupSchedule
    {
        [Key]
        public int BackupScheduleId { get; set; }
        public int CustomersProductsId { get; set; }
        public ModelCustomerProduct CustomerProduct { get; set; }
        public BackuypType Type { get; set; }
        public TimeSpan DateHourExecution { get; set; }
        public BackupStatus BackupStatus { get; set; }
        public int DatabasesId { get; set; }
        public ModelDatabase Databases { get; set; }
    }
}
