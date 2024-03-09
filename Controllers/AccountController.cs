using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using QRSPortal2.Models;
using QRSPortal2.ModelsDB;
using static QRSPortal2.Util;

namespace QRSPortal2.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationRoleManager _roleManager;
        private ApplicationDbContext _db;

        public AccountController()
        {
            _db = new ApplicationDbContext();

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            RoleManager = roleManager;

        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Incorrect username/password.");
                    return View(model);
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Require that the user has already logged in via username/password or external login
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes. 
            // If a user enters incorrect codes for a specified amount of time then the user account 
            // will be locked out for a specified amount of time. 
            // You can configure the account lockout settings in IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent:  model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid code.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            //await MigrateUsers();
            List<SelectListItem> roles = new List<SelectListItem>();
            var dbRoles = _db.Roles.ToList();
            foreach (var role in dbRoles)
                roles.Add(new SelectListItem() { Value = role.Name, Text = role.Name });

            ViewBag.Roles = roles;

            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    result = await UserManager.AddToRoleAsync(user.Id, model.RoleName);
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    
                    // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);	
                model.CallBackUrl = callbackUrl;

                string subject = "Reset Password";
                string body = RenderViewToString(this.ControllerContext, "~/Views/Emails/PasswordReset.cshtml", model);
                await UserManager.SendEmailAsync(user.Id, subject, body);

                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }
                AddErrors(result);
                return View();
            }
            catch (HttpAntiForgeryException ex)
            {
                LogError(ex);
                return RedirectToAction("ResetPassword");
            }
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generate the token and send it
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            try
            {
                Session.Clear();
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");
            }
            catch (HttpAntiForgeryException ex)
            {
                LogError(ex);
                Session.Clear();
                AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                return RedirectToAction("Index", "Home");
            }
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
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
        public bool IsEmailExist(string emailAddress)
        {
            try
            {
                // Check if the email address is in a valid format
                if (!IsValidEmail(emailAddress))
                {
                    return false; // Not a valid email address format
                }

                using (ApplicationDbContext dc = new ApplicationDbContext())
                {
                    // Check if the email address exists in the database
                    var v = dc.CircproUsers.FirstOrDefault(a => a.EmailAddress == emailAddress);
                    return v != null; // Return true if the email address exists in the database
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                throw ex;
            }
        }

        // Method to validate email address format
        private bool IsValidEmail(string emailAddress)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailAddress);
                return addr.Address == emailAddress;
            }
            catch
            {
                return false; // Invalid email address format
            }
        }

        public async Task<bool> MigrateUsers()
        {

            try
            {
                using (var context = new ApplicationDbContext())
                {
                    var sql = @"SELECT * FROM [QRS_DB].[dbo].[circproAgents]  WHERE EMAIL IS NOT NULL";
                    var result = await context.Database.SqlQuery<CircProUserList>(sql).ToListAsync();

                    foreach (var item in result)
                    {
                        var isExist = IsEmailExist(item.EMAIL);
                        if (!isExist)
                        {
                            string UserID = "";
                            int addressID = 0;
                            var emailAddress = item.EMAIL;
                            var FullName = (item.FIRST_NAME == "null" ? null : item.FIRST_NAME) + " " + (item.LAST_NAME == "null" ? null : item.LAST_NAME);
                            FullName = (FullName != null) ? FullName : (item.COMPANY == "null" ? null : item.COMPANY);

                            CircproUsers circProUsers = new CircproUsers
                            {
                                AccountID = item.ACCOUNT,
                                //AddressID = item.ADDRESSID,
                                CellNumber = item.CELL_NUMBER == "null" ? null : item.CELL_NUMBER,
                                Company = item.COMPANY == "null" ? null : item.COMPANY,
                                DistributionID = item.DISTRIBUTION_ID,
                                EmailAddress = emailAddress,
                                FirstName = item.FIRST_NAME == "null" ? null : item.FIRST_NAME,
                                LastName = item.LAST_NAME == "null" ? null : item.LAST_NAME,
                                PhoneNumber = item.PHONE_NUMBER == "null" ? null : item.PHONE_NUMBER,
                                CreatedAt = DateTime.Now,
                                IsActive = true
                            };

                            CircProAddress circProAddress = new CircProAddress 
                            { 
                                AccountID = item.ACCOUNT,
                                EmailAddress = emailAddress,
                                AddressLine1 = item.ADDRESS_LINE1 == "null" ? null : item.ADDRESS_LINE1,
                                AddressLine2 = item.ADDRESS_LINE2 == "null" ? null : item.ADDRESS_LINE2,
                                CityTown = item.ADDRESS_LINE3 == "null" ? null : item.ADDRESS_LINE3,
                                CreatedAt = DateTime.Now,

                            };

                            var newAccount = new ApplicationUser 
                            { 
                                UserName = emailAddress,
                                FullName = FullName,
                                Email = emailAddress,
                                CircproUser = circProUsers
                            };
                            //create application user
                            var createAccount = await UserManager.CreateAsync(newAccount, "Password-01!");
                            if (createAccount.Succeeded) 
                            {
                                UserID = newAccount.Id;
                                var userRole = (newAccount.Email.ToLower().Contains("jamaicaobserver.com")) ? "Circulation" : "Retailer";

                                //assign User Role
                                createAccount = await UserManager.AddToRoleAsync(UserID, userRole);
                                circProAddress.UserID = UserID;
                                context.CircProAddress.Add(circProAddress);
                                await context.SaveChangesAsync();

                                //Get Address ID
                                addressID = circProAddress.AddressID;

                                //update subscribers table w/ address ID
                                var circUser = context.CircproUsers.SingleOrDefault(b => b.UserID == UserID);
                                if (circUser != null)
                                {
                                    circUser.AddressID = addressID;
                                    await context.SaveChangesAsync();
                                }

                                AddErrors(createAccount);

                            }

                        }
                    }

                    var sqlTrans = @"SELECT *  FROM [QRS_DB].[dbo].[circProDistro] WHERE [DISTRIBUTION_ID] IN (SELECT [DistributionID] FROM [CircproUsers] c JOIN [AspNetUsers] a on c.[UserID] = a.Id WHERE [EMAIL] IS NOT NULL)";
                    var resultTrans = await context.Database.SqlQuery<CircProDistribution>(sqlTrans).ToListAsync();

                    foreach (var item in resultTrans)
                    {

                        var circUser = context.CircproUsers.SingleOrDefault(b => b.DistributionID == item.DISTRIBUTION_ID);

                        CircProTransactions circProTransactions = new CircProTransactions
                        { 
                            UserID = circUser.UserID,
                            AccountID = item.DIST_ACCTNBR,
                            PublicationDate = item.PUBLISH,
                            ConfirmDate = item.PUBLISH,
                            ConfirmedAmount = item.RETTOT,
                            ConfirmReturn = true,
                            CreatedAt = item.PUBLISH,
                            UpdatedAt = DateTime.Now,
                            Status = "Closed",
                            EmailAddress = circUser.EmailAddress,
                            ReturnAmount = item.RETTOT,
                            ReturnDate = item.PUBLISH,
                            DistributionAmount = item.DRAWTOT,
                            DistributionTypeID = item.DISTRIBUTION_TYPE_ID
                        };

                        context.CircProTranx.Add(circProTransactions);
                        await context.SaveChangesAsync();
                    }
                }
                    return true;

            }
            catch (Exception ex)
            {

                LogError(ex);
                return false;
            }
        }

        public async Task<bool> LoadTransactions(string accountID, string startDate, string endDate)
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Define the URL of the ASP page
                    string url = WebConfigurationManager.AppSettings["circProAPI_load_tranx"]; 

                    // Define your form data as key-value pairs
                    var formData = new Dictionary<string, string>
                    {
                        { "account_id", accountID },
                        { "startDate", startDate },
                        { "endDate", endDate }
                    };

                    // Encode the form data
                    var encodedFormData = new FormUrlEncodedContent(formData);

                    // Set Content-Type header
                    encodedFormData.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    // Send the POST request and get the response
                    HttpResponseMessage response = await client.PostAsync(url, encodedFormData);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        //string responseContent = await response.Content.ReadAsStringAsync();
                        var context = new ApplicationDbContext();
                        var json = await response.Content.ReadAsStringAsync();
                        var resultTrans = JsonConvert.DeserializeObject<List<CircProDistribution>>(json);

                        foreach (var item in resultTrans)
                        {
                            // Assuming circUser is retrieved successfully using DistributionID
                            var circUser = context.CircproUsers.SingleOrDefault(b => b.DistributionID == item.DISTRIBUTION_ID);

                            if (circUser != null)
                            {
                                CircProTransactions circProTransactions = new CircProTransactions
                                {
                                    UserID = circUser.UserID,
                                    AccountID = item.DIST_ACCTNBR,
                                    PublicationDate = item.PUBLISH,
                                    ConfirmDate = item.PUBLISH,
                                    ConfirmedAmount = item.RETTOT,
                                    ConfirmReturn = true,
                                    CreatedAt = item.PUBLISH,
                                    UpdatedAt = DateTime.Now,
                                    Status = "Open",
                                    EmailAddress = circUser.EmailAddress,
                                    ReturnAmount = item.RETTOT,
                                    ReturnDate = item.PUBLISH,
                                    DistributionAmount = item.DRAWTOT,
                                    DistributionTypeID = item.DISTRIBUTION_TYPE_ID
                                };

                                context.CircProTranx.Add(circProTransactions);
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        // If the request was not successful, output the status code
                        Console.WriteLine($"Request failed with status code {response.StatusCode}");
                        return true;
                    }
                }

                return true;

            }
            catch (Exception ex)
            {

                //throw ex;
                LogError(ex);
                return false;
            }
        }

        public async Task<bool> UpdateDistributions(string accountID, string startDate, string endDate)
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Define the URL of the ASP page
                    string url = WebConfigurationManager.AppSettings["circProAPI_load_tranx"]; 

                    // Define your form data as key-value pairs
                    var formData = new Dictionary<string, string>
                    {
                        { "account_id", accountID },
                        { "startDate", startDate },
                        { "endDate", endDate }
                    };

                    // Encode the form data
                    var encodedFormData = new FormUrlEncodedContent(formData);

                    // Set Content-Type header
                    encodedFormData.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    // Send the POST request and get the response
                    HttpResponseMessage response = await client.PostAsync(url, encodedFormData);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read the response content
                        //string responseContent = await response.Content.ReadAsStringAsync();
                        var context = new ApplicationDbContext();
                        var json = await response.Content.ReadAsStringAsync();
                        var resultTrans = JsonConvert.DeserializeObject<List<CircProDistribution>>(json);

                        foreach (var item in resultTrans)
                        {
                            // Assuming circUser is retrieved successfully using DistributionID
                            var circUser = context.CircProTranx.SingleOrDefault(b => b.AccountID == accountID && b.PublicationDate == item.PUBLISH);

                            if (circUser != null)
                            {
                                circUser.DistributionAmount = item.DRAWTOT;
                                await context.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        // If the request was not successful, output the status code
                        Console.WriteLine($"Request failed with status code {response.StatusCode}");
                        return true;
                    }
                }

                return true;

            }
            catch (Exception ex)
            {

                //throw ex;
                LogError(ex);
                return false;
            }
        }

        public async Task<bool> LoadNewRetailers(string emails)
        {

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Define the URL of the ASP page
                    string url = WebConfigurationManager.AppSettings["circProAPI_new_user"]; 

                    // Define your form data as key-value pairs
                    var formData = new Dictionary<string, string>
                    {
                        { "emails", emails }
                    };

                    // Encode the form data
                    var encodedFormData = new FormUrlEncodedContent(formData);

                    // Set Content-Type header
                    encodedFormData.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    // Send the POST request and get the response
                    HttpResponseMessage response = await client.PostAsync(url, encodedFormData);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<List<CircProUserList>>(json);

                        foreach (var item in result)
                        {
                            var isValidEmail = IsValidEmail(item.EMAIL);
                            if (isValidEmail)
                            {
                                var isExist = IsEmailExist(item.EMAIL);
                                if (!isExist)
                                {
                                    string UserID = "";
                                    int addressID = 0;
                                    var emailAddress = item.EMAIL;
                                    var FullName = (item.FIRST_NAME == "null" ? null : item.FIRST_NAME) + " " + (item.LAST_NAME == "null" ? null : item.LAST_NAME);
                                    FullName = (FullName != null) ? FullName : (item.COMPANY == "null" ? null : item.COMPANY);

                                    CircproUsers circProUsers = new CircproUsers
                                    {
                                        AccountID = item.ACCOUNT,
                                        //AddressID = item.ADDRESSID,
                                        CellNumber = item.CELL_NUMBER == "null" ? null : item.CELL_NUMBER,
                                        Company = item.COMPANY == "null" ? null : item.COMPANY,
                                        DistributionID = item.DISTRIBUTION_ID,
                                        EmailAddress = emailAddress,
                                        FirstName = item.FIRST_NAME == "null" ? null : item.FIRST_NAME,
                                        LastName = item.LAST_NAME == "null" ? null : item.LAST_NAME,
                                        PhoneNumber = item.PHONE_NUMBER == "null" ? null : item.PHONE_NUMBER,
                                        CreatedAt = item.DATE_TIME_STAMP,
                                        IsActive = true
                                    };

                                    CircProAddress circProAddress = new CircProAddress
                                    {
                                        AccountID = item.ACCOUNT,
                                        EmailAddress = emailAddress,
                                        AddressLine1 = item.ADDRESS_LINE1 == "null" ? null : item.ADDRESS_LINE1,
                                        AddressLine2 = item.ADDRESS_LINE2 == "null" ? null : item.ADDRESS_LINE2,
                                        CityTown = item.ADDRESS_LINE3 == "null" ? null : item.ADDRESS_LINE3,
                                        CreatedAt = item.DATE_TIME_STAMP,

                                    };

                                    var newAccount = new ApplicationUser
                                    {
                                        UserName = emailAddress,
                                        FullName = FullName,
                                        Email = emailAddress,
                                        CircproUser = circProUsers
                                    };
                                    //create application user
                                    var createAccount = await UserManager.CreateAsync(newAccount, "Password-01!");
                                    if (createAccount.Succeeded)
                                    {
                                        UserID = newAccount.Id;
                                        var userRole = (newAccount.Email.ToLower().Contains("jamaicaobserver.com")) ? "Circulation" : "Retailer";

                                        //assign User Role
                                        createAccount = await UserManager.AddToRoleAsync(UserID, userRole);
                                        circProAddress.UserID = UserID;
                                        _db.CircProAddress.Add(circProAddress);
                                        await _db.SaveChangesAsync();

                                        //Get Address ID
                                        addressID = circProAddress.AddressID;

                                        //update subscribers table w/ address ID
                                        var circUser = _db.CircproUsers.SingleOrDefault(b => b.UserID == UserID);
                                        if (circUser != null)
                                        {
                                            circUser.AddressID = addressID;
                                            await _db.SaveChangesAsync();
                                        }

                                        AddErrors(createAccount);

                                    }

                                }
                            }
                            
                        }
                    }
                    else
                    {
                        // If the request was not successful, output the status code
                        Console.WriteLine($"Request failed with status code {response.StatusCode}");
                        return false;
                    }
                }

                return true;

            }
            catch (Exception ex)
            {

                //throw ex;
                LogError(ex);
                return false;
            }
        }

        public Dictionary<string, string> GetUserData()
        {
            if (Session["userData"] == null)
            {
                // Initialize the dictionary with default values
                Session["userData"] = new Dictionary<string, string>
                {
                    { "UserRole", "" },
                    { "UserName", "" }
                };
            }
            return (Dictionary<string, string>)Session["userData"];
        }

        public string GetAccountId()
        {
            if (Session["accountId"] == null)
            {
                Session["accountId"] = "";
            }
            return (string)Session["accountId"];
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                if (_roleManager != null)
                {
                    _roleManager.Dispose();
                    _roleManager = null;
                }
            }

            base.Dispose(disposing);
        }

        public void InitializeController(RequestContext context)
        {
            base.Initialize(context);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}