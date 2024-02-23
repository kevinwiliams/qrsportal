using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QRSPortal2.Models
{
    public class Retailer
    {
        [Display(Name = "Account")]
        public string AccountID { get; set; }
        [Display(Name = "Dist. ID")]
        public int DistributionID { get; set; }
        [Display(Name = "Name")]
        public string RetailerName { get; set; }
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        public string Company { get; set; }
        [Display(Name = "Address")]
        public string RetailerAddress { get; set; }
        [Display(Name = "Phone")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Cell")]
        public string CellNumber { get; set; }
    }

}