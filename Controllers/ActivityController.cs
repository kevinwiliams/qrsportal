using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    public class ActivityController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Activity
        public ActionResult Index()
        {
            return View();
        }

        // GET: Activity/Details/5
        public ActionResult History(string id, string pd)
        {
            DateTime parsedPubDate;
            List<QRSActivityLog> qRSActivityLogs = new List<QRSActivityLog>();

            if (DateTime.TryParse(pd, out parsedPubDate)) 
            {
                qRSActivityLogs = _db.QRSActivityLog.AsNoTracking().Where(x => x.AccountID == id && x.PublicationDate == parsedPubDate).ToList();
                ViewData["PublicationDate"] = parsedPubDate.ToString("yyyy-MMM-dd");
                return View(qRSActivityLogs);
            }
            return View(qRSActivityLogs);
        }

        // GET: Activity/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Activity/Create
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

        // GET: Activity/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Activity/Edit/5
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

        // GET: Activity/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Activity/Delete/5
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
