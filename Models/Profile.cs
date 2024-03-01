using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using QRSPortal2.ModelsDB;

namespace QRSPortal2.Models
{
    public class Profile
    {
        [Display(Name = "Name")]
        public string RetailerName { get; set; }
        [Display(Name = "Address")]
        public string Address { get; set; }
        public CircproUsers RetailerInfo { get; set; } 
        public CircProAddress RetailerAddress { get; set; }
        public ChangePasswordViewModel ChangePassword { get; set; }

    }
}
