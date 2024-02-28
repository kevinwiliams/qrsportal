using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QRSPortal2.Models
{
    public class TransactionData
    {
        public int PeriodNumber { get; set; }
        public int TotalDistributionAmount { get; set; }
        public int TotalReturnAmount { get; set; }
        public int TotalConfirmedAmount { get; set; }
        public string PeriodText { get; set; }
    }

    public class TransactionDataViewModel
    {
        public List<TransactionData> WeeklyData { get; set; }
        public List<TransactionData> MonthlyData { get; set; }
        public List<TransactionData> DailyData { get; set; }
        public List<QRSActivityLog> ActivityLogs { get; set; }
    }
}