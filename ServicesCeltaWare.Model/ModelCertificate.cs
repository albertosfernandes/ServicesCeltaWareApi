using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelCertificate
    {
        [Key]
        public int CertificateId { get; set; }
        public ModelCustomer Customer { get; set; }
        public int CustomerId { get; set; }
        public string FileRepositorie { get; set; }
        public string FileName { get; set; }
        public string FriendlyName { get; set; }
        public DateTime DateHourExpiration { get; set; }
        public string Password { get; set; }
        public string HashCert { get; set; }
        public bool IsInstalled { get; set; }

    }
}
