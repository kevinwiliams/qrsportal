using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRSPortal2.Models
{
    public class CircProUserList
    {
        public string CELL_NUMBER { get; set; }
        public int DISTRIBUTION_TYPE_ID { get; set; }
        public string ACCOUNT { get; set; }
        public string PHONE_NUMBER { get; set; }
        public int ADDRESSID { get; set; }
        public string ADDRESS_LINE1 { get; set; }
        public string LAST_NAME { get; set; }
        public string DISTRIBUTION_TYPE_NAME { get; set; }
        public int DISTRIBUTION_ID { get; set; }
        public int NAMEDENTITYID { get; set; }
        public string FIRST_NAME { get; set; }
        public string EMAIL { get; set; }
        public string COMPANY { get; set; }
        public string ADDRESS_LINE2 { get; set; }
        public string ADDRESS_LINE3 { get; set; }
    }

    public class CircProDistribution
    {
        public int DISTRIBUTION_TYPE_ID { get; set; }
        public DateTime PUBLISH { get; set; }
        public string DIST_ACCTNBR { get; set; }
        public byte DRAWTOT { get; set; }
        public int DISTRIBUTION_ID { get; set; }
        public int NAMEDENTITYID { get; set; }
        public byte RETTOT { get; set; }
    }

}