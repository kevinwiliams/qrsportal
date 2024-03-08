using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace QRSPortal2.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private ApplicationDbContext _db = new ApplicationDbContext();

        // GET: Activity
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logs()
        {
            AccountController ac = new AccountController();
            ac.InitializeController(this.Request.RequestContext);

            ViewData["UserRole"] = ac.GetUserData()["UserRole"];
            ViewData["UserName"] = ac.GetUserData()["UserName"];

            List<QRSActivityLog> qRSActivityLogs = new List<QRSActivityLog>();
            qRSActivityLogs = _db.QRSActivityLog.AsNoTracking().OrderByDescending(x => x.CreatedAt).ToList();

            return View(qRSActivityLogs);

        }

        // GET: Activity/Details/5
        public ActionResult History(string id, string pd)
        {
            AccountController ac = new AccountController();
            ac.InitializeController(this.Request.RequestContext);

            DateTime parsedPubDate;
            List<QRSActivityLog> qRSActivityLogs = new List<QRSActivityLog>();

            if (DateTime.TryParse(pd, out parsedPubDate)) 
            {
                qRSActivityLogs = _db.QRSActivityLog.AsNoTracking().Where(x => x.AccountID == id && x.PublicationDate == parsedPubDate).OrderByDescending(x => x.CreatedAt).ToList();
                ViewData["PublicationDate"] = parsedPubDate.ToString("yyyy-MMM-dd");
                ViewData["UserRole"] = ac.GetUserData()["UserRole"];
                ViewData["UserName"] = ac.GetUserData()["UserName"];

                return View(qRSActivityLogs);
            }
            return View(qRSActivityLogs);
        }


        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetLastEntry(FormCollection frm)
        {
            // Access form data using the collection parameter
            string accountId = frm["accountId"];
            string publicationDate = frm["publicationDate"];
            DateTime parsedPublicationDate;
            var response = Json(new { });

            if (DateTime.TryParse(publicationDate, out parsedPublicationDate))
            {

                var result = _db.QRSActivityLog.AsNoTracking().Where(x => x.AccountID == accountId && x.PublicationDate == parsedPublicationDate).OrderByDescending(x => x.CreatedAt).ToList();
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        response = Json(new
                        {
                            LASTUPDATED = result.FirstOrDefault().CreatedAt.ToString("yyyy-MM-dd h:mm tt"),
                            RETAMT = result.FirstOrDefault().ReturnAmount,
                            USER = result.FirstOrDefault().UserName
                        });
                    }
                    
                }

            }


            return response;
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
