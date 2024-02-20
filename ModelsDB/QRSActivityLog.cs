using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRSPortal2.ModelsDB
{
    public class QRSActivityLog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActivityLogID { get; set; }
        public string AccountID { get; set; }
        public string IPAddress { get; set; }
        public string LogInformation { get; set; }
        public string SystemInformation { get; set; }
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime PublicationDate { get; set; }
        public int ReturnAmount { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }


    }
}