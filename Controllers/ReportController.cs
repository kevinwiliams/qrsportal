using QRSPortal2.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    [Authorize(Roles = "Admin, Circulation")]
    public class ReportController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private AccountController _ac = new AccountController();
        // GET: Report
        public ActionResult Supervisor()
        {
            try
            {
                var sql = @"SELECT [UserName]
                              ,[EmailAddress]
                              ,[AccountID]
                              ,[RetailerName]
                              ,[Company]
                              ,[RetailerAddress]
                              ,[PublicationDate]
                              ,[TotalReturnAmount]
                              ,[TotalDistributionAmount]
                              ,[CreatedAt]
                          FROM [QRS_DB].[dbo].[View_Supervisor_Report]";

                var result = _db.Database.SqlQuery<SupervisorReport>(sql).ToList();

                var supervisorList = new List<SelectListItem>();
                var supervisorNames = result
                    .Where(u => u.UserName != null && u.EmailAddress != null)  // Filter out null email addresses
                    .GroupBy(u => u.EmailAddress)  // Group by email address
                    .Select(group => group.First())  // Select the first user within each group
                    .OrderBy(u => u.UserName)  // Order by username
                    .Distinct()
                    .ToList();

                foreach (var supervisor in supervisorNames)
                    if(Util.IsUserInRole(supervisor.EmailAddress, "Supervisor"))
                        supervisorList.Add(new SelectListItem { Text = supervisor.UserName, Value = supervisor.UserName });

                ViewBag.Supervisors = supervisorList;

                _ac.InitializeController(this.Request.RequestContext);
                ViewData["UserName"] = _ac.GetUserData()["UserName"];

                return View(result);

            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }
        }
               
        [HttpPost]
        public ActionResult Supervisor(string supervisors, DateTime? startDate, DateTime? endDate)
        {
            try
            {

                // SQL query without filtering conditions initially
                var sql = @"
                    SELECT [UserName]
                          ,[EmailAddress]
                          ,[AccountID]
                          ,[RetailerName]
                          ,[Company]
                          ,[RetailerAddress]
                          ,[PublicationDate]
                          ,[TotalReturnAmount]
                          ,[TotalDistributionAmount]
                          ,[CreatedAt]
                    FROM [QRS_DB].[dbo].[View_Supervisor_Report]";

                var supeList = _db.Database.SqlQuery<SupervisorReport>(sql).ToList();
                var supervisorList = new List<SelectListItem>();
                var supervisorNames = supeList.Where(u => u.UserName != null && u.EmailAddress != null).GroupBy(u => u.EmailAddress).Select(group => group.First())
                    .OrderBy(u => u.UserName)
                    .Distinct()
                    .ToList();

                foreach (var supervisor in supervisorNames)
                    if (Util.IsUserInRole(supervisor.EmailAddress, "Supervisor"))
                        supervisorList.Add(new SelectListItem { Text = supervisor.UserName, Value = supervisor.UserName });

                ViewBag.Supervisors = supervisorList;
                // List to store SQL parameters
                var parameters = new List<SqlParameter>();

                // Check if supervisor name is provided
                if (!string.IsNullOrEmpty(supervisors))
                {
                    sql += " WHERE [UserName] LIKE '%' + @supervisor + '%'";
                    parameters.Add(new SqlParameter("@supervisor", supervisors));
                }

                // Check if start date is provided
                if (startDate.HasValue)
                {
                    if (parameters.Any())
                        sql += " AND [PublicationDate] >= @startDate";
                    else
                        sql += " WHERE [PublicationDate] >= @startDate";

                    parameters.Add(new SqlParameter("@startDate", startDate.Value));
                }

                // Check if end date is provided
                if (endDate.HasValue)
                {
                    if (parameters.Any())
                        sql += " AND [PublicationDate] <= @endDate";
                    else
                        sql += " WHERE [PublicationDate] <= @endDate";

                    parameters.Add(new SqlParameter("@endDate", endDate.Value));
                }

                // Execute the SQL query with parameters
                var result = _db.Database.SqlQuery<SupervisorReport>(sql, parameters.ToArray()).ToList();
                
                _ac.InitializeController(this.Request.RequestContext);
                ViewData["UserName"] = _ac.GetUserData()["UserName"];

                return View(result);
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }
            
        }

        public ActionResult Transactions()
        {
            try
            {
                var sql = @"SELECT TOP 30
                              [AccountID]
                              ,[RetailerName]
                              ,[Company]
                              ,[RetailerAddress]
                              ,[PublicationDate]
                              ,[TotalReturnAmount]
                              ,[TotalConfirmedAmount]
                              ,[TotalDistributionAmount]
                              ,[CreatedAt]
                              ,[Status]
                          FROM [QRS_DB].[dbo].[View_Transactions_Report]";

                var result = _db.Database.SqlQuery<TransactionsReport>(sql).ToList();

                _ac.InitializeController(this.Request.RequestContext);
                ViewData["UserName"] = _ac.GetUserData()["UserName"];

                return View(result);

            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }
        }

        [HttpPost]
        public ActionResult Transactions(string pubStatus, DateTime? startDate, DateTime? endDate)
        {
            try
            {

                // SQL query without filtering conditions initially
                var sql = @"
                    SELECT 
                          [AccountID]
                          ,[RetailerName]
                          ,[Company]
                          ,[RetailerAddress]
                          ,[PublicationDate]
                          ,[TotalReturnAmount]
                          ,[TotalConfirmedAmount]
                          ,[TotalDistributionAmount]
                          ,[CreatedAt]
                          ,[Status]
                    FROM [QRS_DB].[dbo].[View_Transactions_Report]";

                // List to store SQL parameters
                var parameters = new List<SqlParameter>();

                // Check if supervisor name is provided
                if (!string.IsNullOrEmpty(pubStatus))
                {
                    sql += " WHERE [Status] = @status";
                    parameters.Add(new SqlParameter("@status", pubStatus));
                }

                // Check if start date is provided
                if (startDate.HasValue)
                {
                    if (parameters.Any())
                        sql += " AND [PublicationDate] >= @startDate";
                    else
                        sql += " WHERE [PublicationDate] >= @startDate";

                    parameters.Add(new SqlParameter("@startDate", startDate.Value));
                }

                // Check if end date is provided
                if (endDate.HasValue)
                {
                    if (parameters.Any())
                        sql += " AND [PublicationDate] <= @endDate";
                    else
                        sql += " WHERE [PublicationDate] <= @endDate";

                    parameters.Add(new SqlParameter("@endDate", endDate.Value));
                }

                // Execute the SQL query with parameters
                var result = _db.Database.SqlQuery<TransactionsReport>(sql, parameters.ToArray()).ToList();

                _ac.InitializeController(this.Request.RequestContext);
                ViewData["UserName"] = _ac.GetUserData()["UserName"];

                return View(result);
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return View();
            }

        }

        // GET: Report/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Report/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Report/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Report/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Report/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Report/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Report/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
