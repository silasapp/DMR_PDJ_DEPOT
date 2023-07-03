//using System.IO;
//using System.Reflection;
//using iTextSharp.text;
//using iTextSharp.text.pdf;
//using NewDepot.Models;
//using System;
//using System.Linq;
//using NMDPRA_Depot.Domain.Abstract;
//using NewDepot.Models;
//using System.Web;
//using NewDepot.Helpers;
//using System.Configuration;

//namespace NewDepot.Helpers
//{
//    public class PermitHelper
//    {
//        IPermitRepository _permit;
//        IApplicationRepository _appRep;
//        ICompanyRepository _compRep;
//        IapplicationsRepository _vAppRep;
//        IMessageRepository _msgRep;
//        ELPSAPIHelper elpsHelper;

//        public PermitHelper(IPermitRepository permit, IApplicationRepository appRep, ICompanyRepository compRep,
//            IapplicationsRepository vAppRep, IMessageRepository msgRep)
//        {
//            _permit = permit;
//            _appRep = appRep;
//            _compRep = compRep;
//            _vAppRep = vAppRep;
//            _msgRep = msgRep;

//            elpsHelper = new ELPSAPIHelper();
//        }

//        public string CreatePermit(applications app, Facility fac, string r = "")
//        {
//            try
//            {
//                #region Create The Permit
//                var useNewPermitNo = ConfigurationManager.AppSettings["NewLicenseValue"];
//                string pm = "";
//                var elps_Permit = new PermitAPIModel();
//                string letterTemplate = "";
//                var tenure = 0;

//                Permit permit = new Permit();
//                permit.Application_Id = app.Id;
//                permit.Company_Id = app.Company_Id;
//                permit.Date_Issued = UtilityHelper.CurrentTime;
//                bool TOfrmATC = false;
//                if (app.PhaseName.ToLower() == "take over" && app.PaymentDescription.Contains("From ATC"))
//                {
//                    TOfrmATC = true;
//                }
//                if (app.PhaseName.ToLower() == "Suitability Inspection".ToLower())
//                {
//                    letterTemplate = "SuitabilityLetter.txt";
//                    tenure = Convert.ToInt16(ConfigurationManager.AppSettings["suitabilityExpiry"]);
//                    permit.Date_Expire = UtilityHelper.CurrentTime.AddDays(5000);
//                }
//                else if (app.PhaseName.ToLower() == "Approval To Construct".ToLower())
//                {
//                    letterTemplate = "ATCLetter.txt";
//                    tenure = Convert.ToInt16(ConfigurationManager.AppSettings["atcExpiry"]);
//                    permit.Date_Expire = UtilityHelper.CurrentTime.AddDays(tenure);
//                }
//                else if (app.PhaseName.ToLower() == "license to operate" || app.PhaseName.ToLower() == "take over")
//                {
//                    letterTemplate = "NDTs.txt";
//                    //var currentMonth = UtilityHelper.CurrentTime.Month;
//                    //var rem = 12 - currentMonth;
//                    permit.Date_Expire = new DateTime(app.Date_Added.Year, 12, 31);// DateTime.Now.AddYears(1); // DateTime.Parse("12/31/" + DateTime.Now.Year);
//                    int noOfYr = 0;
//                    //int dt = DateTime.Now.Year;
//                    if (!string.IsNullOrEmpty(app.Current_Permit))
//                    {
//                        var p = app.Current_Permit.Split('/');
//                        if (p[0] == "NMDPRA")
//                        {
//                            //Platform Permit, Lets look for it and get the Year issued
//                            //NMDPRA/PDJ/18/N3080
//                            var x = app.Year.ToString().Substring(0, 2) + p[p.Length - 2];
//                            int yr = 0;
//                            if (int.TryParse(x, out yr))
//                            {
//                                noOfYr = app.Year - yr;
//                            }
//                        }
//                        else
//                        {
//                            int yr = 0;
//                            if (int.TryParse(p[p.Length - 1], out yr))
//                            {
//                                noOfYr = app.Year - yr;
//                            }
//                        }
//                        //DEP00085/2017
//                    }


//                    if (noOfYr == 0)
//                    {
//                        permit.Date_Expire = permit.Date_Expire.AddYears(1);
//                    }
//                    if (permit.Date_Expire.Year - DateTime.Now.Year < 0)
//                    {
//                        permit.Date_Expire = new DateTime(DateTime.Now.Year, 12, 31).AddYears(5).AddHours(-3);
//                    }
//                }
//                else if (app.PhaseName.ToLower() == "Calibration/Integrity Tests(NDTs)".ToLower())
//                {
//                    letterTemplate = "NDTs.txt";
//                    //var currentMonth = UtilityHelper.CurrentTime.Month;
//                    //var rem = 12 - currentMonth;
//                    permit.Date_Expire = new DateTime(app.Date_Added.Year, 1, 1).AddYears(5).AddHours(-3);// DateTime.Now.AddYears(1); // DateTime.Parse("12/31/" + DateTime.Now.Year);
//                }
//                else
//                {
//                    letterTemplate = "NDTs.txt";
//                    if (app.PhaseId == 11)
//                    {
//                        permit.Date_Expire = new DateTime(DateTime.Today.Year, 1, 1).AddYears(5).AddHours(-3);
//                    }
//                    else
//                    {
//                        permit.Date_Expire = new DateTime(app.Date_Added.Year, 12, 31); // UtilityHelper.CurrentTime.AddYears(1);
//                        int noOfYr = 0;
//                        //int dt = DateTime.Now.Year;
//                        if (!string.IsNullOrEmpty(app.Current_Permit))
//                        {
//                            var p = app.Current_Permit.Split('/');
//                            if (p[0] == "NMDPRA")
//                            {
//                                //Platform Permit, Lets look for it and get the Year issued
//                                //NMDPRA/PDJ/18/N3080
//                                var x = app.Year.ToString().Substring(0, 2) + p[p.Length - 2];
//                                int yr = 0;
//                                if (int.TryParse(x, out yr))
//                                {
//                                    noOfYr = app.Year - yr;
//                                }
//                            }
//                            else
//                            {
//                                int yr = 0;
//                                if (int.TryParse(p[p.Length - 1], out yr))
//                                {
//                                    noOfYr = app.Year - yr;
//                                }
//                            }
//                            //DEP00085/2017
//                        }


//                        if (noOfYr == 0)
//                        {
//                            permit.Date_Expire = permit.Date_Expire.AddYears(1);
//                        }
//                    }
//                }

//                permit.CategoryName = app.CategoryName + "(" + app.PhaseName + ")";

//                var application = _appRep.FindBy(a => a.Id == app.Id).FirstOrDefault();
//                if (permit.Date_Issued > permit.Date_Expire)
//                {
//                    permit.Date_Expire = new DateTime(permit.Date_Issued.Year, 12, 31);
//                }
//                if (r=="ge")
//                {
//                    if (application.Date_Added.Month>=9)
//                    {
//                        permit.Date_Expire = new DateTime(application.Year + 1, 12, 31).AddHours(23);
//                    }
//                    else
//                    {
//                        permit.Date_Expire = new DateTime(application.Year, 12, 31).AddHours(23);
//                    }
//                }
                
//                //if (!string.IsNullOrEmpty(application.Current_Permit))            Old Method
//                if (!string.IsNullOrEmpty(application.Type) && application.Type.Trim().ToLower() == "renew")
//                {
//                    //Renew Permit
//                    if (useNewPermitNo == "Yes" && permit.Date_Expire> new DateTime(2019, 12, 31).AddDays(1))
//                    {
//                        permit.permit_no = GenerateNewPermitNo(app.Id, "R", fac.CategoryCode, permit.Date_Expire.Year.ToString(), app.PhaseId);

//                    } 
//                    else
//                    {

//                        permit.permit_no = GeneratePermitNo(app.Id, "R", permit.Date_Expire, app.Category_Id, app.PhaseId);
//                    }
//                    elps_Permit.Is_Renewed = "renew";

//                    //Update old permit to Completes status
//                    if (!string.IsNullOrEmpty(application.Current_Permit))
//                    {
//                        var oldPermit = _permit.FindBy(a => a.permit_no.ToLower() == application.Current_Permit.ToLower()).FirstOrDefault();
//                        if (oldPermit != null)
//                        {
//                            oldPermit.Is_Renewed = "Completed";
//                            _permit.Edit(oldPermit);
//                        }
//                    }
//                }
//                else
//                {
//                    //New Permit
//                    if (useNewPermitNo == "Yes" && permit.Date_Expire > new DateTime(2019, 12, 31).AddDays(1))
//                    {
//                        permit.permit_no = GenerateNewPermitNo(app.Id, "N", fac.CategoryCode, permit.Date_Expire.Year.ToString(), app.PhaseId);

//                    }
//                    else
//                    {
//                        permit.permit_no = GeneratePermitNo(app.Id, "N", permit.Date_Expire, app.Category_Id, app.PhaseId);
//                    }
//                    elps_Permit.Is_Renewed = "new";
//                }


//                //Push Permit to ELPS
//                elps_Permit.CategoryName = app.CategoryName;
//                elps_Permit.Company_Id = _compRep.FindBy(c => c.Id == application.Company_Id).FirstOrDefault().Elps_Id.GetValueOrDefault();
//                elps_Permit.Date_Expire = permit.Date_Expire;
//                elps_Permit.Date_Issued = permit.Date_Issued;
//                //elps_Permit.LicenseId = Convert.ToInt32(elpsHelper.ApiKey);
//                elps_Permit.OrderId = application.Reference;
//                elps_Permit.permit_no = permit.permit_no;
//                int elpsPermitId = 0;

//                if (!elpsHelper.PushPermit(elps_Permit, out elpsPermitId))
//                {
//                    throw new ArgumentException("Error Pushing Permit to ELPS!");
//                }

//                permit.Elps_Id = elpsPermitId;
//                _permit.Add(permit);
//                _permit.Save("System", HttpContext.Current.Request.UserHostAddress);
               
//                #endregion
//                pm = permit.permit_no;
//                #region send Mail
//                if (r != "re" && r != "ge")
//                {
//                    var date = UtilityHelper.CurrentTime;
//                    var appl = _vAppRep.FindBy(a => a.Id == app.Id).FirstOrDefault();
//                    var dt = date.Day.ToString() + date.Month.ToString() + date.Year.ToString();
//                    var sn = string.Format("NMDPRA/PDJ/{0}/{1}", dt, appl.Company_Id);
//                    var body = "";
//                    using (var sr = new StreamReader(HttpContext.Current.Server.MapPath(@"\\App_Data\\Templates\") + letterTemplate)) //"PermitApproved.txt"))
//                    {
//                        body = sr.ReadToEnd();
//                    }

//                    var topCaption = "";
//                    string subject = ApprovalSubject(out topCaption, app.PhaseId, TOfrmATC);

//                    var msgBody = "";

//                    if (app.PhaseName.ToLower() == "Approval To Construct".ToLower())
//                    {
//                        msgBody = string.Format(body, subject, app.CompanyName, permit.permit_no, ElapseTime.ShortDate(permit.Date_Issued), ElapseTime.ShortDate(permit.Date_Expire), app.FacilityName + " (" + app.FacilityAddress() + ")", DateTime.Now.Year, tenure + " days");
//                    }
//                    else
//                    {
//                        topCaption += "<br />Details of the Facility is as follow:";
//                        var tenureStr = app.PhaseName.ToLower() == "suitability" ? tenure + " days" : "NA";//not Available Yet
//                        msgBody = string.Format(body, subject, app.CompanyName, permit.permit_no, ElapseTime.ShortDate(permit.Date_Issued), ElapseTime.ShortDate(permit.Date_Expire), app.FacilityName + " (" + app.FacilityAddress() + ")", DateTime.Now.Year, tenureStr, topCaption);
//                    }

//                    //var msgBody = string.Format(body, subject, app.CompanyName, permit.permit_no, permit.Date_Issued, permit.Date_Expire, app.FacilityName, DateTime.Now.Year);
//                    MailHelper.SendEmail(app.user_id, subject, msgBody);

//                    var msg = new NewDepot.Models.messages();
//                    msg.Company_Id = appl.Company_Id;
//                    msg.Content = msgBody;
//                    msg.Date = UtilityHelper.CurrentTime;
//                    msg.Read = 0;
//                    msg.subject = subject;      // "Application Rejected: " + app.Reference;
//                    msg.sender_id = "Application";
//                    _msgRep.Add(msg);
//                    _msgRep.Save("System", HttpContext.Current.Request.UserHostAddress);


//                }

//                #endregion

//                return pm;
//            }
//            catch (Exception x)
//            {
//                UtilityHelper.LogMessages(x.ToString());
//               return x.ToString();// return "Error";
//            }
//        }

//        private string ApprovalSubject(out string note, int phaseId = 0, bool toFrmATC = false)
//        {
//            note = "Congratulations! ";

//            if (phaseId == 1)
//            {
//                note += "A new Suitability Letter has been issued in favour of your Facility.";
//                return "Suitability Letter for New Facility";
//            }
//            else if (phaseId == 2)
//            {
//                note += "A new Depot Approval - \"Approval To Construct (ATC)\" has been Approved & Issued in favour of your Facility.";
//                return "Approval To Construct Approved & Issued";
//            }
//            else if (phaseId == 3)
//            {
//                note += "A new Depot Approval - \"Calibration/Integrity Tests(NDTs)\" has been Approved & Issued in favour of your Facility.";
//                return "Calibration/Integrity Tests(NDTs) Approved & Issued";
//            }
//            else if (phaseId == 5 || phaseId == 9)
//            { //Facility Modification and Modification without Approval
//                note += "A new Depot Approval - \"Depot Modification\" has been Approved & Issued in favour of your Facility.";
//                return "Depot Modification Approved & Issued";
//            }//

//            else if (phaseId == 8)
//            {
//                note += "A new Depot Approval - \"Approval To Construct (ATC)\" has been Approved & Issued in favour of your Facility.";
//                return "Approval To Construct Approved & Issued";
//            }
//            else if (phaseId == 6)
//            {
//                if (toFrmATC)
//                {
//                    note += "A new Depot Approval - \"Approval To Construct (ATC) from Take Over\" has been Approved & Issued in favour of your Facility.";
//                    return "Approval To Construct Approved & Issued (TO)";
//                }
//                else
//                {
//                    note += "A new Depot License - \"Take Over\" for \"License To Operate (LTO)\" has been Approved & Issued in favour of your Facility.";
//                    return "Take Over Approved & Issued for \"License To Operate (LTO)\"";


//                }
//            }
//            else if (phaseId == 11)
//            {
//                note += "A new Depot Approval - \"Calibration/Integrity Tests(NDTs)\" has been Approved & Issued in favour of your Facility.";
//                return "Re-Calibration Approved & Issued";

//            }
//            else if (phaseId == 10)
//            {
//                note += "A new Depot Approval - \"Selling Above Ex-Deport Price\"  Sanction Payment Accepted";
//                return "Selling Above Ex-Deport Price Sanction Payment Accepted";

//            }
//            else
//            {
//                note += "A new Depot License - \"License To Operate (LTO)\" has been Approved & Issued in favour of your Facility.";
//                return "License To Operate Approved & Issued";
//            }
//        }


//        //public bool Process(Application application, application_Processings appProc, string user, string ip)
//        //{
//        //    try
//        //    {
//        //        //if (!string.IsNullOrEmpty(application.Current_Permit))
//        //        if (!string.IsNullOrEmpty(application.Type) && application.Type.Trim().ToLower() == "renew")
//        //        {
//        //            // Renew
//        //            Permit permit = new Permit();
//        //            permit.Application_Id = appProc.ApplicationId;
//        //            permit.Company_Id = appProc.Company_id;
//        //            permit.Date_Issued = UtilityHelper.CurrentTime;
//        //            permit.Date_Expire = UtilityHelper.CurrentTime.AddYears(1);
//        //            permit.permit_no = GeneratePermitNo(appProc.ApplicationId, "R");  // "NMDPRA/ROMS/" +  UtilityHelper.CurrentTime.Year + "/N" + ApplicationId;
//        //            permit.CategoryName = appProc.CategoryName;

//        //            _permit.Add(permit);

//        //            //Update old permit to Completes status
//        //            if (!string.IsNullOrEmpty(application.Current_Permit))
//        //            {
//        //                var oldPermit = _permit.FindBy(a => a.permit_no.ToLower() == application.Current_Permit.ToLower()).FirstOrDefault();
//        //                if (oldPermit != null)
//        //                {
//        //                    oldPermit.Is_Renewed = "Completed";
//        //                    _permit.Edit(oldPermit);
//        //                }
//        //            }
//        //        }
//        //        else
//        //        {
//        //            //New Permit
//        //            Permit permit = new Permit();
//        //            permit.Application_Id = appProc.ApplicationId;
//        //            permit.Company_Id = appProc.Company_id;
//        //            permit.Date_Issued = UtilityHelper.CurrentTime;
//        //            permit.Date_Expire = UtilityHelper.CurrentTime.AddYears(1);
//        //            permit.permit_no = GeneratePermitNo(appProc.ApplicationId, "N");  // "NMDPRA/ROMSP/" +  UtilityHelper.CurrentTime.Year + "/N" + ApplicationId;
//        //            permit.CategoryName = appProc.CategoryName;

//        //            _permit.Add(permit);

//        //        }
//        //        _permit.Save(user, ip);

//        //        return true;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
//        //        return false;
//        //    }
//        //}


//        private string GeneratePermitNo(int appId, string status, DateTime expYear, int catId = 0, int phaseId = 0, bool tofrmATC = false)
//        {
//            string no = "NMDPRA/PDJ/";
//            string touse = string.Empty;

//            Random rnd = new Random();
//        generate:
//            //00001 - 99999
//            int digits = rnd.Next(10001, 99999);

//            //if (catId == 2)
//            //{
//            switch (phaseId)
//            {
//                case 1:
//                    no += "SUI/";
//                    break;
//                case 2:
//                case 8:
//                    no += "ATC/";
//                    break;
//                case 3:
//                case 11:
//                    no += "PLT/";
//                    break;
//                case 4:
//                case 7:
//                    no += "LTO/";
//                    break;
//                case 5:
//                case 9:
//                    no += "MD/";
//                    break;
//                case 6:

//                    no += "TO/";

//                    break;
//                case 10:
//                    no += "SA/";
//                    break;
//                default:
//                    break;
//            }
//            if (string.IsNullOrEmpty(status) && status.ToLower() == "r")
//                no += expYear.Year.ToString().Substring(2, 2) + "/R{0}";
//            else//
//                no += expYear.Year.ToString().Substring(2, 2) + "/N{0}";
//            //no += UtilityHelper.CurrentTime.Year.ToString().Substring(2, 2) + "/N{0}";

//            touse = string.Format(no, digits.ToString("00000"));


//            //}
//            //else
//            //{
//            //    if (catId == 1)
//            //        no += "A/";
//            //    else
//            //        no += "B/";

//            //    if (string.IsNullOrEmpty(status) && status.ToLower() == "r")
//            //        no += UtilityHelper.CurrentTime.Year.ToString().Substring(2, 2) + "/{0}/R{1}";
//            //    else
//            //        no += UtilityHelper.CurrentTime.Year.ToString().Substring(2, 2) + "/{0}/N{1}";

//            //    touse = string.Format(no, digits, appId.ToString("0000"));
//            //}


//            var check = _permit.FindBy(p => p.permit_no.ToLower() == touse.ToLower()).FirstOrDefault();
//            //Check if the NO is not existing
//            if (check == null)
//            {
//                return touse;
//            }
//            else
//                goto generate;
//        }


//        private string GenerateNewPermitNo(int appId, string status, string facCat, string year, int phaseId = 0)
//        {
//            //string no = "NMDPRA/PDJ/";
//            string no = $"NMDPRA/";
//            bool approval = false;
//            string touse = string.Empty;
//            switch (phaseId)
//            {
//                case 1:
//                    no += "SUI";
//                    approval = true;
//                    break;
//                case 2:
//                case 8:
//                    no += "ATC";
//                    approval = true;
//                    break;
//                case 3:
//                case 11:
//                    no += "PLT";
//                    approval = true;
//                    break;
//                case 4:
//                case 6:
//                case 7:
//                    no += "";
//                    break;
//                case 5:
//                case 9:
//                    no += "MD";
//                    approval = true;
//                    break;
//                case 10:
//                    no += "SA";
//                    approval = true;
//                    break;
//                default:
//                    break;
//            }
//            if (!approval)
//            {
//                no += $"{status.ToUpper()}/{facCat.ToUpper()}";
//            }
//            Random rnd = new Random();
//        generate:
//            //00001 - 99999

//            // var lastPermit = _permit.GetAll().OrderByDescending(a=>a.Date_Expire).FirstOrDefault();

//            int digits = rnd.Next(1, 999);
//            no += $"/{digits.ToString("000")}/{year}";
//            touse = no;// string.Format(no, digits.ToString("000"));

//            var check = _permit.FindBy(p => p.permit_no.ToLower() == touse.ToLower()).FirstOrDefault();
//            //Check if the NO is not existing
//            if (check == null)
//            {
//                return touse;
//            }
//            else
//                goto generate;
//        }

//    }
//}