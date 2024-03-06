using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QRSPortal2.Models
{
    public class DistributionData
    {
        [Display(Name = "Account")]
        public string AccountID { get; set; }
        public int DistributionTypeID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MMM-dd}")] 
        [Display(Name = "PUB")]
        public DateTime PublicationDate { get; set; }
        [Display(Name = "DRAW")]
        public int DistributionAmount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")] 
        public DateTime ReturnDate { get; set; }
        [Display(Name = "RET")]
        public int ReturnAmount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")] 
        public DateTime ConfirmDate { get; set; }
        [Display(Name = "C")]
        public int ConfirmedAmount { get; set; }
        public int DisputeAmount { get; set; }
        public bool ConfirmReturn { get; set; }
        public string Status { get; set; }

        public string RetailerName { get; set; }
        public string Company { get; set; }
        public string RetailerAddress { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string CellNumber { get; set; }
    }


}