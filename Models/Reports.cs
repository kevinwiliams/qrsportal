﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Models
{
    public class Reports
    {
        public SupervisorReport SupervisorReport { get; set; }
        public TransactionsReport TransactionsReport { get; set; }
    }

    public class SupervisorReport
    {
        [Display(Name = "Supervisor")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string EmailAddress { get; set; }
        [Display(Name = "Account")]
        public string AccountID { get; set; }
        [Display(Name = "Retailer")]
        public string RetailerName { get; set; }
        public string Company { get; set; }
        [Display(Name = "Address")]
        public string RetailerAddress { get; set; }
        [Display(Name = "Pub Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime PublicationDate { get; set; }
        [Display(Name = "RET")]
        public int TotalReturnAmount { get; set; }
        [Display(Name = "DRAW")]
        public int TotalDistributionAmount { get; set; }
        [Display(Name = "Logged")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}")]
        public DateTime CreatedAt { get; set; }
        public string Supervisors { get; set; }
        public string Retailers { get; set; }
       
    }

    public class TransactionsReport
    {

        [Display(Name = "Account")]
        public string AccountID { get; set; }
        [Display(Name = "Retailer")]
        public string RetailerName { get; set; }
        public string Company { get; set; }
        [Display(Name = "Address")]
        public string RetailerAddress { get; set; }
        [Display(Name = "Pub Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime PublicationDate { get; set; }
        [Display(Name = "RET")]
        public int TotalReturnAmount { get; set; }
        [Display(Name = "CONF")]
        public int TotalConfirmedAmount { get; set; }
        [Display(Name = "DRAW")]
        public int TotalDistributionAmount { get; set; }
        [Display(Name = "Logged")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd hh:mm:ss}")]
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public string Supervisors { get; set; }
        public string Retailers { get; set; }

    }
}