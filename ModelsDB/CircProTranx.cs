using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRSPortal2.ModelsDB
{
    public class CircProTransactions
    {
        // This was changed  to follow convention and prevent errors
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CircProTranxID { get; set; }
        [StringLength(128)]
        public string UserID { get; set; }
        [StringLength(128)]
        public string AccountID { get; set; }
        [StringLength(50)]
        public string EmailAddress { get; set; }
        public int DistributionTypeID { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime PublicationDate { get; set; }
        public int DistributionAmount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<DateTime> ReturnDate { get; set; }
        public Nullable<int> ReturnAmount { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public Nullable<DateTime> ConfirmDate { get; set; }
        public Nullable<int> ConfirmedAmount { get; set; }
        public Nullable<bool> ConfirmReturn { get; set; }
        [StringLength(10)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Nullable<DateTime> UpdatedAt { get; set; }
        [ForeignKey("UserID")]
        public CircproUsers CircproUser { get; set; }
    }
}