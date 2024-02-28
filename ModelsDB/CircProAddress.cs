using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRSPortal2.ModelsDB
{
    public class CircProAddress
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressID { get; set; }
        [StringLength(128)]
        public string UserID { get; set; }
        [StringLength(128)]
        public string AccountID { get; set; }
        [StringLength(5)]
        public string AddressType { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(100)]
        public string AddressLine1 { get; set; }
        [StringLength(100)]
        public string AddressLine2 { get; set; }
        [StringLength(50)]
        public string CityTown { get; set; }
        [StringLength(50)]
        public string StateParish { get; set; }
        [StringLength(10)]
        public string ZipCode { get; set; }
        [StringLength(50)]
        public string CountryCode { get; set; }
        public System.DateTime CreatedAt { get; set; }
        [ForeignKey("UserID")]
        public virtual CircproUsers CircoProUser { get; set; }
    }
}