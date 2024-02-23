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

                if (User.IsInRole("Retailer") || User.IsInRole("Admin"))
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