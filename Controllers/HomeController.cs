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