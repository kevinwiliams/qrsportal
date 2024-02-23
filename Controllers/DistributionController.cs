using Newtonsoft.Json;
using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    public class DistributionController : Controller
    {
        // GET: Distribution
        public async Task<ActionResult> Index()
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

        // GET: Distribution/Details/5
        public async Task<ActionResult> Account(string id)
        {
            try
            {
                var isLoaded = IsInitLoad(id);

                if (!isLoaded) {

                    var startDate = "2024-01-01";
                    var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                    AccountController account = new AccountController();
                    account.InitializeController(this.Request.RequestContext);

                    await account.LoadTransactions(id, startDate, endDate);
                }

                using (var cxt = new ApplicationDbContext())
                {

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
                                        COALESCE(TRIM(A.AddressLine1) + ', ', '') + 
                                        COALESCE(TRIM(A.AddressLine2) + ', ', '') + 
                                        COALESCE(TRIM(A.CityTown), ''), 
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
                        ViewData["UserRole"] = (User.IsInRole("Retailer") ? "Retailer" : (User.IsInRole("Circulation") ? "Circulation" : (User.IsInRole("Supervisor") ? "Supervisor" : "Admin")));
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
                string confirmAmount = frm["confirmAmount"];
                string publicationDate = frm["publicationDate"];
                string userRole = frm["userRole"];
                DateTime parsedPublicationDate;

                if (DateTime.TryParse(publicationDate, out parsedPublicationDate))
                {
                    using (var cxt = new ApplicationDbContext())
                    {
                        var pubEntry = cxt.CircProTranx.FirstOrDefault(a => a.AccountID == accountId && a.PublicationDate == parsedPublicationDate);
                        if (pubEntry != null)
                        {
                            cxt.Entry(pubEntry).State = System.Data.Entity.EntityState.Modified;

                            pubEntry.ReturnAmount = Convert.ToInt32(returnAmount);
                            pubEntry.ReturnDate = DateTime.Now;
                            pubEntry.UpdatedAt = DateTime.Now;

                            if (userRole != "Retailer")
                            {
                                pubEntry.ConfirmedAmount = Convert.ToInt32(confirmAmount);
                                pubEntry.ConfirmDate = DateTime.Now;
                                pubEntry.UpdatedAt = DateTime.Now;
                                pubEntry.Status = "Closed";
                                pubEntry.ConfirmReturn = true;
                            }
                            await cxt.SaveChangesAsync();

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
    }
}
