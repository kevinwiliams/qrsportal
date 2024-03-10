using QRSPortal2.Models;
using System;
using System.Collections.Generic;
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
                          FROM [QRS_DB].[dbo].[View_Supervisor_Report]";

                var result = _db.Database.SqlQuery<SupervisorReport>(sql).ToList();

                var supervisorList = new List<SelectListItem>();
                var supervisorNames = new SelectList(result.Where(u => u.UserName != null).OrderBy(u => u.UserName).Select(u => u.UserName).Distinct().ToList());

                foreach (var role in supervisorNames)
                {
                    supervisorList.Add(new SelectListItem { Text = role.Text, Value = role.Value });
                }

                ViewBag.Supervisors = supervisorList;

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
