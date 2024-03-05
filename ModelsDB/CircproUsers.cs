using QRSPortal2.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QRSPortal2.ModelsDB
{
    public class CircproUsers
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Index(IsUnique = true)]
        public int Id { get; set; }

        [Key, ForeignKey("ApplicationUser")]
        [StringLength(128)]
        public string UserID { get; set; }
        [StringLength(10)]
        public string AccountID { get; set; }
        public int DistributionID { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }
        [StringLength(100)]
        public string LastName { get; set; }
        [StringLength(100)]
        public string Company { get; set; }
        [StringLength(500)]
        public string PhoneNumber { get; set; }
        [StringLength(50)]
        public string CellNumber { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }
        public bool IsActive { get; set; }
        public Nullable<int> AddressID { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public Nullable<System.DateTime> LastLogin { get; set; }
        public bool NotifyEmail { get; set; }

        //Navigation properties
        [ForeignKey("UserID")]
        public ApplicationUser ApplicationUser { get; set; } 
       
    }
}