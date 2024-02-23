using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRSPortal2.ModelsDB
{
    public class LogErrors
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ErrID { get; set; }
        [StringLength(255)]
        public string Err_Msg { get; set; }
        [StringLength(25)]
        public string Err_Date { get; set; }
        [StringLength(10)]
        public string Err_time { get; set; }
        [StringLength(255)]
        public string Err_Name { get; set; }
        public string StackTrace { get; set; }
    }
}