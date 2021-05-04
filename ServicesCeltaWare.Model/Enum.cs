using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class Enum
    {
        public enum ProductName
        {
            None = 0,
            BSF = 1,
            CCS = 2,
            CSS = 3,
            Concentrador = 4,
            SynchronizerService = 5,
            Database = 6,
            CertificadoA1 = 7,
            MysqlDatabase = 8
        }

        public enum BackuypType
        {
            Full = 0,
            Diferential = 1,
            Incremental = 2,
            MysqlFull = 3
        }

        public enum BackupStatus
        {
            Success = 0,
            Failed = 1,
            Runing = 2,
            Corrupted = 3,
            None = 4,
            OutOfDate = 5,
            Uploading = 6
        }

        public enum ValidateType
        {
            LabelOnly = 0,
            VerifyOnly = 1
        }
    }
}
