using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServicesCeltaWare.Model
{
    public class ModelDatabaseUser
    {
        [Key]
        public int DatabasesUsersId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int DatabasesId { get; set; }
    }
}
