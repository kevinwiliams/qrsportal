using QRSPortal2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    public class RetailerController : Controller
    {
        // GET: Retailer
        public ActionResult Index()
        {
            try
            {
                ViewData["Error"] = TempData["Error"];
                ViewData["UserRole"] = (User.IsInRole("Retailer") ? "Retailer" : (User.IsInRole("Circulation") ? "Circulation" : (User.IsInRole("Supervisor") ? "Supervisor" : "Admin")));

                using (var cxt = new ApplicationDbContext())
                {

                    var sql = @"SELECT DISTINCT
                                    U.AccountID, 
                                    U.DistributionID,
                                    CONCAT(U.FirstName, ' ', U.LastName) AS RetailerName,
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
                                FROM [dbo].[CircproUsers] U
                                JOIN [dbo].[CircProAddresses] A ON U.UserID = A.UserID";

                    var result = cxt.Database.SqlQuery<Retailer>(sql).ToList();
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
