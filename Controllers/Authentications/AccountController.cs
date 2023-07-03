using LpgLicense.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using NewDepot.Helpers;
//
using NewDepot.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Claims;

namespace NewDepot.Controllers.Authentication
{
    public class AccountController : Controller
    {
        private readonly Depot_DBContext _context;

        RestSharpServices _restService = new RestSharpServices();

        public IConfiguration _configuration;

        ElpsResponse elpsResponse = new ElpsResponse();
        Helpers.Authentications auth = new Helpers.Authentications();
        GeneralClass generalClass = new GeneralClass();
        IHttpContextAccessor _httpContextAccessor;
        HelpersController _helpersController;



        // session
        public const string sessionEmail = "_sessionEmail";
        public const string sessionUserID = "_sessionUserID";
        public const string sessionRoleID = "_sessionRoleID";
        public const string sessionFieldID = "_sessionFieldID";
        public const string sessionLogin = "_sessionLogin";
        public const string sessionElpsID = "_sessionElpsID";
        public const string sessionRoleName = "_sessionRoleName";
        public const string sessionUserName = "_sessionUserName";
        public const string sessionTheme = "_sessionTheme";


        public AccountController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }



        public IActionResult TL(string email, string ciphertext, string key)
        {
            string accessCode = generalClass.Encrypty(ciphertext, key);

            if (accessCode == "1LgUTeieq83zCjen7JNxsA==")
            {

                var paramData = _restService.parameterData("staffEmail", email);
                var response = _restService.Response("/api/Accounts/Staff/{staffEmail}/{email}/{apiHash}", paramData); // GET
                string elpsStaffEmail = "";
                if (response.ErrorException != null)
                {
                    elpsResponse.message = _restService.ErrorResponse(response);
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    var res = JsonConvert.DeserializeObject<LpgLicense.Models.Staff>(response.Content);
                    // checking local staff database for staff

                    if (res == null)
                    {
                        // no staff check for company
                        var paramData2 = _restService.parameterData("compemail", email);
                        var response2 = _restService.Response("/api/company/{compemail}/{email}/{apiHash}", paramData2); // GET

                        if (response2.ErrorException != null)
                        {
                            elpsResponse.message = _restService.ErrorResponse(response2);
                        }
                        else
                        {
                            // checck company
                            var res2 = JsonConvert.DeserializeObject<CompanyDetail>(response2.Content);

                            if (res2 != null)
                            {

                                var _company = (from s in _context.companies where s.elps_id == res2.id select s);

                                if (_company.Count() > 0)
                                {
                                    if (_company.FirstOrDefault().DeleteStatus == true)
                                    {
                                        elpsResponse.message = email + " company has been deleted on this portal, please contact support.";
                                    }
                                    else if (_company.FirstOrDefault().ActiveStatus != true)
                                    {
                                        elpsResponse.message = email + " company has been deactivated on this portal, please contact support.";

                                    }
                                    else if (_company.FirstOrDefault().isFirstTime == true)
                                    {
                                        _helpersController.LogMessages(email + " Needs to accept legal conditions", _company.FirstOrDefault().CompanyEmail.ToString());
                                        return RedirectToAction("LegalStuff", "Companies", new { id = generalClass.Encrypt(_company.FirstOrDefault().id.ToString()) });
                                    }
                                    else
                                    {
                                        elpsResponse.message = email + " Company Found On Local DB.";
                                        elpsResponse.value = _company;

                                        _company.FirstOrDefault().CompanyEmail = email.Trim();
                                        _company.FirstOrDefault().name = res2.name;
                                        _context.SaveChanges();


                                        int roleID = 0; string roleName = "";
                                        var getRoleID = (from u in _context.UserRoles where u.DeleteStatus != true && u.RoleName == "Company" select u).FirstOrDefault();
                                        if (getRoleID != null)
                                            roleID = getRoleID.Role_id; roleName = getRoleID.RoleName;

                                        Logins logins = new Logins()
                                        {
                                            UserID = _company.FirstOrDefault().id,
                                            RoleID = roleID,
                                            HostName = auth.GetHostName(),
                                            MacAddress = auth.GetMACAddress(),
                                            Local_Ip = auth.GetLocal_IpAddress(),
                                            Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                                            UserAgent = Request.Headers["User-Agent"].ToString(),
                                            LoginTime = DateTime.Now,
                                            LoginStatus = "Logged in",
                                        };

                                        _context.Logins.Add(logins);
                                        _context.SaveChanges();


                                        int lastLoginID = logins.LoginID;

                                        // get role name



                                        var identity = new ClaimsIdentity(new[]
                                                   {
                                            new Claim(ClaimTypes.Role, roleName),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                                        var authProperties = new AuthenticationProperties
                                        {
                                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                                        };

                                        var principal = new ClaimsPrincipal(identity);

                                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                                        var getin = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                                        HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(email));
                                        HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_company.FirstOrDefault().id.ToString()));
                                        HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_company.FirstOrDefault().RoleId.ToString()));
                                        HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_company.FirstOrDefault().elps_id.ToString()));
                                        HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));
                                        HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName));
                                        HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_company.FirstOrDefault().name));

                                        return RedirectToAction("Dashboard", "Companies");
                                    }
                                }
                                else
                                {
                                    elpsResponse.message = email + " This company was not found on DEPOT PORTAL.... Creating company now.";

                                    companies companies = new companies()
                                    {
                                        elps_id = res2.id,
                                        user_id = res2.user_Id.Trim(),
                                        name = res2.name.ToUpper(),
                                        CompanyEmail = email,
                                        business_type = res2.business_Type,
                                        CompanyCode = res2.postal_Code,
                                        contact_firstname = res2.contact_FirstName,
                                        contact_lastname = res2.contact_LastName,
                                        contact_phone = res2.contact_Phone,
                                        City = res2.city,
                                        StateName = res2.stateName,
                                        LocationId = 0,
                                        RoleId = _context.UserRoles.Where(x => x.RoleName.Contains("COMPANY")).FirstOrDefault()?.Role_id,
                                        ActiveStatus = true,
                                        CreatedAt = DateTime.Now,
                                        DeleteStatus = false,
                                        isFirstTime = true
                                    };
                                    _context.companies.Add(companies);
                                    int done = _context.SaveChanges();

                                    int ELPSID = Convert.ToInt32(companies.elps_id);

                                    if (done > 0)
                                    {
                                        _helpersController.LogMessages(email + " Company Successfully created. Logging user in", companies.CompanyEmail.ToString());

                                        return RedirectToAction("UserAuth", "Auth", new { email = email });
                                    }
                                    else
                                    {
                                        elpsResponse.message = email + " Something went wrong trying to create your company on DEPOT portal. please try again.";
                                    }
                                }
                            }
                            else
                            {
                                elpsResponse.message = email + " company was not found on ELPS and DEPOT portal....";
                            }
                        }
                    }
                    else
                    {
                        elpsStaffEmail = res.email.Split('@')[0];

                        var _staff = (from s in _context.Staffs where s.Email.Contains(elpsStaffEmail.Trim()) select s);
                        var _staff2 = (from s in _context.Staffs where s.UserId == res.userId select s);

                        if (_staff.Count() > 0)
                        {
                            elpsResponse.message = email + " Staff Found On Local DB.";
                            elpsResponse.value = _staff;
                            _staff.FirstOrDefault().Email = res.email;
                            _context.SaveChanges();
                        }
                    }

                    var stafflog = (from log in _context.Staff
                                    join r in _context.UserRoles on log.RoleID equals r.Role_id
                                    where log.StaffEmail.Contains(elpsStaffEmail.Trim())
                                    select new
                                    {
                                        log.Theme,
                                        //r.RoleName,
                                        log.StaffElpsID,
                                        log.StaffEmail,
                                        log.RoleID,
                                        log.StaffID,
                                        log.FirstName,
                                        log.LastName,
                                        staff_FullN = log.FirstName + " " + log.LastName,
                                        Role = r.RoleName,
                                        log.LocationID,
                                        log.FieldOfficeID
                                    });


                    if (stafflog.ToList().Count <= 0)
                    {

                        //check if it's a company
                        var _company = (from log in _context.companies
                                        join r in _context.UserRoles on log.RoleId equals r.Role_id
                                        where log.CompanyEmail == email
                                        select log);
                        var roleName = _context.UserRoles.Where(x => x.Role_id == _company.FirstOrDefault().RoleId);

                        Logins logins = new Logins()
                        {
                            UserID = _company.FirstOrDefault().id,
                            RoleID = (int)_company.FirstOrDefault().RoleId,
                            HostName = auth.GetHostName(),
                            MacAddress = auth.GetMACAddress(),
                            Local_Ip = auth.GetLocal_IpAddress(),
                            Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            LoginTime = DateTime.Now,
                            LoginStatus = "Logged in",
                        };

                        _context.Logins.Add(logins);
                        _context.SaveChanges();


                        int lastLoginID = logins.LoginID;
                        var identity = new ClaimsIdentity(new[]
                                                            {
                                            new Claim(ClaimTypes.Role, roleName.FirstOrDefault().RoleName),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                        };

                        var principal = new ClaimsPrincipal(identity);

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                        if (_company.Count() > 0)
                        {
                            HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(email));
                            HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_company.FirstOrDefault().id.ToString()));
                            HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_company.FirstOrDefault().RoleId.ToString()));
                            HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_company.FirstOrDefault().elps_id.ToString()));
                            HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName.FirstOrDefault().RoleName));
                            HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_company.FirstOrDefault().name));
                            HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));

                            return RedirectToAction("Dashboard", "Companies");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Logins logins = new Logins()
                        {
                            UserID = stafflog.FirstOrDefault().StaffID,
                            RoleID = (int)stafflog.FirstOrDefault().RoleID,
                            HostName = auth.GetHostName(),
                            MacAddress = auth.GetMACAddress(),
                            Local_Ip = auth.GetLocal_IpAddress(),
                            Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                            UserAgent = Request.Headers["User-Agent"].ToString(),
                            LoginTime = DateTime.Now,
                            LoginStatus = "Logged in",
                        };

                        _context.Logins.Add(logins);
                        _context.SaveChanges();

                        int lastLoginID = logins.LoginID;
                        var identity = new ClaimsIdentity(new[]
                                                            {
                                            new Claim(ClaimTypes.Role, stafflog.FirstOrDefault().Role.ToString()),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                        };

                        var principal = new ClaimsPrincipal(identity);

                        HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                        HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(stafflog.FirstOrDefault().StaffEmail.ToString()));
                        HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(stafflog.FirstOrDefault().StaffID.ToString()));
                        HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(stafflog.FirstOrDefault().RoleID.ToString()));
                        HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(stafflog.FirstOrDefault().StaffElpsID.ToString()));
                        HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(stafflog.FirstOrDefault().Role.ToString()));
                        HttpContext.Session.SetString(sessionFieldID, generalClass.Encrypt(stafflog.FirstOrDefault().FieldOfficeID.ToString()));
                        HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));

                        HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(stafflog.FirstOrDefault().FirstName));
                        HttpContext.Session.SetString(sessionTheme, generalClass.Encrypt(stafflog.FirstOrDefault().Theme));

                        return RedirectToAction("Dashboard", "Staffs");

                    }

                }
            }
            return Json("Ooops! Sorry, you do not have access to this page");

        }
        public IActionResult TLl(string email, string ciphertext, string key)
        {
            string accessCode = generalClass.Encrypty(ciphertext, key);

            if (accessCode == "1LgUTeieq83zCjen7JNxsA==")
            {
                var _company = (from s in _context.companies where s.CompanyEmail.ToLower() == email.ToLower() select s);

            if (_company.Count() > 0)
            {
                if (_company.FirstOrDefault().DeleteStatus == true)
                {
                    elpsResponse.message = email + " company has been deleted on this portal, please contact support.";
                }
                else if (_company.FirstOrDefault().ActiveStatus != true)
                {
                    elpsResponse.message = email + " company has been deactivated on this portal, please contact support.";

                }
                else if (_company.FirstOrDefault().isFirstTime == true)
                {
                    _helpersController.LogMessages(email + " Needs to accept legal conditions", _company.FirstOrDefault().CompanyEmail.ToString());
                    return RedirectToAction("LegalStuff", "Companies", new { id = generalClass.Encrypt(_company.FirstOrDefault().id.ToString()) });
                }
                else
                {
                    elpsResponse.message = email + " Company Found On Local DB.";
                    elpsResponse.value = _company;

                    _company.FirstOrDefault().CompanyEmail = email.Trim();
                    _context.SaveChanges();

                    int roleID = 0; string roleName = "";
                    var getRoleID = (from u in _context.UserRoles where u.DeleteStatus != true && u.RoleName == "Company" select u).FirstOrDefault();
                    if (getRoleID != null)
                        roleID = getRoleID.Role_id; roleName = getRoleID.RoleName;

                    Logins logins = new Logins()
                    {
                        UserID = _company.FirstOrDefault().id,
                        RoleID = roleID,
                        HostName = auth.GetHostName(),
                        MacAddress = auth.GetMACAddress(),
                        Local_Ip = auth.GetLocal_IpAddress(),
                        Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        LoginTime = DateTime.Now,
                        LoginStatus = "Logged in",
                    };

                    _context.Logins.Add(logins);
                    _context.SaveChanges();


                    int lastLoginID = logins.LoginID;

                    // get role name



                    var identity = new ClaimsIdentity(new[]
                               {
                                            new Claim(ClaimTypes.Role, roleName),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    };

                    var principal = new ClaimsPrincipal(identity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                    var getin = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

                    HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(email));
                    HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_company.FirstOrDefault().id.ToString()));
                    HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_company.FirstOrDefault().RoleId.ToString()));
                    HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_company.FirstOrDefault().elps_id.ToString()));
                    HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));
                    HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName));
                    HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_company.FirstOrDefault().name));

                    return RedirectToAction("Dashboard", "Companies");
                }
            }
            else
            {

                string elpsStaffEmail = email.Split('@')[0];

                var _staff = (from s in _context.Staffs where s.Email.Contains(elpsStaffEmail.Trim()) select s);

                if (_staff.Count() > 0)
                {
                    elpsResponse.message = email + " Staff Found On Local DB.";
                    elpsResponse.value = _staff;
                    _staff.FirstOrDefault().Email = email;
                    _context.SaveChanges();
                }


                string StaffEmail = email.Split('@')[0];

                var stafflog = (from log in _context.Staff
                                join r in _context.UserRoles on log.RoleID equals r.Role_id
                                where log.StaffEmail.Contains(StaffEmail.Trim())
                                select new
                                {
                                    log.Theme,
                                    //r.RoleName,
                                    log.StaffElpsID,
                                    log.StaffEmail,
                                    log.RoleID,
                                    log.StaffID,
                                    log.FirstName,
                                    log.LastName,
                                    staff_FullN = log.FirstName + " " + log.LastName,
                                    Role = r.RoleName,
                                    log.LocationID,
                                    log.FieldOfficeID
                                });


                if (stafflog.ToList().Count <= 0)
                {

                    //check if it's a company
                    var company = (from log in _context.companies
                                   join r in _context.UserRoles on log.RoleId equals r.Role_id
                                   where log.CompanyEmail == email
                                   select log);
                    var roleName = _context.UserRoles.Where(x => x.Role_id == company.FirstOrDefault().RoleId);

                    Logins logins = new Logins()
                    {
                        UserID = company.FirstOrDefault().id,
                        RoleID = (int)company.FirstOrDefault().RoleId,
                        HostName = auth.GetHostName(),
                        MacAddress = auth.GetMACAddress(),
                        Local_Ip = auth.GetLocal_IpAddress(),
                        Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        LoginTime = DateTime.Now,
                        LoginStatus = "Logged in",
                    };

                    _context.Logins.Add(logins);
                    _context.SaveChanges();


                    int lastLoginID = logins.LoginID;
                    var identity = new ClaimsIdentity(new[]
                                                        {
                                            new Claim(ClaimTypes.Role, roleName.FirstOrDefault().RoleName),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    };

                    var principal = new ClaimsPrincipal(identity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                    if (_company.Count() > 0)
                    {
                        HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(email));
                        HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(_company.FirstOrDefault().id.ToString()));
                        HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(_company.FirstOrDefault().RoleId.ToString()));
                        HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(_company.FirstOrDefault().elps_id.ToString()));
                        HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(roleName.FirstOrDefault().RoleName));
                        HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(_company.FirstOrDefault().name));
                        HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));

                        return RedirectToAction("Dashboard", "Companies");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    Logins logins = new Logins()
                    {
                        UserID = stafflog.FirstOrDefault().StaffID,
                        RoleID = (int)stafflog.FirstOrDefault().RoleID,
                        HostName = auth.GetHostName(),
                        MacAddress = auth.GetMACAddress(),
                        Local_Ip = auth.GetLocal_IpAddress(),
                        Remote_Ip = HttpContext.GetRemote_IpAddress().ToString(),
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        LoginTime = DateTime.Now,
                        LoginStatus = "Logged in",
                    };

                    _context.Logins.Add(logins);
                    _context.SaveChanges();

                    int lastLoginID = logins.LoginID;
                    var identity = new ClaimsIdentity(new[]
                                                        {
                                            new Claim(ClaimTypes.Role, stafflog.FirstOrDefault().Role.ToString()),

                                        }, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(1)
                    };

                    var principal = new ClaimsPrincipal(identity);

                    HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).Wait();

                    HttpContext.Session.SetString(sessionEmail, generalClass.Encrypt(stafflog.FirstOrDefault().StaffEmail.ToString()));
                    HttpContext.Session.SetString(sessionUserID, generalClass.Encrypt(stafflog.FirstOrDefault().StaffID.ToString()));
                    HttpContext.Session.SetString(sessionRoleID, generalClass.Encrypt(stafflog.FirstOrDefault().RoleID.ToString()));
                    HttpContext.Session.SetString(sessionElpsID, generalClass.Encrypt(stafflog.FirstOrDefault().StaffElpsID.ToString()));
                    HttpContext.Session.SetString(sessionRoleName, generalClass.Encrypt(stafflog.FirstOrDefault().Role.ToString()));
                    HttpContext.Session.SetString(sessionFieldID, generalClass.Encrypt(stafflog.FirstOrDefault().FieldOfficeID.ToString()));
                    HttpContext.Session.SetString(sessionLogin, generalClass.Encrypt(lastLoginID.ToString()));

                    HttpContext.Session.SetString(sessionUserName, generalClass.Encrypt(stafflog.FirstOrDefault().FirstName));
                    HttpContext.Session.SetString(sessionTheme, generalClass.Encrypt(stafflog.FirstOrDefault().Theme));

                    return RedirectToAction("Dashboard", "Staffs");

                }

            }
            return RedirectToAction("Index", "Home");
        }
        return Json("Ooops! Sorry, you do not have access to this page");
    }


    public ContentResult Logout()
        {
            string publicKey = _configuration.GetSection("ElpsKeys").GetSection("PK").Value.ToString();
            var elpsLogoff = ElpsServices._elpsBaseUrl + "Account/RemoteLogOff";
            var returnUrl = Url.Action("Index", "Home", null, Request.Scheme);

            var frm = "<form action='" + elpsLogoff + "' id='frmTest' method='post'>" +
                    "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" +
                    "<input type='hidden' name='appId' value='" + publicKey + "' />" +
                    "</form>" +
                    "<script>document.getElementById('frmTest').submit();</script>";

            if (string.IsNullOrWhiteSpace(_helpersController.getSessionLogin().ToString()))
            {
                return Content(frm, "text/html");
            }
            else
            {
                var logins = _context.Logins.Where(x => x.LoginID == _helpersController.getSessionLogin());

                logins.FirstOrDefault().LoginStatus = "Logout";
                logins.FirstOrDefault().LogoutTime = DateTime.Now;
                _context.SaveChanges();

                _helpersController.LogMessages(_helpersController.getSessionEmail() + " logged out successfully...", _helpersController.getSessionEmail());

                HttpContext.Session.Remove(sessionEmail);
                HttpContext.Session.Remove(sessionUserID);
                HttpContext.Session.Remove(sessionRoleID);
                HttpContext.Session.Remove(sessionElpsID);
                HttpContext.Session.Remove(sessionLogin);
                HttpContext.Session.Remove(sessionRoleName);
                HttpContext.Session.Remove(sessionUserName);
                HttpContext.Session.Remove(sessionTheme);

                var log = HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return Content(frm, "text/html");
            }
        }


       
        public IActionResult AccessDenied()
        {
            ViewData["Message"] = "Access Denied:  Sorry, your session is timed out or you do not have the right to view this page.";
            return View();
        }
        
        public IActionResult ExpiredSession()
        {
            ViewData["Message"] = "Access Denied:  Sorry, your session is timed out, kindly login to view this page.";
            return View();
        }
        public IActionResult ChangePassword()
        {
            return View();
        }





        public JsonResult ChangePasswordAction(string OldPassword, string NewPassword, string ConfirmPassword)
        {
            string result = "";

            LpgLicense.Models.ChangePassword changePassword = new ChangePassword()
            {
                oldPassword = OldPassword,
                newPassword = NewPassword,
                confirmPassword = ConfirmPassword
            };

            var paramData = _restService.parameterData("useremail", _helpersController.getSessionEmail());
            var response = _restService.Response("/api/Accounts/ChangePassword/{useremail}/{email}/{apiHash}", paramData, "POST", changePassword); // GET

            if (response.ErrorException != null)
            {
                result = _restService.ErrorResponse(response);
            }
            else
            {
                var res = JsonConvert.DeserializeObject<LpgLicense.Models.ChangePasswordResponse>(response.Content);

                if (res.code == 1)
                {
                    result = "Password Changed";
                }
                else
                {
                    result = "Password not change. " + res.msg;
                }
            }

            _helpersController.LogMessages("Result for user changing password " + result, _helpersController.getSessionEmail());

            return Json(result);

        }
    }
}