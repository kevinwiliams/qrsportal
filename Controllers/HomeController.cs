using QRSPortal2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            if (Request.IsAuthenticated)
            {
                ViewData["UserRole"] = (User.IsInRole("Retailer") ? "Retailer" : (User.IsInRole("Circulation") ? "Circulation" : (User.IsInRole("Supervisor") ? "Supervisor" : "Admin")));
                
                if (User.IsInRole("Circulation") || User.IsInRole("Admin")) {
                    try
                    {
                        List<TransactionData> weeklyData = GetWeeklyTransactionData();
                        List<TransactionData> monthlyData = GetMonthlyTransactionData();

                        TransactionDataViewModel viewModel = new TransactionDataViewModel
                        {
                            WeeklyData = weeklyData,
                            MonthlyData = monthlyData
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
    }
}