using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace QRSPortal2.Controllers
{
    [Authorize]
    public class DistributionController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Distribution
        public ActionResult Index()
        {
            try
            {

                using (var cxt = new ApplicationDbContext())
                {

                    var sql = @"SELECT 
                                  [AccountID]
                                  ,[DistributionTypeID]
                                  ,[PublicationDate]
                                  ,[DistributionAmount]
                                  ,[ReturnDate]
                                  ,[ReturnAmount]
                                  ,[ConfirmDate]
                                  ,[ConfirmedAmount]
                                  ,[ConfirmReturn]
                                  ,[Status]
                              FROM [dbo].[CircProTransactions]";

                    var result = cxt.Database.SqlQuery<DistributionData>(sql).ToList();
                    //TODO:Loop to trim address field

                    return View(result);

                }
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return View();
            }
        }

        // GET: Distribution/Account/00021521
        public async Task<ActionResult> Account(string id)
        {
            try
            {
                var isLoaded = IsInitLoad(id);

                if (!isLoaded) {

                    var startDate = "2024-01-01";
                    var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                    AccountController ac = new AccountController();
                    ac.InitializeController(this.Request.RequestContext);

                    await ac.LoadTransactions(id, startDate, endDate);
                }

                using (var cxt = new ApplicationDbContext())
                {
                    AccountController ac = new AccountController();
                    ac.InitializeController(this.Request.RequestContext);

                    var sql = @"SELECT 
                                  T.[AccountID]
                                  ,T.[DistributionTypeID]
                                  ,T.[PublicationDate]
                                  ,T.[DistributionAmount]
                                  ,T.[ReturnDate]
                                  ,T.[ReturnAmount]
                                  ,T.[ConfirmDate]
                                  ,T.[ConfirmedAmount]
                                  ,T.[ConfirmReturn]
                                  ,T.[Status]
	                              ,CONCAT(U.FirstName, ' ', U.LastName) AS RetailerName,
                                    U.EmailAddress,
                                    U.Company,
                                    NULLIF(
                                        COALESCE(LTRIM(RTRIM(A.AddressLine1)) + ', ', '') + 
                                        COALESCE(LTRIM(RTRIM(A.AddressLine2)) + ', ', '') + 
                                        COALESCE(LTRIM(RTRIM(A.CityTown)), ''), 
                                        ''
                                    ) AS RetailerAddress,
                                    U.PhoneNumber,
                                    U.CellNumber
                              FROM [dbo].[CircProTransactions] T
                              JOIN [dbo].[CircproUsers] U ON U.AccountID = T.AccountID
                              JOIN [dbo].[CircProAddresses] A ON U.UserID = A.UserID
                                WHERE T.[AccountID] = @AccountID 
                            ORDER BY T.[PublicationDate] DESC";
                    var accountID = new SqlParameter("@AccountID", id);
                    var result =  cxt.Database.SqlQuery<DistributionData>(sql, accountID).ToList();
                    if (result != null) {

                        //TODO:Loop to trim address field
                        ViewData["AccountID"] = result.FirstOrDefault().AccountID;
                        ViewData["Company"] = result.FirstOrDefault().Company;
                        ViewData["Address"] = result.FirstOrDefault().RetailerAddress;
                        ViewData["Retailer"] = result.FirstOrDefault().RetailerName;
                        ViewData["UserRole"] = ac.GetUserData()["UserRole"];
                        ViewData["UserName"] = ac.GetUserData()["UserName"];
                    }
                    else
                    {
                        result = new List<DistributionData>();
                    }


                    return View(result);

                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "error";
                Util.LogError(ex);
                return RedirectToAction("Index","Retailer");
            }
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> UpdateReturns(FormCollection frm)
        {

            try
            {
                // Access form data using the collection parameter
                string accountId = frm["accountId"];
                string returnAmount = frm["returnAmount"];
                string drawAmount = frm["drawAmount"];
                string confirmAmount = frm["confirmAmount"];
                string publicationDate = frm["publicationDate"];
                string loggedEmail = frm["loggedEmail"];
                string userRole = frm["userRole"];
                int returnCount = 0;
                string retStatus = "Open";
                DateTime parsedPublicationDate;

                if (DateTime.TryParse(publicationDate, out parsedPublicationDate))
                {
                    using (var cxt = new ApplicationDbContext())
                    {
                        var pubEntry = cxt.CircProTranx.FirstOrDefault(a => a.AccountID == accountId && a.PublicationDate == parsedPublicationDate);
                        if (pubEntry != null)
                        {
                            cxt.Entry(pubEntry).State = System.Data.Entity.EntityState.Modified;

                            returnCount = Convert.ToInt32(returnAmount);

                            pubEntry.ReturnAmount = returnCount;
                            pubEntry.Status = retStatus;
                            pubEntry.ReturnDate = DateTime.Now;
                            pubEntry.UpdatedAt = DateTime.Now;

                            if (userRole != "Retailer")
                            {
                                returnCount = Convert.ToInt32(confirmAmount);

                                retStatus = "Closed";

                                pubEntry.ConfirmedAmount = returnCount;
                                pubEntry.ConfirmDate = DateTime.Now;
                                pubEntry.UpdatedAt = DateTime.Now;
                                pubEntry.Status = retStatus;
                                pubEntry.ConfirmReturn = true;
                            }
                            await cxt.SaveChangesAsync();

                            // Update Activity Logs

                            QRSActivityLog qRSActivityLog = new QRSActivityLog 
                            {
                                AccountID = accountId,
                                LogInformation = "",
                                UserName = cxt.Users.FirstOrDefault(x => x.Email == loggedEmail).FullName,
                                EmailAddress = loggedEmail,
                                PublicationDate = parsedPublicationDate,
                                DistributionAmount = Convert.ToInt32(drawAmount),
                                ReturnAmount = returnCount,
                                Status = retStatus
                            };

                            Util.LogUserActivity(qRSActivityLog);

                            return Json(new { success = true });

                        }
                        else
                        {
                            return Json(new { success = false });
                        }
                    }
                }

                return Json(new { success = false });
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return Json(new { success = false });
            }
           
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetLatest(string id)
        {
            try
            {
                var startDate = GetLastDistDate(id).ToString("yyyy-MM-dd");
                var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                AccountController account = new AccountController();
                account.InitializeController(this.Request.RequestContext);

                return Json(new { success = await account.LoadTransactions(id, startDate, endDate)});
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return Json(new { success = false });
            }
        }

        // GET: Distribution/Create
        public ActionResult Create()
    {
        return View();
    }

        // POST: Distribution/Create
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

        // GET: Distribution/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Distribution/Edit/5
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

        // GET: Distribution/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Distribution/Delete/5
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

        [NonAction]
        public bool IsInitLoad(string accountID)
        {

            try
            {
                using (ApplicationDbContext dc = new ApplicationDbContext())
                {
                    var v = dc.CircProTranx.Where(a => a.AccountID == accountID).FirstOrDefault();
                    return v != null;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [NonAction]
        public DateTime GetLastDistDate(string accountID)
        {

            try
            {
                using (ApplicationDbContext dc = new ApplicationDbContext())
                {
                    var v = dc.CircProTranx.Where(a => a.AccountID == accountID).OrderByDescending(x => x.PublicationDate).FirstOrDefault().PublicationDate;
                    return v.AddDays(1);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
