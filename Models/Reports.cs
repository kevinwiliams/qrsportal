using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRSPortal2.Models
{
    public class Reports
    {
    }

    public class SupervisorReport
    {
        public string UserName { get; set; }
        public string EmailAddress { get; set; }
        public string AccountID { get; set; }
        public string RetailerName { get; set; }
        public string Company { get; set; }
        public string RetailerAddress { get; set; }
        public DateTime PublicationDate { get; set; }
        public int TotalReturnAmount { get; set; }
        public int TotalDistributionAmount { get; set; }
    }
}