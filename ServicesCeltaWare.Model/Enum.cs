using System;
using System.Collections.Generic;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class Enum
    {
        public enum BackuypType
        {
            Full = 0,
            Diferential = 1
        }

        public enum BackupStatus
        {
            Success = 0,
            Failed = 1,
            Runing = 2,
            Corrupted = 3,
            None = 4
        }
    }
}
