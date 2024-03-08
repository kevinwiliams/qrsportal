using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
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
        private AccountController _ac = new AccountController();

        // GET: Distribution
        public ActionResult Index()
        {
            try
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

                var result = _db.Database.SqlQuery<DistributionData>(sql).ToList();
                //TODO:Loop to trim address field

                return View(result);

                
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
                _ac.InitializeController(this.Request.RequestContext);


                if (!isLoaded) {

                    var startDate = "2024-01-01";
                    var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                    await _ac.LoadTransactions(id, startDate, endDate);
                }


                // Get latest records
                //await GetLatest(id);

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
                                U.CellNumber,
                                T.IsDisputed
                            FROM [dbo].[CircProTransactions] T
                            JOIN [dbo].[CircproUsers] U ON U.AccountID = T.AccountID
                            JOIN [dbo].[CircProAddresses] A ON U.UserID = A.UserID
                            WHERE T.[AccountID] = @AccountID 
                        ORDER BY T.[PublicationDate] DESC";
                var accountID = new SqlParameter("@AccountID", id);
                var result = _db.Database.SqlQuery<DistributionData>(sql, accountID).ToList();
                if (result != null) {

                    //TODO:Loop to trim address field
                    ViewData["AccountID"] = result.FirstOrDefault().AccountID;
                    ViewData["Company"] = result.FirstOrDefault().Company;
                    ViewData["Address"] = result.FirstOrDefault().RetailerAddress;
                    ViewData["Retailer"] = result.FirstOrDefault().RetailerName;
                    ViewData["Email"] = result.FirstOrDefault().EmailAddress;
                    ViewData["Phone"] = result.FirstOrDefault().PhoneNumber + " - " + result.FirstOrDefault().CellNumber;
                    ViewData["UserRole"] = _ac.GetUserData()["UserRole"];
                    ViewData["UserName"] = _ac.GetUserData()["UserName"];
                }
                else
                {
                    result = new List<DistributionData>();
                }


                return View(result);

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
                DistributionData distributionData = new DistributionData();
                string subject = "";
                string body = "";

               

                if (DateTime.TryParse(publicationDate, out parsedPublicationDate))
                {

                    //Load Email Data
                    var result = await (from t in _db.CircProTranx
                                        join u in _db.CircproUsers on t.AccountID equals u.AccountID
                                        join a in _db.CircProAddress on u.UserID equals a.UserID
                                        where t.AccountID == accountId && t.PublicationDate == parsedPublicationDate
                                        orderby t.PublicationDate descending
                                        select new DistributionData
                                        {
                                            AccountID = t.AccountID,
                                            DistributionTypeID = t.DistributionTypeID,
                                            PublicationDate = t.PublicationDate,
                                            DistributionAmount = t.DistributionAmount,
                                            ReturnDate = (DateTime)t.ReturnDate,
                                            ReturnAmount = (int)t.ReturnAmount,
                                            ConfirmDate = (DateTime)t.ConfirmDate,
                                            ConfirmedAmount = (int)t.ConfirmedAmount,
                                            ConfirmReturn = (bool)t.ConfirmReturn,
                                            Status = t.Status,
                                            RetailerName = u.FirstName + " " + u.LastName,
                                            EmailAddress = u.EmailAddress,
                                            Company = u.Company,
                                            RetailerAddress = (a.AddressLine1 ?? "") + ", " + (a.AddressLine2 ?? "") + ", " + (a.CityTown ?? ""),
                                            PhoneNumber = u.PhoneNumber,
                                            CellNumber = u.CellNumber
                                        }).FirstOrDefaultAsync();

                    distributionData = result;

                    var pubEntry = _db.CircProTranx.FirstOrDefault(a => a.AccountID == accountId && a.PublicationDate == parsedPublicationDate);
                    if (pubEntry != null)
                    {
                        _db.Entry(pubEntry).State = System.Data.Entity.EntityState.Modified;

                        returnCount = Convert.ToInt32(returnAmount);

                        pubEntry.ReturnAmount = returnCount;
                        pubEntry.Status = retStatus;
                        pubEntry.ReturnDate = DateTime.Now;
                        pubEntry.UpdatedAt = DateTime.Now;
                        //load email variable
                        distributionData.ReturnAmount = returnCount;
                        subject = "QRS Returns Notification - " + accountId;
                        body = RenderViewToString(this.ControllerContext, "~/Views/Emails/ConfirmReturnRetailer.cshtml", distributionData);


                        if (userRole != "Retailer")
                        {
                            returnCount = Convert.ToInt32(confirmAmount);

                            retStatus = "Closed";

                            pubEntry.ConfirmedAmount = returnCount;
                            pubEntry.ConfirmDate = DateTime.Now;
                            pubEntry.UpdatedAt = DateTime.Now;
                            pubEntry.Status = retStatus;
                            pubEntry.ConfirmReturn = true;
                            //load email variable
                            distributionData.ConfirmedAmount = Convert.ToInt32(confirmAmount);
                            subject = "QRS Returns Closed Confirmation - " + accountId;
                            body = RenderViewToString(this.ControllerContext, "~/Views/Emails/ConfirmReturn.cshtml", distributionData);

                        }
                        //update database
                        await _db.SaveChangesAsync();

                        //set up email
                        bool notifyRetailer = _db.CircproUsers.FirstOrDefault(x => x.AccountID == accountId).NotifyEmail;

                        if (notifyRetailer || userRole != "Retailer")
                        {
                            bool emailSent = await Util.SendMail(User.Identity.Name, subject, body);
                        }

                        // Update Activity Logs

                        QRSActivityLog qRSActivityLog = new QRSActivityLog 
                        {
                            AccountID = accountId,
                            LogInformation = "",
                            UserName = _db.Users.FirstOrDefault(x => x.Email == loggedEmail).FullName,
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
        public async Task<JsonResult> SubmitDispute(FormCollection frm)
        {

            try
            {
                // Access form data using the collection parameter
                string accountId = frm["accountId"];
                string returnAmount = frm["returnAmount"];
                string drawAmount = frm["disputeDrawAmount"];
                string disputeAmount = frm["disputeAmount"];
                string publicationDate = frm["disputePublicationDate"];
                string loggedEmail = frm["loggedEmail"];
                string userRole = frm["userRole"];
                string retailerNote = frm["retailerNote"];
                DateTime parsedPublicationDate;
                
                DistributionData distributionData = new DistributionData();

                if (DateTime.TryParse(publicationDate, out parsedPublicationDate))
                {

                    var pubEntry = _db.CircProTranx.FirstOrDefault(a => a.AccountID == accountId && a.PublicationDate == parsedPublicationDate);
                    if (pubEntry != null) {

                        //Flag entry in the table
                        _db.Entry(pubEntry).State = System.Data.Entity.EntityState.Modified;
                        pubEntry.IsDisputed = true;
                        await _db.SaveChangesAsync();

                        //Load Email Data
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
                            WHERE T.[AccountID] = @AccountID AND T.[PublicationDate] = @PublicationDate
                        ORDER BY T.[PublicationDate] DESC";
                        var accountID = new SqlParameter("@AccountID", accountId);
                        var pubDate = new SqlParameter("@PublicationDate", parsedPublicationDate);
                        var result = await _db.Database.SqlQuery<DistributionData>(sql, accountID, pubDate).FirstOrDefaultAsync();
                        distributionData = result;
                        distributionData.DisputeAmount = Convert.ToInt32(disputeAmount);
                        distributionData.RetailerNote = retailerNote;

                        //set up email
                        string subject = "QRS Draw Dispute - " + accountId;
                        string body = RenderViewToString(this.ControllerContext, "~/Views/Emails/DrawDispute.cshtml", distributionData);
                        bool emailSent = await Util.SendMail(User.Identity.Name, subject, body);

                        // Update Activity Logs
                        //QRSActivityLog qRSActivityLog = new QRSActivityLog
                        //{
                        //    AccountID = accountId,
                        //    LogInformation = "Dispute",
                        //    UserName = _db.Users.FirstOrDefault(x => x.Email == loggedEmail).FullName,
                        //    EmailAddress = loggedEmail,
                        //    PublicationDate = parsedPublicationDate,
                        //    DistributionAmount = Convert.ToInt32(drawAmount),
                        //    ReturnAmount = returnCount,
                        //    Status = retStatus
                        //};

                        //Util.LogUserActivity(qRSActivityLog);

                        if (emailSent)
                            return Json(new { success = true });
                        else
                            return Json(new { success = false });

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
                _ac.InitializeController(this.Request.RequestContext);

                return Json(new { success = await _ac.LoadTransactions(id, startDate, endDate)});
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return Json(new { success = false });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetLatestDraw(string id)
        {
            try
            {
                var startDate = DateTime.Now.AddDays(-60).ToString("yyyy-MM-dd");
                var endDate = DateTime.Now.ToString("yyyy-MM-dd");
                _ac.InitializeController(this.Request.RequestContext);

                return Json(new { success = await _ac.UpdateDistributions(id, startDate, endDate) });
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

        public static string RenderViewToString<TModel>(ControllerContext controllerContext, string viewName, TModel model)
        {
            ViewEngineResult viewEngineResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);
            if (viewEngineResult.View == null)
            {
                throw new Exception("Could not find the View file. Searched locations:\r\n" + viewEngineResult.SearchedLocations);
            }
            else
            {
                IView view = viewEngineResult.View;

                using (var stringWriter = new StringWriter())
                {
                    var viewContext = new ViewContext(controllerContext, view, new ViewDataDictionary<TModel>(model), new TempDataDictionary(), stringWriter);
                    view.Render(viewContext, stringWriter);

                    return stringWriter.ToString();
                }
            }
        }

        [NonAction]
        public bool IsInitLoad(string accountID)
        {

            try
            {
                var v = _db.CircProTranx.Where(a => a.AccountID == accountID).FirstOrDefault();
                return v != null;
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
                var v = _db.CircProTranx.Where(a => a.AccountID == accountID).OrderByDescending(x => x.PublicationDate).FirstOrDefault().PublicationDate;
                return v.AddDays(1);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
