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
        [Display(Name = "Account ID")]
        public string AccountID { get; set; }
        [Display(Name = "IP")]
        public string IPAddress { get; set; }
        public string LogInformation { get; set; }
        public string SystemInformation { get; set; }
        [Display(Name = "User")]
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MMM-dd}")]
        [Display(Name = "PubDate")]
        public DateTime PublicationDate { get; set; }
        [Display(Name = "RET")]
        public int ReturnAmount { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        [Display(Name = "Updated")]
        public DateTime CreatedAt { get; set; }


    }
}