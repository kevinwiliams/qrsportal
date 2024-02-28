using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        [Authorize]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                ViewData["UserRole"] = (User.IsInRole("Retailer") ? "Retailer" : (User.IsInRole("Circulation") ? "Circulation" : (User.IsInRole("Supervisor") ? "Supervisor" : "Admin")));
                ViewData["UserName"] = _db.Users.FirstOrDefault(x => x.Email == User.Identity.Name).FullName;


                if (User.IsInRole("Circulation") || User.IsInRole("Admin")) {
                    try
                    {
                        //List<TransactionData> weeklyData = GetWeeklyTransactionData();
                        //List<TransactionData> monthlyData = GetMonthlyTransactionData();
                        List<QRSActivityLog> activityLogs = GetRecentActivities();

                        TransactionDataViewModel viewModel = new TransactionDataViewModel
                        {
                            //WeeklyData = weeklyData,
                            //MonthlyData = monthlyData,
                            ActivityLogs = activityLogs
                        };
                        return View(viewModel);

                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                   

                if (User.IsInRole("Supervisor") || User.IsInRole("Retailer"))
                {
                    try
                    {
                        using (ApplicationDbContext dc = new ApplicationDbContext())
                        {
                            var accountId = dc.CircproUsers.Where(a => a.EmailAddress == User.Identity.Name).FirstOrDefault();

                            if (accountId != null)
                            {
                                ViewData["UserName"] = _db.Users.FirstOrDefault(x => x.Email == User.Identity.Name).FullName;
                                return RedirectToAction("account", "distribution", new { id = accountId.AccountID });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }
                //return RedirectToAction("dashboard", "account");
            }

            return View();
        }

        private List<TransactionData> GetMonthlyTransactionData()
        {
            List<TransactionData> list = new List<TransactionData>();
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    using (var cxt = new ApplicationDbContext())
                    {

                        var sql = @"SELECT MONTH(PublicationDate) AS PeriodNumber,
                                           SUM(DistributionAmount) AS TotalDistributionAmount,
                                           SUM(ReturnAmount) AS TotalReturnAmount,
                                           SUM(ConfirmedAmount) AS TotalConfirmedAmount
                                    FROM [dbo].[CircProTransactions]
                                    GROUP BY MONTH(PublicationDate)
                                    ORDER BY PeriodNumber";

                        var result = cxt.Database.SqlQuery<TransactionData>(sql).ToList();
                        //TODO:Loop to trim address field
                        foreach (var item in result)
                        {
                            item.PeriodText = "Month " + item.PeriodNumber;
                        }

                        return result;

                    }
                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return list;
            }
        }

        private List<QRSActivityLog> GetRecentActivities()
        {
            List<QRSActivityLog> list = new List<QRSActivityLog>();
            try
            {
                list = _db.QRSActivityLog.Take(9).OrderByDescending(x => x.CreatedAt).ToList();

                foreach (var item in list)
                {
                    item.LogInformation = FormatRelativeDate(item.CreatedAt);
                }

                return list;
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return list;
            }
        }

        private List<TransactionData> GetWeeklyTransactionData()
        {
            List<TransactionData> list = new List<TransactionData>();
            try
            {
                using (ApplicationDbContext db = new ApplicationDbContext())
                {
                    using (var cxt = new ApplicationDbContext())
                    {

                        var sql = @"SELECT DATEPART(week, PublicationDate) AS PeriodNumber,
                                            SUM(DistributionAmount) AS TotalDistributionAmount,
                                            SUM(ReturnAmount) AS TotalReturnAmount,
                                            SUM(ConfirmedAmount) AS TotalConfirmedAmount
                                    FROM [dbo].[CircProTransactions]
                                    GROUP BY DATEPART(week, PublicationDate)
                                    ORDER BY PeriodNumber";

                        var result = cxt.Database.SqlQuery<TransactionData>(sql).ToList();

                        foreach (var item in result)
                        {
                            item.PeriodText = "Week " + item.PeriodNumber;
                        }
                        //TODO:Loop to trim address field

                        return result;

                    }
                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return list;
            }
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> GetChartData(string aggregationType)
        {
            try
            {
                var currentDate = DateTime.Now;
                int currentYear = DateTime.Now.Year;
                int currentMonth = DateTime.Now.Month;
                var startDate = new DateTime(currentDate.Year, 1, 1); // Start of the current year

                TransactionDataViewModel viewModel = new TransactionDataViewModel();
                IQueryable<TransactionData> query;


                switch (aggregationType.ToLower())
                {
                    case "monthly":
                        query = _db.CircProTranx.AsNoTracking()
                            .Where(t => t.PublicationDate.Year == currentYear && t.PublicationDate >= startDate)
                            .GroupBy(t => new { Month = SqlFunctions.DatePart("month", t.PublicationDate) })
                            .Select(g => new TransactionData
                            {
                                PeriodNumber = (int)g.Key.Month,
                                TotalDistributionAmount = g.Sum(t => t.DistributionAmount),
                                TotalReturnAmount = (int)g.Sum(t => t.ReturnAmount),
                                TotalConfirmedAmount = (int)g.Sum(t => t.ConfirmedAmount)
                            }).OrderBy(g => g.PeriodNumber); 
                        break;
                    case "weekly":
                        query = _db.CircProTranx.AsNoTracking()
                            .Where(t => t.PublicationDate.Year == currentYear && t.PublicationDate >= startDate)
                            .GroupBy(t => new { Week = SqlFunctions.DatePart("week", t.PublicationDate) })
                            .Select(g => new TransactionData
                            {
                                PeriodNumber = (int)g.Key.Week,
                                TotalDistributionAmount = g.Sum(t => t.DistributionAmount),
                                TotalReturnAmount = (int)g.Sum(t => t.ReturnAmount),
                                TotalConfirmedAmount = (int)g.Sum(t => t.ConfirmedAmount)
                            }).OrderBy(g => g.PeriodNumber); 
                        break;
                    case "daily":
                        query = _db.CircProTranx.AsNoTracking()
                            .Where(t => t.PublicationDate.Year == currentYear && t.PublicationDate.Month == currentMonth && t.PublicationDate >= startDate)
                            .GroupBy(t => SqlFunctions.DatePart("day", t.PublicationDate))
                            .Select(g => new TransactionData
                            {
                                PeriodNumber = (int)g.Key,
                                TotalDistributionAmount = (int)g.Sum(t => t.DistributionAmount),
                                TotalReturnAmount = (int)g.Sum(t => t.ReturnAmount),
                                TotalConfirmedAmount = (int)g.Sum(t => t.ConfirmedAmount)
                            }).OrderBy(g => g.PeriodNumber); 
                        break;
                    default:
                        return Json(new { success = false, message = "Invalid aggregation type" });
                }

                var data = await query.ToListAsync();

                switch (aggregationType.ToLower())
                {
                    case "monthly":
                        foreach (var item in data)
                            item.PeriodText = "Month " + item.PeriodNumber;
                        break;
                    case "weekly":
                        foreach (var item in data)
                            item.PeriodText = "Week " + item.PeriodNumber;
                        break;
                    case "daily":
                        foreach (var item in data)
                            item.PeriodText = " " + item.PeriodNumber;
                        break;
                    default:
                        break;
                }

                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [Authorize]
        public ActionResult Retailer()
        {
            return View();
        }
        [Authorize]
        public ActionResult EnterReturn(string showInputConfirm) 
        {
            ViewBag.ShowConfirm = showInputConfirm;
            return View();
        }
        [Authorize]
        public ActionResult ShowHistory()
        {
            return View();
        }
        [Authorize]
        public ActionResult Circulation()
        {
            return View();
        }

        public static string FormatRelativeDate(DateTime date)
        {
            TimeSpan timeDifference = DateTime.Now - date;

            if (timeDifference.TotalMinutes < 60)
            {
                return $"{(int)timeDifference.TotalMinutes} min";
            }
            else if(timeDifference.TotalHours < 24)
            {
                return $"{(int)timeDifference.TotalHours} hrs";
            }
            else if (timeDifference.TotalDays < 2)
            {
                return "1 day";
            }
            else if (timeDifference.TotalDays < 7)
            {
                return $"{(int)timeDifference.TotalDays} days";
            }
            else if (timeDifference.TotalDays < 14)
            {
                return "1 week";
            }
            else if (timeDifference.TotalDays < 30)
            {
                int weeks = (int)(timeDifference.TotalDays / 7);
                return $"{weeks} weeks";
            }
            else if (timeDifference.TotalDays < 60)
            {
                return "1 month";
            }
            else if (timeDifference.TotalDays < 365)
            {
                int months = (int)(timeDifference.TotalDays / 30);
                return $"{months} months";
            }
            else
            {
                int years = (int)(timeDifference.TotalDays / 365);
                return $"{years} years";
            }
        }

        

    }
}