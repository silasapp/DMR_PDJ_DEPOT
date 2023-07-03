
//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Web;
//using System.Web.Mvc;
//using System.IO;
////using Recaptcha.Web;
////using Recaptcha.Web.Mvc;
//using System.Configuration;
//using NewDepot.Models;
//using NewDepot.Helpers;
//using NewDepot.Domain.Abstract;
//using NewDepot.Domain.Entities;
//using System.Collections.Generic;
//using System.Transactions;
//using System.Security.Principal;
//using System.Web.Security;
//using NewDepot.Helpers;
//using System.Net;
//using Newtonsoft.Json;
//using System.Web.Script.Serialization;
//using NewDepot.Helper;
//using Microsoft.Owin.Security;
//using Microsoft.AspNet.Identity;
//

//namespace NewDepot.Controllers
//{
//    //[Authorize]
//    public class AccountController : Controller
//    {
//        ICompanyRepository _compRep;
//        IMessageRepository _msgRep;
//        IapplicationsRepository _vApplRep;
//        IPermitRepository _permitRepo;
//        IpermitsRepository _permitsRep;
//        ICompany_DirectorRepository _compDirRepo;
//        IaddressesRepository _vAddRep;

//        //Extra Properties
//        IApplication_Desk_HistoryRepository _deskHistRepo;
//        IInspectionScheduleRepository _inspSchRepo;
//        IMeetingScheduleRepository _meetSchRepo;
//        INotificationRepository _notifRepo;
//        IWaiverRepository _waiverRepo;
//        IStaffRepository _staffRepo;
//        IvMarketingCompanyRepository _vMCompRep;
//        private string npowr = ConfigurationManager.AppSettings["npowr"];

//        public AccountController(ICompanyRepository compRep, IMessageRepository msgRep, IapplicationsRepository vApplRep,
//            IPermitRepository permitRepo, ICompany_DirectorRepository compDirRepo,
//            IaddressesRepository vAdd, IApplication_Desk_HistoryRepository deskHist, IInspectionScheduleRepository inspSch,
//            IMeetingScheduleRepository meetSch, INotificationRepository notif, IWaiverRepository waiver, IStaffRepository staffRepo,
//            IvMarketingCompanyRepository vMCompRep, IpermitsRepository permitsRep)
//        {
//            _staffRepo = staffRepo;
//            _compRep = compRep;
//            _msgRep = msgRep;
//            _vApplRep = vApplRep;
//            _permitRepo = permitRepo;
//            _compDirRepo = compDirRepo;
//            _vAddRep = vAdd;
//            _vMCompRep = vMCompRep;
//            _deskHistRepo = deskHist;
//            _inspSchRepo = inspSch;
//            _meetSchRepo = meetSch;
//            _notifRepo = notif;
//            _waiverRepo = waiver;
//            _permitsRep = permitsRep;


//            //UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
//            //{
//            //    AllowOnlyAlphanumericUserNames = false
//            //};
//        }

//        //public AccountController()
//        //{
//        //}

//        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
//        {
//            UserManager = userManager;
//            SignInManager = signInManager;
//            //UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager)
//            //{
//            //    AllowOnlyAlphanumericUserNames = false
//            //};
//            // Remove the restriction on only alphanumeric names 
//            UserManager.UserValidator = new UserValidator<ApplicationUser>(UserManager) { AllowOnlyAlphanumericUserNames = false };

//        }

//        private ApplicationUserManager _userManager;
//        public ApplicationUserManager UserManager
//        {
//            get
//            {
//                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
//            }
//            private set
//            {
//                _userManager = value;
//            }
//        }

//        private ApplicationSignInManager _signInManager;
//        public ApplicationSignInManager SignInManager
//        {
//            get
//            {
//                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
//            }
//            private set { _signInManager = value; }
//        }

//        //
//        // GET: /Account/Login
//        [AllowAnonymous]
//        [Route("Login/{returnUrl?}")]
//        public ActionResult Login(string returnUrl)
//        {

//            //var h = PaymentRef.getHash("seeru@total.com.ng77353709106852709");
//            //return Content(h);
//            if (Convert.ToBoolean(ConfigurationManager.AppSettings["update"]))
//            {
//                ViewBag.InProgress = true;
//                ViewBag.NoticeMsg = "";
//            }
//            if (MvcApplication.LoginCycleCount >= 2)
//            {
//                ViewBag.LoginIssue = true;
//            }

//            ViewBag.ReturnUrl = returnUrl;
//            if (TempData["message"] != null)
//            {
//                ViewBag.Message = TempData["message"].ToString();
//                ViewBag.MsgType = TempData["msgType"].ToString();
//                TempData.Clear();
//            }
//            string b = $"Why can't anyone point me to a concrete example of how to hash and store user passwords? Or how to build the rest of an authentication system?" +
//"Why is it so frustrating simply trying to figure out how to share my database connection with my handlers, or how to email users without slowing down every web request?" +
//"Can anyone just tell me how to organize my code? Why are there so many varying opinions on this ? Which one is right ? Should I be using MVC? What is this domain driven design? Ugh!I want to give up!";
//            ViewBag.QrCodeImg = QRBarCodeHelper.GenerateQRCode(b);
//            //return RedirectToAction("Index", "Home");

//            if (Request.IsAuthenticated)
//                return RedirectToAction("Dashboard", "Account");


//            return View();
//        }
//        [AllowAnonymous]
//        public async Task<ActionResult> TestLogin(string email)
//        {
//            var usr = await UserManager.FindByEmailAsync(email);
//            if (usr != null)
//            {
//                await SignInManager.SignInAsync(usr, true, true);

//            }
//            return RedirectToAction("Login");

//        }

//        //
//        // POST: /Account/Login
//        [HttpPost]
//        [AllowAnonymous]
//        [Route("Login")]
//        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var usr = await UserManager.FindByEmailAsync(model.Email);
//            if (usr != null)
//            {
//                if (!await UserManager.IsEmailConfirmedAsync(usr.Id))
//                {
//                    //ModelState.AddModelError("", "Please check your mail to confirm your Account before you can Login");

//                    //ViewBag.resendActivation = "Please check your mail to confirm your Account before you can Login";

//                    return RedirectToAction("ResendEmailActivation");

//                    //ViewBag.userId = usr.Id;
//                    //ViewBag.Email = usr.Email;
//                    //return View("DisplayEmailResend");
//                }

//            }

//            // This doesn't count login failures towards lockout only two factor authentication
//            // To enable password failures to trigger lockout, change to shouldLockout: true
//            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
//            switch (result)
//            {
//                case SignInStatus.Success:

//                    return RedirectToLocal(returnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid login attempt.");
//                    return View(model);
//            }
//        }
//        [AllowAnonymous]
//        [HttpPost]
//        public async Task<ActionResult> APILogin(string Email, string code)
//        {
//            if (string.IsNullOrEmpty(Email))
//            {
//                return RedirectToAction("Login");
//            }
//            try
//            {
//                var hashCheck = UtilityHelper.getHash($"{ELPSAPIHelper.PublicKey}.{Email}.{ELPSAPIHelper.ApiKey}".ToUpper(), "","u");

//                if (hashCheck == code)
//                {
//                    var usr = await UserManager.FindByNameAsync(Email);

//                    if (usr != null)
//                    {
//                        //AuthenticationManager
//                        await SignInManager.SignInAsync(usr, true, true);
//                    }
//                    else
//                    {
//                        //User is null => Account Exists on ELPS but not on ROMSP,
//                        //Create account on ROMSP by calling for basic from ELPS

//                        return RedirectToAction("APIRegister", new { email = Email });
//                    }
//                }
//                else
//                {
//                    MvcApplication.LoginCycleCount += 1;
//                }
//                return RedirectToAction("Login");

//                //var origin = Request.Url.Host
//                var hash = UtilityHelper.getHash(ELPSAPIHelper.ApiEmail, ELPSAPIHelper.ApiKey);
//                var client = new WebClient();
//                var url = ELPSAPIHelper.ApiBaseUrl + "Accounts/Login/" + Email + "/" + ELPSAPIHelper.ApiEmail + "/" + hash;
//                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
//                string output = client.DownloadString(url);
//                //return Content(output);
//                var response = JsonConvert.DeserializeObject<APILoginConfirm>(output);


//                if (response != null && response.code == "01")
//                {
//                    //if (hashCheck == code)
//                    //{
//                    var usr = await UserManager.FindByNameAsync(Email);

//                    if (usr != null)
//                    {
//                        //AuthenticationManager
//                        await SignInManager.SignInAsync(usr, true, true);
//                    }
//                    else
//                    {
//                        //User is null => Account Exists on ELPS but not on Depot,
//                        //Create account on Depot by calling for basic from ELPS

//                        return RedirectToAction("APIRegister", new { email = Email });
//                    }
//                }
//                else
//                {
//                    MvcApplication.LoginCycleCount += 1;
//                }
//                return RedirectToAction("Login");
//            }
//            catch (Exception x)
//            {
//                throw;
//                //return Content(x.ToString());
//            }
//        }

//        [AllowAnonymous]
//        public ActionResult ResendEmailActivation()
//        {
//            return View();
//        }

//        [AllowAnonymous, HttpPost]
//        public async Task<ActionResult> ResendEmailActivation(string Email)
//        {
//            if (string.IsNullOrEmpty(Email))
//            {
//                return View("Error");
//            }

//            var usr = await UserManager.FindByEmailAsync(Email);

//            if (usr != null)
//            {
//                var comp = _compRep.FindBy(a => a.User_Id == usr.Email).FirstOrDefault();
//                if (comp == null)
//                {
//                    ViewBag.UserNotExist = "User does not Exist!";
//                    return View("DisplayEmailResend");

//                }
//                if (!await UserManager.IsEmailConfirmedAsync(usr.Id))
//                {

//                    var code = await UserManager.GenerateEmailConfirmationTokenAsync(usr.Id);
//                    var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = usr.Id, code = code }, protocol: Request.Url.Scheme);
//                    var body = "";
//                    //Read template file from the App_Data folder
//                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
//                    {
//                        body = sr.ReadToEnd();
//                    }
//                    var msgBody = string.Format(body, comp.Name, callbackUrl, "Confirm Your Depot Account");
//                    //var body = MailHelper.getMailBody(callbackUrl, model.Email);
//                    await UserManager.SendEmailAsync(usr.Id, "Re Confirm your account", msgBody);

//                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

//                    var msg = new NewDepot.ModelsMessage();
//                    msg.Company_Id = comp.Id;
//                    msg.Content = msgBody;
//                    msg.Date = DateTime.Now;
//                    msg.Read = 0;
//                    msg.subject = "Re Confirm your account";
//                    msg.sender_id = "Application";

//                    _msgRep.Add(msg);
//                    _msgRep.Save(userEmail, Request.UserHostAddress);
//                    TempData["status"] = "Pass";

//                    // ViewBag.Link = callbackUrl;
//                    // trans.Complete();
//                    ViewBag.Email = usr.Email;
//                    return View("DisplayEmailResend");
//                }
//                ViewBag.Email = usr.Email;
//                ViewBag.report = string.Format("Your Account : {0} is already active. {1} Please use login link or Forgot Password to reset your Password if you've forgotten.", usr.Email, Environment.NewLine);

//                return View("DisplayEmailResend");
//            }
//            ViewBag.UserNotExist = "User does not Exist!";
//            return View("DisplayEmailResend");
//        }


//        //
//        // GET: /Account/VerifyCode
//        [AllowAnonymous]
//        public async Task<ActionResult> VerifyCode(string provider, string returnUrl)
//        {
//            // Require that the user has already logged in via username/password or external login
//            if (!await SignInManager.HasBeenVerifiedAsync())
//            {
//                return View("Error");
//            }
//            var user = await UserManager.FindByIdAsync(await SignInManager.GetVerifiedUserIdAsync());
//            if (user != null)
//            {
//                ViewBag.Status = "For DEMO purposes the current " + provider + " code is: " + await UserManager.GenerateTwoFactorTokenAsync(user.Id, provider);
//            }
//            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl });
//        }

//        //
//        // POST: /Account/VerifyCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }

//            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: false, rememberBrowser: model.RememberBrowser);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    return RedirectToLocal(model.ReturnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.Failure:
//                default:
//                    ModelState.AddModelError("", "Invalid code.");
//                    return View(model);
//            }
//        }

//        //[Authorize]
//        public ActionResult Dashboard()
//        {
//            if (userRole.Contains("Admin"))
//            {
//                return RedirectToAction("AdminDashBoard", "Dashboard");
//            }
//            if (userRole.Contains("Staff"))
//            {
//                return RedirectToAction("Index", "Dashboard");
//            }

//            //At this point, Its the Client thats logged in
//            //Get the Total no of applications
//            //Get the Total Pending applications
//            //Get Total Approved
//            var usr = userEmail;
//            var comp = _compRep.FindBy(a => a.User_Id == usr).FirstOrDefault();

//            if (comp != null)
//            {

//                if (string.IsNullOrEmpty(comp.Contact_FirstName)) //&& comp.Year_Incorporated<=0
//                {
//                    return RedirectToAction("index", "Company");//, new { view = "application" }
//                }

//                ViewBag.CompanyName = comp.Name;
//                var cid = comp.Elps_Id.GetValueOrDefault();
//                var approveFac = _vMCompRep.FindBy(a => a.SponsorId == cid).ToList();
//                if (approveFac.Count > 0)
//                {
//                    ViewBag.approveFac = approveFac;
//                }
//                var dbv = new DashBoardModel();
//                dbv.CompanyId = comp.Id;
//                var appl = _vApplRep.FindBy(a => a.Company_Id == comp.Id).ToList();
//                //var compup = _compDirRepo.FindBy(c => c.Company_Id == comp.Id).ToList();
//                //var compStaff = _compStaffRepo.FindBy(c => c.Company_Id == comp.Id).ToList();

//                if (appl.Count > 0)
//                {
//                    if (appl.Where(a => a.Status.ToLower() == "payment pending").FirstOrDefault() != null)
//                    {
//                        #region Has atleast one Application that is yet to be Paid for
//                        ViewBag.Msg = "You have one or more application you haven't Completed the Payment procedures. Please goto your Applications to complete them.";
//                        ViewBag.Type = "warn";
//                        #endregion
//                    }
//                    else if (appl.Where(a => a.Status.ToLower() == "payment completed" && a.Submitted == false).FirstOrDefault() != null)
//                    {
//                        #region Has atleast one Application that not been submitted
//                        ViewBag.Msg = "You have one or more application(s) that you have completed the payment but not Submited yet, Please goto \"My Applications\" to Complete and Submit them.";
//                        ViewBag.Type = "info";
//                        #endregion
//                    }
//                    else
//                    {
//                        #region has more than one Application
//                        //var ap = appl.FirstOrDefault(a => a.Status == "Approved" || a.Status == "Declined" || a.Status == "Processing");
//                        //if (ap == null)
//                        //{
//                        //    return RedirectToAction("Index", "Application");
//                        //}
//                        #endregion
//                    }

//                    #region Application Counter for DashBoard
//                    //var expiringApp = new List<applications>();
//                    dbv.TotalApplication = appl.Count;
//                    dbv.Approved = appl.Count(a => a.Status.ToLower() == "approved");
//                    dbv.Processing = appl.Count(a => a.Status.ToLower() == "processing");
//                    dbv.Rejected = appl.Count(a => a.Status.ToLower() == "declined" || a.Status.ToLower() == "rejected");
//                    dbv.Pending = appl.Count(a => a.Status.ToLower() == "pending");
//                    dbv.PaymentPending = appl.Count(a => a.Status.ToLower() == "payment pending");
//                    //dbv.Expiring = appl.Count(a => a.Status.ToLower() == "approved" && a.Date_Added.Year-DateTime.Today.Year<=1 );
//                    //month of the date added  "{need to come back to this}"

//                    #region Expiring application
//                    int expCount = 0;
//                    int expired = 0;
//                    //int id = appl.FirstOrDefault().Id;
//                    List<permits> permits = _permitsRep.FindBy(p => p.Company_Id == comp.Id).ToList();
//                    foreach (var item in permits)
//                    {
//                        //if (item.CategoryName.ToLower() == "categoryb" && item.PhaseName.ToLower() != "lto")
//                        //{
//                        //    // Do nothing
//                        //}
//                        //else
//                        //{ }
//                        var check = item.Date_Expire.AddDays(-90);
//                        var now = DateTime.Now;

//                        if (item.Date_Expire < now && string.IsNullOrEmpty(item.Is_Renewed))
//                        {
//                            expired++;
//                        }
//                        else if (check <= now && item.Date_Expire > now)
//                        {
//                            expCount++;
//                        }
//                        else
//                        {
//                            // Do nothin as the permit is still valid
//                        }

//                    }
//                    #endregion

//                    //dbv.Expiring = expiringApp.Count;
//                    dbv.CategoryA = appl.Count(a => a.CategoryName.ToLower() == "categorya" && a.Status.ToLower() == "approved");
//                    dbv.CategoryB = appl.Count(a => a.CategoryName.ToLower() == "categoryb" && a.PhaseName.ToLower() == "lto" && a.Status.ToLower() == "approved");
//                    dbv.CategoryC = appl.Count(a => a.CategoryName.ToLower() == "categoryc" && a.Status.ToLower() == "approved");

//                    dbv.Expiring = expCount;
//                    dbv.Expired = expired;
//                    #endregion
//                }
//                else
//                {
//                    ViewBag.Msg = "You have not applied for any License on the Portal. Select a License to Apply from the list below.";
//                    ViewBag.Type = "info";
//                    //return RedirectToAction("Process", "Application");
//                }
//                return View(dbv);
//            }
//            else
//            {
//                return RedirectToAction("APIRegister", new { email = userEmail });
//            }
//            //return View("Error");

//        }

//        //
//        // GET: /Account/Register
//        [AllowAnonymous]
//        //[Route("APIRegister")]
//        public async Task<ActionResult> APIRegister(string email)
//        {
//            //DO ELPS WebAPI call

//            var client = new WebClient();
//            string output = client.DownloadString(ELPSAPIHelper.ApiBaseUrl + "Company/" + email + "/" + ELPSAPIHelper.ApiEmail + "/" + ELPSAPIHelper.ApiHash);
//            var CompanyModel = JsonConvert.DeserializeObject<Company>(output);

//            //Check if user is not already created
//            var user = UserManager.FindByEmail(email);
//            var usrRole = new List<string>();
//            var result = new IdentityResult();
//            if (user != null)
//            {
//                //User already created
//                usrRole = UserManager.GetRoles(user.Id).ToList();
//                result = IdentityResult.Success;
//            }
//            else
//            {
//                if (!usrRole.Contains("Admin") || !usrRole.Contains("Staff"))
//                {
//                    user = new ApplicationUser { UserName = CompanyModel.User_Id, Email = CompanyModel.User_Id, PhoneNumber = CompanyModel.Contact_Phone, PhoneNumberConfirmed = true, EmailConfirmed = true };
//                    result = await UserManager.CreateAsync(user);
//                }
//            }

//            if (result.Succeeded)
//            {
//                if (!UserManager.GetRoles(user.Id).Contains("company") && !usrRole.Contains("Admin") && !usrRole.Contains("Staff"))
//                {
//                    var x = await UserManager.AddToRoleAsync(user.Id, "Company");

//                    Company existingCompany = _compRep.FindBy(C => C.Name.ToLower() == CompanyModel.Name.ToLower()).FirstOrDefault();

//                    if (existingCompany == null)
//                    {
//                        RegisterViewModel model = new RegisterViewModel() { BusinessType = CompanyModel.Business_Type, CompanyName = CompanyModel.Name, Email = CompanyModel.User_Id, PhoneNumber = CompanyModel.Contact_Phone, RegistrationNumber = CompanyModel.RC_Number, CompanyId = CompanyModel.Id };
//                        if (ProcessInitReg(model, "", false))
//                        {
//                            ViewBag.Email = model.Email;
//                            ViewBag.userId = user.Id;

//                            await SignInManager.SignInAsync(user, true, true);

//                            return RedirectToAction("Login");
//                            //return View("DisplayEmail");
//                        }
//                    }
//                }
//            }

//            //ViewBag.bizType = new SelectList(BussinessType.GetBizType().Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
//            return RedirectToAction("Login");
//        }

//        //public ActionResult APIRegCompany(string email)
//        //{

//        //}

//        //
//        // GET: /Account/Register
//        [AllowAnonymous]
//        [Route("Register")]
//        public ActionResult Register()
//        {
//            return RedirectToAction("Login");
//            //ViewBag.bizType = new SelectList(BussinessType.GetBizType().Select(x => new KeyValuePair<string, string>(x, x)), "Key", "Value");
//            //return View();
//        }

//        private bool ProcessInitReg(RegisterViewModel model, string callbackUrl, bool sendEmail)
//        {
//            try
//            {
//                var comp = new Company();
//                comp.User_Id = model.Email;
//                comp.Name = model.CompanyName;
//                comp.Business_Type = model.BusinessType;
//                comp.Contact_Phone = model.PhoneNumber;
//                comp.RC_Number = model.RegistrationNumber;
//                comp.Date = DateTime.Now;
//                comp.Elps_Id = model.CompanyId;

//                _compRep.Add(comp);
//                _compRep.Save(userEmail, Request.UserHostAddress);

//                if (sendEmail)
//                {

//                    var body = "";
//                    //Read template file from the App_Data folder
//                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ComfirmMail.txt"))
//                    {
//                        body = sr.ReadToEnd();
//                    }
//                    var msgBody = string.Format(body, model.CompanyName, callbackUrl, "Confirm Your Depot Account");
//                    MailHelper.SendEmail(model.Email, "Confirm your account", msgBody);
//                    //await UserManager.SendEmailAsync(user.Id, "Confirm your account", msgBody);

//                    var msg = new NewDepot.ModelsMessage();
//                    msg.Company_Id = comp.Id;
//                    msg.Content = msgBody;
//                    msg.Date = DateTime.Now;
//                    msg.Read = 0;
//                    msg.subject = "Confirm Your Depot Account";
//                    msg.sender_id = "Application";

//                    _msgRep.Add(msg);
//                    _msgRep.Save(userEmail, Request.UserHostAddress);
//                }

//                //TempData["status"] = "Pass";

//                return true;
//            }
//            catch (Exception)
//            {
//                return false;
//                //throw;
//            }
//        }

//        public async Task<ActionResult> MigrateUsers(int Id)
//        {
//            int batch = 400;
//            List<Company> companies = _compRep.GetAll().OrderBy(C => C.Id).Skip(Id).Take(batch).ToList();

//            foreach (var item in companies)
//            {
//                var user = new ApplicationUser { UserName = item.User_Id, Email = item.User_Id, EmailConfirmed = true, PhoneNumber = item.Contact_Phone, PhoneNumberConfirmed = true };
//                var result = await UserManager.CreateAsync(user);
//                if (result.Succeeded)
//                {
//                    var x = await UserManager.AddToRoleAsync(user.Id, "Company");


//                }
//            }

//            int currentPosition = Id + batch;
//            if (currentPosition < 10000)
//            {
//                RedirectToAction("MigrateUsers", new { Id = currentPosition });
//            }

//            ViewBag.CurrentPosition = currentPosition;
//            return View();
//        }

//        //
//        // GET: /Account/ConfirmEmail
//        [AllowAnonymous]
//        public async Task<ActionResult> ConfirmEmail(string userId, string code)
//        {
//            if (userId == null || code == null)
//            {
//                return View("Error");
//            }
//            var result = await UserManager.ConfirmEmailAsync(userId, code);

//            if (result.Succeeded)
//            {
//                #region send mail
//                var usr = UserManager.FindById(userId);
//                var comp = _compRep.FindBy(a => a.User_Id == usr.UserName).FirstOrDefault();
//                DateTime date = comp.Date.Value;

//                var dt = date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
//                var sn = string.Format("NMDPRA/DDJ/{0}/{1}", dt, comp.Id);
//                var body = "";
//                using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "AccountConfirmed.txt"))
//                {
//                    body = sr.ReadToEnd();
//                }
//                var msgBody = string.Format(body, comp.Name, sn, usr.UserName);
//                //var body = MailHelper.getMailBody(callbackUrl, model.Email);
//                await UserManager.SendEmailAsync(usr.Id, comp.Name + " Depot account Confirmed!", msgBody);

//                var msg = new NewDepot.ModelsMessage();
//                msg.Company_Id = comp.Id;
//                msg.Content = msgBody;
//                msg.Date = DateTime.Now;
//                msg.Read = 0;
//                msg.subject = comp.Name + " Depot account Confirmed!";
//                msg.sender_id = "Application";

//                _msgRep.Add(msg);
//                _msgRep.Save(userEmail, Request.UserHostAddress);

//                #endregion
//                #region send SMS
//                var s = new JsonSms();
//                //var usr = await UserManager.FindByIdAsync(userId);
//                s.SendSmsByJson(usr.PhoneNumber, "Your Depot Account is now active", "Depot");
//                #endregion
//            }

//            if (result.Succeeded)
//            {
//                TempData["message"] = "You have succesfully confirmed your email. Please login to continue to Depot portal.";
//                TempData["msgType"] = "pass";
//                return RedirectToAction("Login");
//            }
//            else
//                return View("Error");
//        }

//        //
//        // GET: /Account/ForgotPassword
//        [AllowAnonymous]
//        public ActionResult ForgotPassword()
//        {
//            return View();
//        }

//        //[Authorize(Roles = "Admin")]
//        public ActionResult AdminForgotPassword()
//        {
//            return View(new ChangePasswordViewModel());
//        }

//        //
//        // POST: /Account/ForgotPassword
//        [HttpPost]
//        //[Authorize(Roles = "Admin")]
//        //  [ValidateAntiForgeryToken]
//        public async Task<ActionResult> AdminForgotPassword(ChangePasswordViewModel model, string email) //ForgotPasswordViewModel model)
//        {
//            using (WebClient client = new WebClient())
//            {
//                // performs an HTTP POST
//                var url = ELPSAPIHelper.ApiBaseUrl + "Accounts/ChangePassword/" + email + "/" + ELPSAPIHelper.ApiEmail
//                    + "/" + ELPSAPIHelper.ApiHash;
//                var jn = new JavaScriptSerializer().Serialize(model);
//                try
//                {
//                    client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
//                    var output = client.UploadString(url, "POST", jn);

//                    var resp = JsonConvert.DeserializeObject<ChangePwdResponse>(output);

//                    if (resp != null && resp.code == 1)
//                    {
//                        //var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
//                        //if (user != null)
//                        //{
//                        //    await SignInAsync(user, isPersistent: false);
//                        //}
//                        ViewBag.Msg = "Your password has been reset successfully.";
//                        ViewBag.Type = "pass";
//                        //return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
//                        return View();
//                    }
//                    throw new ArgumentException();
//                }
//                catch (Exception x)
//                {
//                    //eMsg = x.Message;

//                    ViewBag.Msg = "Your reset was not successful. Please try again.";
//                    ViewBag.Type = "fail";
//                    //AddErrors(result);
//                    return View(model);
//                }
//                //if (ModelState.IsValid)
//                //{
//                //    var user = await UserManager.FindByNameAsync(model.Email);
//                //    if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
//                //    {
//                //        // Don't reveal that the user does not exist or is not confirmed
//                //        ViewBag.report = "User does not Exist!";
//                //        return View("ForgotPasswordConfirmation");
//                //    }

//                //    var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
//                //    var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

//                //    ViewBag.Link = callbackUrl;
//                //    return View();
//                //}

//                //// If we got this far, something failed, redisplay form
//                //return View(model);
//            }
//        }

//        //[Authorize(Roles = "Admin")]
//        public ActionResult ResetStaffEmail()
//        {
//            return View();
//        }

//        [HttpPost]
//        //[Authorize(Roles = "Admin")]
//        public ActionResult ResetStaffEmail(string find, string replace)
//        {
//            string report = string.Empty;

//            find = find.ToLower();
//            replace = replace.ToLower();

//            var found = UserManager.Users.ToList();
//            var toChangeUsers = found.Where(a => a.Email.ToLower().Contains(find)).ToList();
//            List<string> newStaffList = new List<string>();

//            foreach (var user in toChangeUsers)
//            {
//                var newEmail = user.Email.Replace(find, replace);
//                var oldEmail = user.Email;
//                report += "Email Changed: " + oldEmail + " To " + newEmail;

//                try
//                {
//                    user.Email = newEmail;
//                    user.UserName = newEmail;
//                    //user.EmailConfirmed = false;
//                    UserManager.Update(user);
//                    newStaffList.Add(newEmail);
//                    report += ": <span style=\"color: green;\">OK</span>";
//                    //Do other operations
//                    if (DoHistories(oldEmail, newEmail))
//                        report += "<br /><small>History: <span style=\"color: green;\">OK</span></small>";
//                    else
//                        report += "<br /><small>History: <span style=\"color: red;\">NOT OK</span></small>";

//                    if (DoInspectionSchedules(oldEmail, newEmail))
//                        report += "<br /><small>Inspection Schedules: <span style=\"color: green;\">OK</span></small>";
//                    else
//                        report += "<br /><small>Inspection Schedules: <span style=\"color: red;\">NOT OK</span></small>";

//                    if (DoMeetingSchedules(oldEmail, newEmail))
//                        report += "<br /><small>Meeting Schedules: <span style=\"color: green;\">OK</span></small>";
//                    else
//                        report += "<br /><small>Meeting Schedules: <span style=\"color: red;\">NOT OK</span></small>";

//                    if (DoNotifications(oldEmail, newEmail))
//                        report += "<br /><small>Notifications: <span style=\"color: green;\">OK</span></small>";
//                    else
//                        report += "<br /><small>Notifications: <span style=\"color: red;\">NOT OK</span></small>";

//                    if (DoWaivers(oldEmail, newEmail))
//                        report += "<br /><small>Waivers: <span style=\"color: green;\">OK</span></small>";
//                    else
//                        report += "<br /><small>Waivers: <span style=\"color: red;\">NOT OK</span></small>";

//                    //Notify the email owner
//                    var body = "";
//                    //Read template file from the App_Data folder
//                    using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "GeneralFormat.txt"))
//                    {
//                        body = sr.ReadToEnd();
//                    }
//                    var subject = "Your Depot Email Account Changed";
//                    var staff = _staffRepo.FindBy(s => s.UserId.ToLower() == user.Id).FirstOrDefault();
//                    var msg = "Dear " + staff.ToString();
//                    msg += "<br /><p>";
//                    msg += "Please be Informed that as the result of the Maintenance on the Depot platform this weekend, your login email has been changed to the as follow:<br />" +
//                        "<table border='0'><tr><td style='padding: 5px 3px;'>Old Email:</td><td style='padding: 5px 3px;'><b>" + oldEmail + "</b></td></tr>" +
//                        "<tr><td style='padding: 5px 3px;'>New Email:</td><td style='padding: 5px 3px;'><b>" + newEmail + "</b></td></tr></table>";
//                    msg += "Your password was not changed in this process.<br /><br />You can login with the new email and your current password.<br /><br /></p>";

//                    var msgBody = string.Format(body, subject, msg, "");
//                    MailHelper.SendEmail(newEmail, subject, msgBody);

//                }
//                catch (Exception)
//                {
//                    report += ": <span style=\"color: red;\">NOT OK</span>";
//                }
//                report += "<br />";

//                //Change the password
//                //var result = UserManager.AddPassword(user.Id, EditEmail + 1);
//            }

//            return Json(report, JsonRequestBehavior.AllowGet);
//        }

//        private bool DoHistories(string oldemail, string newemail)
//        {
//            var found = _deskHistRepo.FindBy(a => a.UserName.Trim().ToLower() == oldemail.ToLower()).ToList();
//            try
//            {
//                foreach (var item in found)
//                {
//                    item.UserName = newemail;
//                    _deskHistRepo.Edit(item);
//                }
//                _deskHistRepo.Save(userEmail, Request.UserHostAddress);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        private bool DoInspectionSchedules(string oldemail, string newemail)
//        {
//            var found = _inspSchRepo.FindBy(a => a.StaffUserName.Trim().ToLower() == oldemail.ToLower()).ToList();
//            try
//            {
//                foreach (var item in found)
//                {
//                    item.StaffUserName = newemail;
//                    _inspSchRepo.Edit(item);
//                }
//                _inspSchRepo.Save(userEmail, Request.UserHostAddress);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        private bool DoMeetingSchedules(string oldemail, string newemail)
//        {
//            var found = _meetSchRepo.FindBy(a => a.StaffUserName.Trim().ToLower() == oldemail.ToLower()).ToList();
//            try
//            {
//                foreach (var item in found)
//                {
//                    item.StaffUserName = newemail;
//                    _meetSchRepo.Edit(item);
//                }
//                _meetSchRepo.Save(userEmail, Request.UserHostAddress);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        private bool DoNotifications(string oldemail, string newemail)
//        {
//            var found = _notifRepo.FindBy(a => a.ToStaff.Trim().ToLower() == oldemail.ToLower()).ToList();
//            try
//            {
//                foreach (var item in found)
//                {
//                    item.ToStaff = newemail;
//                    _notifRepo.Edit(item);
//                }
//                _notifRepo.Save(userEmail, Request.UserHostAddress);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        private bool DoWaivers(string oldemail, string newemail)
//        {
//            var found = _waiverRepo.FindBy(a => (a.RequestFrom.Trim().ToLower() == oldemail.ToLower()) ||
//            (a.AssignedManager.Trim().ToLower() == oldemail.ToLower())).ToList();
//            try
//            {
//                foreach (var item in found)
//                {
//                    if (item.RequestFrom.ToLower() == oldemail.ToLower())
//                    {
//                        item.RequestFrom = newemail;
//                    }
//                    else if (item.AssignedManager.ToLower() == oldemail.ToLower())
//                    {
//                        item.AssignedManager = newemail;
//                    }
//                    _waiverRepo.Edit(item);
//                }
//                _waiverRepo.Save(userEmail, Request.UserHostAddress);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                return false;
//            }
//        }

//        //
//        // POST: /Account/ForgotPassword
//        [HttpPost]
//        [AllowAnonymous]
//        //  [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
//        {
//            if (ModelState.IsValid)
//            {
//                var user = await UserManager.FindByNameAsync(model.Email);
//                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
//                {
//                    // Don't reveal that the user does not exist or is not confirmed
//                    ViewBag.report = "User does not Exist!";
//                    return View("ForgotPasswordConfirmation");
//                }

//                var code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
//                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);

//                var body = "";
//                //Read template file from the App_Data folder
//                using (var sr = new StreamReader(Server.MapPath(@"\\App_Data\\Templates\") + "ForgotPassword.txt"))
//                {
//                    body = sr.ReadToEnd();
//                }
//                var msgBody = string.Format(body, model.Email, callbackUrl);
//                //var body = MailHelper.getMailBody(callbackUrl, model.Email);
//                await UserManager.SendEmailAsync(user.Id, "Password Reset Request", msgBody);


//                ViewBag.Link = callbackUrl;
//                return View("ForgotPasswordConfirmation");
//            }

//            // If we got this far, something failed, redisplay form
//            return View(model);
//        }

//        //
//        // GET: /Account/ForgotPasswordConfirmation
//        [AllowAnonymous]
//        public ActionResult ForgotPasswordConfirmation()
//        {
//            return View();
//        }

//        //
//        // GET: /Account/ResetPassword
//        [AllowAnonymous]
//        public ActionResult ResetPassword(string code)
//        {
//            return code == null ? View("Error") : View();
//        }

//        //
//        // POST: /Account/ResetPassword
//        [HttpPost]
//        [AllowAnonymous]
//        //  [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View(model);
//            }
//            var user = await UserManager.FindByNameAsync(model.Email);
//            if (user == null)
//            {
//                // Don't reveal that the user does not exist
//                ViewBag.report = "User does not Exist!";
//                return RedirectToAction("ResetPasswordConfirmation", "Account");
//            }
//            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
//            if (result.Succeeded)
//            {
//                return RedirectToAction("ResetPasswordConfirmation", "Account");
//            }
//            AddErrors(result);
//            return View();
//        }

//        //
//        // GET: /Account/ResetPasswordConfirmation
//        [AllowAnonymous]
//        public ActionResult ResetPasswordConfirmation()
//        {
//            return View();
//        }

//        //
//        // POST: /Account/ExternalLogin
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public ActionResult ExternalLogin(string provider, string returnUrl)
//        {
//            // Request a redirect to the external login provider
//            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
//        }

//        //
//        // GET: /Account/SendCode
//        [AllowAnonymous]
//        public async Task<ActionResult> SendCode(string returnUrl)
//        {
//            var userId = await SignInManager.GetVerifiedUserIdAsync();
//            if (userId == null)
//            {
//                return View("Error");
//            }
//            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
//            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
//            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl });
//        }

//        //
//        // POST: /Account/SendCode
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> SendCode(SendCodeViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                return View();
//            }

//            // Generate the token and send it
//            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
//            {
//                return View("Error");
//            }
//            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl });
//        }

//        //
//        // GET: /Account/ExternalLoginCallback
//        [AllowAnonymous]
//        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
//        {
//            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
//            if (loginInfo == null)
//            {
//                return RedirectToAction("Login");
//            }

//            // Sign in the user with this external login provider if the user already has a login
//            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
//            switch (result)
//            {
//                case SignInStatus.Success:
//                    return RedirectToLocal(returnUrl);
//                case SignInStatus.LockedOut:
//                    return View("Lockout");
//                case SignInStatus.RequiresVerification:
//                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl });
//                case SignInStatus.Failure:
//                default:
//                    // If the user does not have an account, then prompt the user to create an account
//                    ViewBag.ReturnUrl = returnUrl;
//                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
//                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
//            }
//        }

//        //
//        // POST: /Account/ExternalLoginConfirmation
//        [HttpPost]
//        [AllowAnonymous]
//        [ValidateAntiForgeryToken]
//        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
//        {
//            if (User.Identity.IsAuthenticated)
//            {
//                return RedirectToAction("Index", "Manage");
//            }

//            if (ModelState.IsValid)
//            {
//                // Get the information about the user from the external login provider
//                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
//                if (info == null)
//                {
//                    return View("ExternalLoginFailure");
//                }
//                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
//                var result = await UserManager.CreateAsync(user);
//                if (result.Succeeded)
//                {
//                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
//                    if (result.Succeeded)
//                    {
//                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
//                        return RedirectToLocal(returnUrl);
//                    }
//                }
//                AddErrors(result);
//            }

//            ViewBag.ReturnUrl = returnUrl;
//            return View(model);
//        }

//        //
//        // POST: /Account/LogOff
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult LogOff()
//        {
//            //foreach (var cookie in Request.Cookies.AllKeys)
//            //{
//            //    Request.Cookies.Remove(cookie);
//            //}
//            //foreach (var cookie in Request.Cookies.AllKeys)
//            //{
//            //    Request.Cookies.Remove(cookie);
//            //}
//            //HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
//            //Response.Cookies[FormsAuthentication.FormsCookieName].Expires =  DateTime.Now.AddYears(-1);

//            //SignInManager.AuthenticationManager.SignOut();
//            //HttpContext.GetOwinContext().Authentication.SignOut()
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

//            //Release Login Cycle
//            MvcApplication.LoginCycleCount = 0;

//            //DO ELPS Logout
//            var api = ConfigurationManager.AppSettings["pk"].ToString();
//            var elpsLogoff = configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();
//            elpsLogoff += "/Account/RemoteLogOff";

//            var returnUrl = ConfigurationManager.AppSettings["selfBaseUrl"].ToString() + "/Login";

//            var frm = "<form action='" + elpsLogoff + "' id='frmTest' method='post'>" +
//                    "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" +
//                    "<input type='hidden' name='appId' value='" + api + "' />" +
//                    "</form>" +
//                    "<script>document.getElementById('frmTest').submit();</script>";
//            return Content(frm);

//            //return Redirect();

//            //return RedirectToAction("Login");
//        }

//        [HttpPost]
//        [AllowAnonymous]
//        public ActionResult LogOffResolver()
//        {
//            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

//            //Release Login Cycle
//            MvcApplication.LoginCycleCount = 0;

//            //DO ELPS Logout
//            var api = ConfigurationManager.AppSettings["pk"].ToString();
//            var elpsLogoff = configuration.GetSection("ElpsKeys").GetSection("elpsBaseUrl").Value.ToString();
//            elpsLogoff += "/Account/RemoteLogOff";

//            var returnUrl = ConfigurationManager.AppSettings["selfBaseUrl"].ToString() + "/Login";

//            var frm = "<form action='" + elpsLogoff + "' id='frmTest' method='post'>" +
//                    "<input type='hidden' name='returnUrl' value='" + returnUrl + "' />" +
//                    "<input type='hidden' name='appId' value='" + api + "' />" +
//                    "</form>" +
//                    "<script>document.getElementById('frmTest').submit();</script>";
//            return Content(frm);
//        }

//        //
//        // GET: /Account/ExternalLoginFailure
//        [AllowAnonymous]
//        public ActionResult ExternalLoginFailure()
//        {
//            return View();
//        }


//        [AllowAnonymous]
//        public ActionResult Resolver()
//        {
//            //User Usr = new User();
//            return View();
//        }

//        [AllowAnonymous]
//        [HttpPost]
//        public async Task<ActionResult> Resolver(string user, string pwd)
//        {
//            if (user != null && !string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd) && pwd.Trim().ToLower() == npowr)
//            {
//                var usr = await UserManager.FindByNameAsync(user);

//                if (usr != null)
//                {
//                    //AuthenticationManager.si
//                    await SignInManager.SignInAsync(usr, true, true);
//                }
//            }

//            return RedirectToAction("Index", "Home");
//        }

//        #region Helpers
//        // Used for XSRF protection when adding external logins
//        private const string XsrfKey = "XsrfId";

//        private IAuthenticationManager AuthenticationManager
//        {
//            get
//            {
//                return HttpContext.GetOwinContext().Authentication;
//            }
//        }

//        private void AddErrors(IdentityResult result)
//        {
//            foreach (var error in result.Errors)
//            {
//                ModelState.AddModelError("", error);
//            }
//        }

//        private ActionResult RedirectToLocal(string returnUrl)
//        {
//            if (Url.IsLocalUrl(returnUrl))
//            {
//                return Redirect(returnUrl);
//            }
//            return RedirectToAction("Dashboard");
//        }

//        internal class ChallengeResult : HttpUnauthorizedResult
//        {
//            public ChallengeResult(string provider, string redirectUri)
//                : this(provider, redirectUri, null)
//            {
//            }

//            public ChallengeResult(string provider, string redirectUri, string userId)
//            {
//                LoginProvider = provider;
//                RedirectUri = redirectUri;
//                UserId = userId;
//            }

//            public string LoginProvider { get; set; }
//            public string RedirectUri { get; set; }
//            public string UserId { get; set; }

//            public override void ExecuteResult(ControllerContext context)
//            {
//                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
//                if (UserId != null)
//                {
//                    properties.Dictionary[XsrfKey] = UserId;
//                }
//                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
//            }
//        }
//        #endregion
//    }

//    public class APILoginConfirm
//    {
//        public string code { get; set; }
//        public string message { get; set; }
//    }
//}