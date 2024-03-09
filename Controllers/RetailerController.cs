using QRSPortal2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    

    [Authorize(Roles = "Admin, Circulation, Supervisor")]
    public class RetailerController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();
        private AccountController _ac = new AccountController();
        // GET: Retailer
        public ActionResult Index()
        {
            AccountController ac = new AccountController();
            ac.InitializeController(this.Request.RequestContext);
            try
            {
                ViewData["Error"] = TempData["Error"];
                ViewData["UserRole"] = ac.GetUserData()["UserRole"];
                ViewData["UserName"] = ac.GetUserData()["UserName"];

                var sql = @"SELECT DISTINCT
                                U.AccountID, 
                                U.DistributionID,
                                CONCAT(U.FirstName, ' ', U.LastName) AS RetailerName,
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
                            FROM [dbo].[CircproUsers] U
                            JOIN [dbo].[CircProAddresses] A ON U.UserID = A.UserID";

                var result = _db.Database.SqlQuery<Retailer>(sql).ToList();
                var retailerCnt = result.Count();

                // Construct a string with delimited emails and single quotes
                //string delimitedEmails = string.Join(",", result.Select(x => $"'{x.EmailAddress}'"));

                // Remove the trailing comma if present
                //delimitedEmails = delimitedEmails.TrimEnd(',');

                // Assign the string to ViewData
                ViewData["CircproUsers"] = retailerCnt;
                //ViewData["AllRetailers"] = delimitedEmails;
                //TODO:Loop to trim address field

                return View(result);
                
            }
            catch (Exception ex)
            {

                Util.LogError(ex);
                return View();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetNewUsers(string emails)
        {
            try
            {
                _ac.InitializeController(this.Request.RequestContext);

                return Json(new { success = await _ac.LoadNewRetailers(emails) });
            }
            catch (Exception ex)
            {
                Util.LogError(ex);
                return Json(new { success = false });
            }
        }

        // GET: Retailer/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Retailer/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Retailer/Create
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

        // GET: Retailer/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Retailer/Edit/5
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

        // GET: Retailer/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Retailer/Delete/5
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
