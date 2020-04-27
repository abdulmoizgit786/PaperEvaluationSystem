using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;
using Evalin.Models;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace Evalin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
       
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Login()
        {
            ViewData.Add("ActivePage", "Login");
            ViewBag.Request = "";
            return View(); 
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(User user)
        {
            try
            {
                if (user.Username != null && user.Password != null)
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("username", user.Username);
                    DataAccess.addParam("password", user.Password);
                    DataTable dt = DataAccess.daobj("dbo.Authenticate @username,@password", true, true);
                    if (dt.Rows[0][0] != null && !dt.Rows[0][0].Equals(1) && dt.Columns.Count > 1)
                    {
                        Session["Remember"] = true; 
                        
                        if (dt.Rows[0]["type"].Equals("Student"))
                        {
                            Student s = new Student();
                            s.Id = dt.Rows[0]["id"].ToString();
                            s.Password = dt.Rows[0]["password"].ToString();
                            s.Fullname = dt.Rows[0]["full_name"].ToString();
                            s.Username = dt.Rows[0]["username"].ToString();
                            s.type = dt.Rows[0]["type"].ToString();
                            s.Emailaddress = dt.Rows[0]["Email"].ToString();
                            s.ImageLocation = dt.Rows[0]["image"].ToString();
                            Session["CurrentUser"] = s;
                            return RedirectToAction("Dashboard", "Student");

                        }
                        else if (dt.Rows[0]["type"].Equals("Teacher"))
                        {
                            Teacher t = new Teacher();
                            t.Id = dt.Rows[0]["id"].ToString();
                            t.Password = dt.Rows[0]["password"].ToString();
                            t.Fullname = dt.Rows[0]["full_name"].ToString();
                            t.Username = dt.Rows[0]["username"].ToString();
                            t.type = dt.Rows[0]["type"].ToString();
                            t.Emailaddress = dt.Rows[0]["Email"].ToString();
                            t.ImageLocation = dt.Rows[0]["image"].ToString();
                            Session["CurrentUser"] = t;
                            return RedirectToAction("Dashboard", "Teacher");
                        }
                        else
                        { ModelState.AddModelError("Username", "Error While Getting Data Try Again Later"); }
                    }
                    else if (dt.Rows[0][0].Equals(1))
                    { ModelState.AddModelError("Username", "This User is Blocked"); }
                    else
                    { ModelState.AddModelError("Username", "Invalid Username or Password"); }
                }
            }
            catch (Exception e)
            { }
            return View(user);
       }



        [AllowAnonymous]
        [HttpGet]
        public ActionResult Register()
        {
            ViewData.Add("ActivePage", "Register");
            return View("~/Views/Account/Login.cshtml");
        }


        [HttpPost]
        [AllowAnonymous]
        public ActionResult Register(User user)
        {
            var allowedExtensions = new[] {  
            ".Jpg", ".png", ".jpg", ".jpeg"  };

            var allowedemails = new[] { 
            
            "iqra.edu"
            };


            if (user.Emailaddress != null && user.type != null)
            {
                if (user.type.Equals("Teacher") && !(allowedemails.Contains(user.Emailaddress.Split('@')[1])))
                { ModelState.AddModelError("Emailaddress", "Your Email Address Should Belongs To An Organization"); }
            }

            if (user.Image != null)
            {
                if (!allowedExtensions.Contains(Path.GetExtension(user.Image.FileName)))
                {
                    ModelState.AddModelError("Summary", "Invalid Image Extension");
                    goto Image;
                }
            }

            if (ModelState.IsValid && user.Fullname != null && user.Username != null && user.Password != null && user.Emailaddress != null && user.Gender != null && user.Age >= 5 && user.Age <= 80 && user.DOB != null && user.type != null )
            {
                try
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("name", user.Fullname.ToString());
                    DataAccess.addParam("username", user.Username.ToString());
                    DataAccess.addParam("password", user.Password.ToString());
                    DataAccess.addParam("email", user.Emailaddress.ToString());
                    DataAccess.addParam("gender", user.Gender.ToString());
                    DataAccess.addParam("age", user.Age.ToString());
                    DataAccess.addParam("DOB", user.DOB.Date.ToString("dd-MMMM-yyyy"));
                    DataAccess.addParam("type", user.type.ToString());
                    DataAccess.addParam("image",  user.Image != null ? Path.GetExtension(user.Image.FileName):"");
                    DataTable dt = DataAccess.daobj("dbo.Register @name,@username,@password,@email,@gender,@age,@DOB,@type,@image", true, false);
                    if (!dt.Rows[0][0].Equals("An Error Occured") && !dt.Rows[0][0].Equals("This Username Already Exsist Please Choose Another one") && dt.Columns.Count > 1)
                    {
                        Session["Remember"] = true; 
                        try
                        { user.Image.SaveAs(Server.MapPath(dt.Rows[0]["image"].ToString()));}
                        catch (Exception e)
                        { }
                        if (dt.Rows[0]["type"].Equals("Student"))
                        {
                            Student s = new Student();
                            s.Id = dt.Rows[0]["id"].ToString();
                            s.Fullname = dt.Rows[0]["full_name"].ToString();
                            s.Username = dt.Rows[0]["username"].ToString();
                            s.Password = dt.Rows[0]["password"].ToString();
                            s.type = dt.Rows[0]["type"].ToString();
                            s.Emailaddress = dt.Rows[0]["Email"].ToString();
                            s.ImageLocation = dt.Rows[0]["image"].ToString();
                            Session["CurrentUser"] = s;
                            return RedirectToAction("Dashboard", "Student");
                            
                        }
                        else if (dt.Rows[0]["type"].Equals("Teacher"))
                        {
                            Teacher t = new Teacher();
                            t.Id = dt.Rows[0]["id"].ToString();
                            t.Fullname = dt.Rows[0]["full_name"].ToString();
                            t.Username = dt.Rows[0]["username"].ToString();
                            t.Password = dt.Rows[0]["password"].ToString();
                            t.type = dt.Rows[0]["type"].ToString();
                            t.Emailaddress = dt.Rows[0]["Email"].ToString();
                            t.ImageLocation = dt.Rows[0]["image"].ToString();
                            Session["CurrentUser"] = t;
                            return RedirectToAction("Dashboard", "Teacher");
                        }
                        else
                        { ModelState.AddModelError("Summary", "Error While Getting Data Try Again Later"); }
                    }
                    else if (dt.Rows[0][0].Equals("This Username Already Exsist Please Choose Another one"))
                    { ModelState.AddModelError("Username", "Username Already Exsist Please Choose Another One"); }
                    else
                    { ModelState.AddModelError("Summary", "An Error Occured"); }
                
                }
                catch (Exception ex)
                { ModelState.AddModelError("Summary", "Something goes Wrong here Try Again later"); }
            }
            else
            {}
        
            Image:
            ViewData.Add("ActivePage", "Register");
            return View("~/Views/Account/Login.cshtml", user);
        }

        

        [AllowAnonymous]
        [HttpGet]
        public ActionResult logout()
        {
            Session["CurrentUser"] = null;
           return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }

     
    //    // POST: /Account/Disassociate
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Disassociate(string loginProvider, string providerKey)
    //    {
    //        ManageMessageId? message = null;
    //        IdentityResult result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
    //        if (result.Succeeded)
    //        {
    //            message = ManageMessageId.RemoveLoginSuccess;
    //        }
    //        else
    //        {
    //            message = ManageMessageId.Error;
    //        }
    //        return RedirectToAction("Manage", new { Message = message });
    //    }

    //    //
    //    // GET: /Account/Manage
    //    public ActionResult Manage(ManageMessageId? message)
    //    {
    //        ViewBag.StatusMessage =
    //            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
    //            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
    //            : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
    //            : message == ManageMessageId.Error ? "An error has occurred."
    //            : "";
    //        ViewBag.HasLocalPassword = HasPassword();
    //        ViewBag.ReturnUrl = Url.Action("Manage");
    //        return View();
    //    }

    //    //
    //    // POST: /Account/Manage
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> Manage(ManageUserViewModel model)
    //    {
    //        bool hasPassword = HasPassword();
    //        ViewBag.HasLocalPassword = hasPassword;
    //        ViewBag.ReturnUrl = Url.Action("Manage");
    //        if (hasPassword)
    //        {
    //            if (ModelState.IsValid)
    //            {
    //                IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
    //                if (result.Succeeded)
    //                {
    //                    return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
    //                }
    //                else
    //                {
    //                    AddErrors(result);
    //                }
    //            }
    //        }
    //        else
    //        {
    //            // User does not have a password so remove any validation errors caused by a missing OldPassword field
    //            ModelState state = ModelState["OldPassword"];
    //            if (state != null)
    //            {
    //                state.Errors.Clear();
    //            }

    //            if (ModelState.IsValid)
    //            {
    //                IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
    //                if (result.Succeeded)
    //                {
    //                    return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
    //                }
    //                else
    //                {
    //                    AddErrors(result);
    //                }
    //            }
    //        }

    //        // If we got this far, something failed, redisplay form
    //        return View(model);
    //    }

    //    //
    //    // POST: /Account/ExternalLogin
    //    [HttpPost]
    //    [AllowAnonymous]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult ExternalLogin(string provider, string returnUrl)
    //    {
    //        // Request a redirect to the external login provider
    //        return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
    //    }

    //    //
    //    // GET: /Account/ExternalLoginCallback
    //    [AllowAnonymous]
    //    public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
    //    {
    //        var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
    //        if (loginInfo == null)
    //        {
    //            return RedirectToAction("Login");
    //        }

    //        // Sign in the user with this external login provider if the user already has a login
    //        var user = await UserManager.FindAsync(loginInfo.Login);
    //        if (user != null)
    //        {
    //            await SignInAsync(user, isPersistent: false);
    //            return RedirectToLocal(returnUrl);
    //        }
    //        else
    //        {
    //            // If the user does not have an account, then prompt the user to create an account
    //            ViewBag.ReturnUrl = returnUrl;
    //            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
    //            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
    //        }
    //    }

    //    //
    //    // POST: /Account/LinkLogin
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult LinkLogin(string provider)
    //    {
    //        // Request a redirect to the external login provider to link a login for the current user
    //        return new ChallengeResult(provider, Url.Action("LinkLoginCallback", "Account"), User.Identity.GetUserId());
    //    }

    //    //
    //    // GET: /Account/LinkLoginCallback
    //    public async Task<ActionResult> LinkLoginCallback()
    //    {
    //        var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
    //        if (loginInfo == null)
    //        {
    //            return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
    //        }
    //        var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
    //        if (result.Succeeded)
    //        {
    //            return RedirectToAction("Manage");
    //        }
    //        return RedirectToAction("Manage", new { Message = ManageMessageId.Error });
    //    }

    //    //
    //    // POST: /Account/ExternalLoginConfirmation
    //    [HttpPost]
    //    [AllowAnonymous]
    //    [ValidateAntiForgeryToken]
    //    public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
    //    {
    //        if (User.Identity.IsAuthenticated)
    //        {
    //            return RedirectToAction("Manage");
    //        }

    //        if (ModelState.IsValid)
    //        {
    //            // Get the information about the user from the external login provider
    //            var info = await AuthenticationManager.GetExternalLoginInfoAsync();
    //            if (info == null)
    //            {
    //                return View("ExternalLoginFailure");
    //            }
    //            var user = new ApplicationUser() { UserName = model.UserName };
    //            var result = await UserManager.CreateAsync(user);
    //            if (result.Succeeded)
    //            {
    //                result = await UserManager.AddLoginAsync(user.Id, info.Login);
    //                if (result.Succeeded)
    //                {
    //                    await SignInAsync(user, isPersistent: false);
    //                    return RedirectToLocal(returnUrl);
    //                }
    //            }
    //            AddErrors(result);
    //        }

    //        ViewBag.ReturnUrl = returnUrl;
    //        return View(model);
    //    }

    //    //
    //    // POST: /Account/LogOff
    //    [HttpPost]
    //    [ValidateAntiForgeryToken]
    //    public ActionResult LogOff()
    //    {
    //        AuthenticationManager.SignOut();
    //        return RedirectToAction("Index", "Home");
    //    }

    //    //
    //    // GET: /Account/ExternalLoginFailure
    //    [AllowAnonymous]
    //    public ActionResult ExternalLoginFailure()
    //    {
    //        return View();
    //    }

    //    [ChildActionOnly]
    //    public ActionResult RemoveAccountList()
    //    {
    //        var linkedAccounts = UserManager.GetLogins(User.Identity.GetUserId());
    //        ViewBag.ShowRemoveButton = HasPassword() || linkedAccounts.Count > 1;
    //        return (ActionResult)PartialView("_RemoveAccountPartial", linkedAccounts);
    //    }

    //    protected override void Dispose(bool disposing)
    //    {
    //        if (disposing && UserManager != null)
    //        {
    //            UserManager.Dispose();
    //            UserManager = null;
    //        }
    //        base.Dispose(disposing);
    //    }

    //    #region Helpers
    //    // Used for XSRF protection when adding external logins
    //    private const string XsrfKey = "XsrfId";

    //    private IAuthenticationManager AuthenticationManager
    //    {
    //        get
    //        {
    //            return HttpContext.GetOwinContext().Authentication;
    //        }
    //    }

    //    private async Task SignInAsync(ApplicationUser user, bool isPersistent)
    //    {
    //        AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
    //        var identity = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
    //        AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = isPersistent }, identity);
    //    }

    //    private void AddErrors(IdentityResult result)
    //    {
    //        foreach (var error in result.Errors)
    //        {
    //            ModelState.AddModelError("", error);
    //        }
    //    }

    //    private bool HasPassword()
    //    {
    //        var user = UserManager.FindById(User.Identity.GetUserId());
    //        if (user != null)
    //        {
    //            return user.PasswordHash != null;
    //        }
    //        return false;
    //    }

    //    public enum ManageMessageId
    //    {
    //        ChangePasswordSuccess,
    //        SetPasswordSuccess,
    //        RemoveLoginSuccess,
    //        Error
    //    }

    //    private ActionResult RedirectToLocal(string returnUrl)
    //    {
    //        if (Url.IsLocalUrl(returnUrl))
    //        {
    //            return Redirect(returnUrl);
    //        }
    //        else
    //        {
    //            return RedirectToAction("Index", "Home");
    //        }
    //    }

    //    private class ChallengeResult : HttpUnauthorizedResult
    //    {
    //        public ChallengeResult(string provider, string redirectUri) : this(provider, redirectUri, null)
    //        {
    //        }

    //        public ChallengeResult(string provider, string redirectUri, string userId)
    //        {
    //            LoginProvider = provider;
    //            RedirectUri = redirectUri;
    //            UserId = userId;
    //        }

    //        public string LoginProvider { get; set; }
    //        public string RedirectUri { get; set; }
    //        public string UserId { get; set; }

    //        public override void ExecuteResult(ControllerContext context)
    //        {
    //            var properties = new AuthenticationProperties() { RedirectUri = RedirectUri };
    //            if (UserId != null)
    //            {
    //                properties.Dictionary[XsrfKey] = UserId;
    //            }
    //            context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
    //        }
    //    }
    //    #endregion



        public Boolean Authenticate(User user)
        {
            try
            {
                if (user.Username != null && user.Password != null)
                {
                    DataAccess.clearparam();
                    DataAccess.addParam("username", user.Username);
                    DataAccess.addParam("password", user.Password);
                    DataTable dt = DataAccess.daobj("dbo.Authenticate @username,@password", true, true);
                    if (dt.Rows[0][0] != null && !dt.Rows[0][0].Equals(1) && dt.Columns.Count > 1)
                    {
                        return true;
                    }
                    else
                    { throw new HttpRequestValidationException(); }
                }
                else { throw new HttpRequestValidationException();}
            }
            catch (Exception e)
            { return false;}

        }
      
    }

    //Moiz Role based Approach use karo take Student ya Teacher aik doosre ke Controller ko Access na kr saken//

    class MyCustomFilter : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Session["Remember"] != null && filterContext.HttpContext.Session["Remember"].Equals(true) && filterContext.HttpContext.Session["CurrentUser"] != null)
            {
                AccountController ac = new AccountController();
                if (ac.Authenticate((User)filterContext.HttpContext.Session["CurrentUser"]))
                {

                    var RouteData = filterContext.HttpContext.Request.RequestContext.RouteData;
                    string currentController = RouteData.GetRequiredString("controller");
                    if (!currentController.Equals(((User)filterContext.HttpContext.Session["CurrentUser"]).type))
                    { filterContext.Result = new RedirectResult("~/Account/Error");  }
                }
                else
                { filterContext.Result = new RedirectResult("~/Account/Login"); }
           }
            else
            { filterContext.Result = new RedirectResult("~/Account/Login"); }
        }
    }
}