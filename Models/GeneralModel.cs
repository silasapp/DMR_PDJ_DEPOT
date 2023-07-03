
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewDepot.Models;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Net.Mail;
//using NewDepot.Controllers;
namespace NewDepot.Models
{

    public class CategoryType
    {
        public int CatTypeId { get; set; }
        public int PhaseTypeId { get; set; }
        public string CategoryName { get; set; }
        public string PhaseName { get; set; }
        public string AppType { get; set; }
        public int Counter { get; set; }
        public int CategorId { get; set; }
    }
    public class LegacyModel
    {
        public companies Company { get; set; }
        public States_UT State { get; set; }
        public Categories Categories { get; set; }
        public Phases Phases { get; set; }
        public Staff Staff { get; set; }
        public Legacies Legacy { get; set; }
        public string Category { get; set; }
        public string CompanyName { get; set; }
        public Staff CurrentDesk { get; set; }
    }
    public class AllProcessingModel
    {
        public string Company { get; set; }
        public string State { get; set; }
        public string PhaseName { get; set; }
        public string Sort { get; set; }
        public string Staff { get; set; }
        public string Date { get; set; }
        public string Processed { get; set; }
        public string date { get; set; }
        public string Category { get; set; }
        public string CurrentProcDesk { get; set; }
        public string CurrentDesk { get; set; }
        public string AppReference { get; set; }
        public List<ProcessingModel> processingModel { get; set; }
        public List<MyDesk> myDesk { get; set; }
    } public class ProcessingModel
    {
        public int oldStaffID { get; set; }
        public int newStaffID { get; set; }
        public string Company { get; set; }
        public string State { get; set; }
        public string PhaseName { get; set; }
        public string Sort { get; set; }
        public string Staff { get; set; }
        public string Date { get; set; }
        public string Processed { get; set; }
        public string date { get; set; }
        public string Category { get; set; }
        public string CurrentProcDesk { get; set; }
        public string CurrentDesk { get; set; }
        public string AppReference { get; set; }
        public List<MyDesk> myDesk { get; set; }
    }
    public class LegacyDocuments
    {
        public int LocalDocID { get; set; }
        public int CompElpsDocID { get; set; }
        public string DocSource { get; set; }
    }
    public class PaymentReportViewModel
    {
        public string mindate { get; set; }
        public string maxdate { get; set; }
        public string type { get; set; }
        public List<string> state { get; set; }
    }

    public class JQueryDataTableParamModel
    {
        /// <summary>
        /// Request sequence number sent by DataTable,
        /// same value must be returned in response
        /// </summary>       
        public string sEcho { get; set; }

        /// <summary>
        /// Text used for filtering
        /// </summary>
        public string sSearch { get; set; }

        /// <summary>
        /// Number of records that should be shown in table
        /// </summary>
        public int iDisplayLength { get; set; }

        /// <summary>
        /// First record that should be shown(used for paging)
        /// </summary>
        public int iDisplayStart { get; set; }

        /// <summary>
        /// Number of columns in table
        /// </summary>
        public int iColumns { get; set; }

        /// <summary>
        /// Number of columns that are used in sorting
        /// </summary>
        public int iSortingCols { get; set; }

        /// <summary>
        /// Comma separated list of column names
        /// </summary>
        public string sColumns { get; set; }
    }
    public class ApplicationExtraPaymentsModel
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalPayment { get; set; }
        public string RRR { get; set; }
        public string Reference { get; set; }
        public string Comment { get; set; }
        public string Comment_TO { get; set; }
        public DateTime? Date { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string DatePaid { get; set; }

        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string CompanyName { get; set; }
        public string FacilityLGA { get; set; }

        public List<ApplicationModel> applications { get; set; }
    }
    public class AppFormApiSubmitModel
    {
        public int ApplicationId { get; set; }
        public string FormTitle { get; set; }
        public int FormId { get; set; }
        public string Date { get; set; }
        public bool Recommend { get; set; }
        public bool Filled { get; set; }

        public string Reasons { get; set; }
        public string ExtraReport1 { get; set; }
        public string ExtraReport2 { get; set; }
        //public string StaffName { get; set; }
        public string InspectorEmail { get; set; }

        public List<FieldAndValue> FieldAndValue { get; set; }

    }

    public class FieldAndValue
    {
        public string FieldName { get; set; }
        public string Value { get; set; }
    }
    public class AppCountModel
    {
        public List<Categories> Categories { get; set; }
        public List<application_services> Services { get; set; }

        public List<ApplicationJobSpecPresentations> Specifications { get; set; }
    }

    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        //    public IList<UserLoginInfo> CurrentLogins { get; set; }
        //    public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        //public ICollection<System.Web.AspNetHostingPermission.Mvc.SelectListItem> Providers { get; set; }
    }

    public class UserLogins
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string HostName { get; set; }
        public string MacAddress { get; set; }
        public string LocalIp { get; set; }
        public string RemoteIp { get; set; }
        public string UserAgent { get; set; }
        public string Status { get; set; }
        public DateTime LogInTime { get; set; }
        public DateTime? LogOutTime { get; set; }
        public string FieldOffice { get; set; }
    }


    public class RequestModal
    {
        public string RequestId { get; set; }
        public int CompanyId { get; set; }
        public string RequestRefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string EmailSent { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string DeletedAt { get; set; }
        public bool DeletedStatus { get; set; }
        public int Year { get; set; }
        public bool ActiveStatus { get; set; }
        public string Acknowledge { get; set; }
        public string AcknowledgeAt { get; set; }
        public string GeneratedRef { get; set; }
        public string Type { get; set; }
    }


    public class StaffPushApps
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public int DeskCount { get; set; }
        public int FieldOffice { get; set; }
        public int StaffId { get; set; }
        public Staff Staff { get; set; }
    }


    public class CompanyRequest
    {
        public int Year { get; set; }
        public string Type { get; set; }
        public DateTime EndDate { get; set; }
        public int RequestID { get; set; }
    }



    public class History
    {
        public string StaffEmail { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string HistoryDate { get; set; }

    }



    public class AppDocuument
    {
        public int LocalDocID { get; set; }
        public string DocName { get; set; }
        public int EplsDocTypeID { get; set; }
        public int CompanyDocID { get; set; }
        public bool isAddictional { get; set; }
        public bool DeletedStatus { get; set; }
        public string DocType { get; set; }
        public string DocSource { get; set; }
    }

    public class permitsModell
    {
        public int id { get; set; }
        public string permit_no { get; set; }
        public int application_id { get; set; }
        public int company_id { get; set; }
        public int Category_id { get; set; }
        public int OrderId { get; set; }
        public DateTime date_issued { get; set; }
        public DateTime date_expire { get; set; }
        public string categoryName { get; set; }
        public string PhaseName { get; set; }
        public string is_renewed { get; set; }
        public int? elps_id { get; set; }
        public bool Printed { get; set; }
    }
    public class AppReport
    {
        public int StaffID { get; set; }
        public int ReportID { get; set; }
        public string Title { get; set; }
        public string Staff { get; set; }
        public string Comment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }


    public class AppSchdule
    {
        public int SchduleID { get; set; }
        public string SchduleType { get; set; }
        public string SchduleLocation { get; set; }
        public string SchduleDate { get; set; }
        public string SchduleExpired { get; set; }
        public string cResponse { get; set; }
        public string SchduleComment { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public int SchduleByID { get; set; }
        public string SchduleByEmail { get; set; }
        public string CustomerComment { get; set; }
        public string sResponse { get; set; }
        public string SupervisorComment { get; set; }
    }


    public class CurrentDesk
    {
        public string Staff { get; set; }
        public string StaffFieldOffice { get; set; }
    }


    public class RequestApplication
    {
        public int DeskId { get; set; }
        public string RefNo { get; set; }
        public int RequestId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public int? TotalAmount { get; set; }
        public int Year { get; set; }
        public string Status { get; set; }
        public string RRR { get; set; }
        public string TransType { get; set; }
        public int? AmountPaid { get; set; }
        public int? ServiceCharge { get; set; }
        public DateTime? TransDate { get; set; }
        public string TransDescription { get; set; }
        public string TransStatus { get; set; }
        public DateTime? DateApplied { get; set; }
        public string Comment { get; set; }
        public DateTime ReportDate { get; set; }
        public string Staff { get; set; }
        public string GeneratedNo { get; set; }
        public string ReportApproved { get; set; }
        public string ProposalApproved { get; set; }
        public string Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CurrentDesk { get; internal set; }
        public string RequestType { get; internal set; }
        public string DocUrl { get; internal set; }
        public int DurationId { get; internal set; }
    }

    public class CategoryDocuments
    {
        public Categories Category { get; set; }
        public List<Document_Type> documents { get; set; }
    }

    public class PhaseDocuments
    {
        public Phases Phase { get; set; }
        public List<Document_Type> documents { get; set; }
        public List<Document_Type> PhaseFacDocuments { get; set; }
    }
    public class ApplicationDocs
    {
        public int AppID { get; set; }
        public int CDeskID { get; set; }
        public string AppRef { get; set; }
        public int LocalDocID { get; set; }
        public int ElpsCompanyID { get; set; }
        public int ElpsCompanyBID { get; set; }

        public int LocalPartyID { get; set; }
        public int ElpsPartyID { get; set; }
        public int AppDocID { get; set; }
        public bool? isAdditional { get; set; }
        public int CompElpsDocID { get; set; }
        public int? EplsDocTypeID { get; set; }
        public string DocName { get; set; }
        public string DocType { get; set; }
        public string DocSource { get; set; }
        public string AppCategory { get; set; }
        public int SubmitDocID { get; set; }
    }
    public class SubmitDoc
    {
        public int LocalDocID { get; set; }
        public int CompElpsDocID { get; set; }
        public string DocSource { get; set; }
    }

    public class BothDocuments
    {
        public List<LegacyModel> legacyModels { get; set; }
        public List<PresentDocuments> presentDocuments { get; set; }
        public List<MissingDocument> missingDocuments { get; set; }
        public List<PresentDocuments> presentDocuments2 { get; set; }
        public List<MissingDocument> missingDocuments2 { get; set; }
        public List<ApplicationDocuments> AdditionalDoc { get; set; }
    }
    public class OtherDocuments
    {
        public int LocalDocID { get; set; }
        public string DocName { get; set; }

        public List<PresentDocuments> presentDocuments { get; set; }
        public List<MissingDocument> missingDocuments { get; set; }
    }



    public class PresentDocuments
    {
        public int SubmitDocID { get; set; }
        public bool Present { get; set; }
        public string FileName { get; set; }
        public string Source { get; set; }
        public int CompElpsDocID { get; set; }
        public int DocTypeID { get; set; }
        public int LocalDocID { get; set; }
        public string DocType { get; set; }
        public string TypeName { get; set; }
    }


    public class MissingDocument
    {
        public int SubmitDocID { get; set; }
        public bool Present { get; set; }
        public string FileName { get; set; }
        public string Source { get; set; }
        public int CompElpsDocID { get; set; }
        public int DocTypeID { get; set; }
        public int LocalDocID { get; set; }
        public string DocType { get; set; }
        public string TypeName { get; set; }
    }


    public class SearchList
    {
        public List<Categories> categories { get; set; }
        public List<Phases> phases { get; set; }
        public List<FieldOffices> offices { get; set; }
        public List<ZonalOffice> zonalOffices { get; set; }
        public List<States_UT> states { get; set; }
    }



    public class HistoryInformation
    {
        public List<History> histories { get; set; }
    }


    public class ApprovalLetter
    {
        public object CompanyName { get; set; }
        public object FacilityName { get; set; }
        public object FacilityAddress { get; set; }
        public object DateApplied { get; set; }


    }
    public class DM_ATCLetter
    {
        public string StateName { get; set; }
        public string CompanyName { get; set; }
        public object FacilityName { get; set; }
        public object FacilityAddress { get; set; }
        public object DateApplied { get; set; }
        public object ScheduleDate { get; set; }
        public DateTime DateApproved { get; set; }
        public string TanksText { get; set; }
        public string ModifyType { get; set; }
        public List<ApplicationTanks> Tanks { get; set; }

    } public class AppMessage
    {
        public object Subject { get; set; }
        public object Content { get; set; }
        public object RefNo { get; set; }

        public object Status { get; set; }
        public object Stage { get; set; }
        public object TotalAmount { get; set; }
        public object Seen { get; set; }
        public string GeneratedNo { get; set; }
        public object CompanyName { get; set; }
        public object CategoryName { get; set; }
        public object PhaseName { get; set; }
        public object FacilityName { get; set; }
        public object StatutoryLicenceFee { get; set; }
        public object ServiceCharge { get; set; }
        public object TotalAmountDue { get; set; }
        public object ApplicationPeriod { get; set; }
        public object DateSubmitted { get; set; }

    }

    public class RPartner
    {
        public string lineItemsId { get; set; }
        public string beneficiaryName { get; set; }
        public string beneficiaryAccount { get; set; }
        public string bankCode { get; set; }
        public string beneficiaryAmount { get; set; }
        public string deductFeeFrom { get; set; }
    }

    public class ApplicationRequirement
    {
        public int TransactionId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class ApplicationItems
    {
        public string Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    public class ManualValueModel
    {
        public companies Company { get; set; }
        public List<applications> Applications { get; set; }
    }
    public class FormEdit
    {
        public Forms Form { get; set; }
        public bool Locked { get; set; }
    }

    public class CombineFormViewer
    {
        public string FormName { get; set; }
        public List<FieldValues> FormValues { get; set; }
        public bool Recommend { get; set; }
        public string Reason { get; set; }
        public bool ManagerConfirmed { get; set; }
        public string ManagerRejectReason { get; set; }
    }

    public class SchdulesList
    {
        public Schedules Schedule { get; set; }
    }




    public class PermitView
    {
        public string PermitNO { get; set; }
        public string ViewType { get; set; }
        public DateTime? PreviewedAt { get; set; }
        public DateTime? DownloadedAt { get; set; }
        public string UserDetails { get; set; }
    }

    public class BussinessType
    {
        public static List<string> GetBizType()
        {

            var x = new List<string>();

            x.Add("Enterprise");
            x.Add("Limited Liability Company");
            x.Add("Public Liability Company");
            return x;

        }



        //public Byte[] pdfVoucher_file(string sk = "", int custInt = 0, string voucher_code = "")
        //{

        //    var viw = new ViewAsPdf();
        //    viw.ViewName = "~/shared/ErrorTestPdf.cshtml";
        //    viw.Model = new BusinessTypeModel { BusinessTypeName = "PLC" };
        //    var ctx = new ControllerContext();
        //    Byte[] pdfData = viw.BuildFile((byte)ctx);
        //    return pdfData;
        //}

        //public void email_Voucher(string sk, string voucher_code)
        //{
        //    try
        //    {
        //        int constId = 123;
        //        string toEmail = "test@site.com";
        //        string mailBody = "Your Voucher";
        //        MemoryStream pdfStream = new MemoryStream(pdfVoucher_file(sk, constId, voucher_code));
        //        Attachment pdf = new Attachment(pdfStream, "Voucher_" + voucher_code.Trim() + ".pdf", "application/pdf");

        //       // MailHelper.SendEmail(toEmail, "TEst mail", mailBody, pdf);
        //         }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }

    public class BusinessTypeModel
    {
        public string BusinessTypeName { get; set; }
        public int ServiceId { get; set; }
        public List<BusinessTypeVM> BizTypeModel { get; set; }
    }

    public class BusinessTypeVM
    {
        //public Job_Specification JobSpecification { get; set; }
        public bool Selected { get; set; }
    }
    public class ApplicationModel
    {
        //public Application app { get; set; }
        public string Type { get; set; }
        public int Category_Id { get; set; }
        public string Current_Permit { get; set; }
        public int Id { get; set; }
        public string mindate { get; set; }
        public string maxdate { get; set; }
        public string type { get; set; }
        public List<string> state { get; set; }

    }

    public class PermitRenewalModel
    {
        public string PermitNo { get; set; }
        public int ApplicationId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<application_services> Services { get; set; }
    }


    public class ApplicationAPIModel
    {
        public int id { get; set; }
        public string OrderId { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public int CompanyId { get; set; }
        public int LicenseId { get; set; }
        public DateTime Date { get; set; }
        public string licenseName { get; set; }
    }

    public class passModel
    {
        public int companyId { get; set; }
        public int suitabilityId { get; set; }
        public int facilityId { get; set; }
        public int categoryId { get; set; }
        public int ApplicationId { get; set; }
    }

    public static class ApplicationHistoryStatusModel
    {
        public static string Approved { get { return "Approved"; } }
        public static string Rejected { get { return "Rejected"; } }
        public static string Move { get { return "Move"; } }

        public static string FormApproved { get; set; }
        public static string FormRejected { get; set; }
    }
    public class CommonBase
    {
        public string RefNo { get; set; }
        public string Date { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }

    public class LTOLetterModel : CommonBase
    {
        public string Facility { get; set; }
        public string MarketerStatus { get; set; }
        public string LicenseNumber { get; set; }
        public string FieldOffice { get; set; }
    }

    public class SuitabilityLetterModel
    {
        public string IsFieldView { get; set; }
        public string DateApplied { get; set; }
        public DateTime DateApproved { get; set; }
        public string SizeOfLand { get; set; }
        public string FacilityAddress { get; set; }
        public string PrintView { get; set; }
        public string Signature { get; set; }
        public string SignedBy { get; set; }
        public string Signature_N { get; set; }
        public string SignedBy_N { get; set; }
        public string Office { get; set; }
        public bool IsZopscon { get; set; }

        public string Body { get; set; }
        public string FacStateName { get; internal set; }
        public string PhaseShortName { get; internal set; }
        public string ZonalOrFeild { get; set; }
        public string FacilityZonalOrFeildOffice { get; set; }
        public string tnkCountWords { get; set; }

        public string RefNo { get; set; }
        public string Date { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }

    }
    public class Document_Type
    {

        public string Name { get; set; }
        public string Type { get; set; }


        public bool Selected { get; set; }

        public bool isAdditional { get; set; }


        public bool ParentSelected { get; set; }

        public string documentTypeName { get; set; }
        public string Document_Name { get; set; }

        public string UniqueId { get; set; }

        public string Source { get; set; }

        public int CoyFileId { get; set; }

        public int Document_Id { get; set; }
        public int Id { get; set; }
        public int Phase_Id { get; set; }
    }
    public partial class Company_Document
    {

        public int id { get; set; }
        public int Company_Id { get; set; }

        public int Document_Type_Id { get; set; }

        public string documentTypeName { get; set; }
        public string Name { get; set; }

        public string Type { get; set; }

        public string Source { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Date_Modified { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Date_Added { get; set; }

        public bool Status { get; set; }
        public bool Archived { get; set; }
        public string UniqueId { get; set; }

        public string Document_Name { get; set; }
    }
    public class PermitAPIModel
    {
        public int Id { get; set; }
        public string Permit_No { get; set; }

        public string OrderId { get; set; }

        public int Company_Id { get; set; }

        public DateTime Date_Issued { get; set; }

        public DateTime Date_Expire { get; set; }
        public string CategoryName { get; set; }
        public string Is_Renewed { get; set; }
        public int LicenseId { get; set; }
    }

    public class PermitViewModel
    {
        public string PermitFor { get; set; }

        public string PrintView { get; set; }
        public string PermitNo { get; set; }
        public string CompanyIdCode { get; set; }
        public string CompanyNameL1 { get; set; }
        public string CompanyNameL2 { get; set; }
        public string CoyAddL1 { get; set; }
        public string CoyAddL2 { get; set; }
        public string CoyState { get; set; }
        public string FacilityName { get; set; }
        //public string FacilityNameL2 { get; set; }
        public List<KeyValuePair<string, double>> Products { get; set; }
        public string LicenseTitle { get; set; }
        public string FacilityAddress1 { get; set; }
        public string FacilityAddress2 { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal? Fee { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CategoryName { get; set; }
        public byte[] QrCodeImg { get; set; }
        public string FacIdentitificationCode { get; set; }
        public string Signature { get; set; }
        public string Signature_N { get; set; }
        public string SignedStaff { get; set; }
    }
    public class LoginViewModel
    {
        public string email { get; set; }
        public string code { get; set; }
    }

    public class ReportViewModel
    {
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyEmail { get; set; }
        public string Facility { get; set; }
        public string Products { get; set; }
        public string Category { get; set; }
        public string Stage { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
        public DateTime DateApplied { get; set; }
        public string Staff { get; set; }
        public DateTime? ReportDate { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Subject { get; set; }
        public string Comment { get; set; }
        public string DocSource { get; set; }
        public int FacilityId { get; set; }
    }

    //IGR Payment model
    public class RevenueItem
    {
        public int RevenueItemId { get; set; }
        public int Amount { get; set; }
        public int Quantity { get; set; }

    }
    public class OutOfOfficeModel
    {
        public int DeskCount { get; set; }
        public int OutID { get; set; }
        public int StaffID { get; set; }
        public int ReliverID { get; set; }
        public string Staff { get; set; }
        public string StaffRole { get; set; }
        public string StaffLocation { get; set; }
        public string Reliever { get; set; }
        public string RelieverRole { get; set; }
        public string Comment { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string Status { get; set; }
        public bool? DeletedStatus { get; set; }
        public bool? Approved { get; set; }
        public string ApprovedBy { get; set; }
        public string ApproverRole { get; set; }
        public string ApprovedDate { get; set; }
        public string ApproverComment { get; set; }

    }
    public class ROMSFacilityModel
    {
        public string LicenseNumber { get; set; }
        public string FacilityId { get; set; }
        public string UniqueId { get; set; }
        public string LicenseCode { get; set; }
        public string UniqueIdOnELPS { get; set; }
        public string FacilityName { get; set; }
        public CompanyModel Company { get; set; }
        public FacilityAddressModel Address { get; set; }
    }

    public class CompanyModel
    {
        public string Name { get; set; }
        public string ContactName { get; set; }
        public string email { get; set; }
        public string Phone { get; set; }
        public CompanyAddressModel Address { get; set; }
    }
    public class CompanyAddressModel
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string LGA { get; set; }
    }
    public class FacilityAddressModel
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string LGA { get; set; }
        public string Tanks { get; set; }
        public string Pumps { get; set; }
    }
    public class MyApps
    {
        public Legacies legaciess { get; set; }
        public LegacyModel legacies { get; set; }
        public applications app { get; set; }
        public companies comp { get; set; }
        public Facilities fac { get; set; }

        public ApplicationOffice ApplicationOffice;
        public List<AppReport> StaffAppReports { get; set; }
        public List<MeetingSchedules> Schedules
        { get; set; }
        public List<OtherDocuments> otherDocuments { get; set; }
        public List<BothDocuments> bothDocuments { get; set; }

        public List<applications> appList { get; set; } 
        public List<JointApplications> jointApplications { get; set; }
        public List<JointAccounts> jointAccounts { get; set; }
        public JointAccounts SinglejointAccount { get; set; }
        public application_desk_histories appHistory { get; set; }
        public Categories cat { get; set; }
        public Phases phs { get; set; }
        public permits AppPermit { get; set; }
        public List<permits> AppPermits { get; set; }
        public string CompanyEmail { get; set; }
        public string IssueType { get; set; }
        public string RRR { get; set; }
        public string CheckApprovalType { get; set; }
        public string Contact { get; set; }
        public string Products { get; set; }
        public string ModifyType { get; set; }
        public int CategoryId { get; set; }
        public int Id { get; set; }
        public int appID { get; set; }
        public string TanksCount { get; set; }
        public int? ProductsCount { get; set; }
        public string Capacity { get; set; }
        public int? currentDeskID { get; set; }
        public int? OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string ZoneName { get; set; }
        public string ShortName { get; set; }
        public int? ZoneId { get; set; }
        public int Company_Id { get; set; }
        public int category_id { get; set; }
        public int PhaseId { get; set; }
        public string CategoryName { get; set; } 
        public string PhaseName { get; set; }
        public string Current_Permit { get; set; }
        public string Type { get; set; }
        public int ProcessingDays { get; set; }
        public int Year { get; set; }
        public string Yearr { get; set; }
        public decimal Fee_Payable { get; set; }
        public decimal Service_Charge { get; set; }
        public decimal? TransferCost { get; set; }
        public int? days { get; set; }
        public string dateString { get; set; }
        public int? payment_id { get; set; }
        public int? AppStatus { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public CurrentDesk Current_Desk { get; set; }
        public string CurrentOffice { get; set; }
        public string CurrentStaff { get; set; }
        public DateTime Date_Added { get; set; }
        public DateTime? DateProcessed { get; set; }
        public DateTime Date_Modified { get; set; }
        public DateTime? ProcessedOn { get; set; }
        public bool? Submitted { get; set; }
        public bool? hasWorked { get; set; }
        public bool? Assigned { get; set; }
        public bool? Holding { get; set; }
        public string Staff { get; set; }
        public string Activity { get; set; }
        public string CompanyDetails { get; set; }
        public string CompanyName { get; set; }
        public string FacilityName { get; set; }
        public string FlowType { get; set; }
        public string FacilityDetails { get; set; }
        public bool? Migrated { get; set; }
        public int? FacilityId { get; set; }
        public bool? AllowPush { get; set; }
        public Guid ValGroupId { get; set; }
        public string PaymentDescription { get; set; }
        public bool AppProcessed { get; set; }
        public int processID { get; set; }
        public bool SupervisorProcessed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeleteStatus { get; set; }
        public DateTime? DeletedAt { get; set; }

        public List<ApplicationJobSpecPresentations> AppJobSpecs { get; set; }

        public List<SubmittedDocuments> ApplicationDocs { get; set; }


        public List<application_services> AppServices { get; set; }

     
        public List<Document_Type> RemainingDocs { get; set; }
        private int _AddressId;

        public int AddressId
        {
            get { return _AddressId; }
            set { _AddressId = value; }
        }

        public string Address_1 { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string LGA { get; set; }
        public string FacIdentificationCode { get; set; }

        public string CategoryCode { get; set; }

        public string FacilityAddress()
        {
            Address_1 = Address_1.EndsWith(",") ? Address_1.Substring(0, Address_1.Length - 1) : Address_1;
            return Address_1;
        }

        /// <summary>
        /// Returns the Facility full address including COUNTRY
        /// </summary>
        /// <returns></returns>
        public string FacilityFullAddress()
        {
            Address_1 = Address_1.EndsWith(",") ? Address_1 : Address_1 + ",";
            City = City.EndsWith(",") ? City : City + ", ";
            StateName = StateName.EndsWith(",") ? StateName : StateName + ", ";
            return Address_1 + City + StateName + CountryName;
        }

        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// Returns the Facility full address excluding COUNTRY
        /// </summary>
        /// <returns></returns>
        public string FacilityFullAddress2()
        {
            Address_1 = Address_1.Trim().EndsWith(",") ? Address_1 + " " : Address_1 + ",";
            if (!string.IsNullOrEmpty(City) && Address_1.ToLower().Contains(City.ToLower()))
                Address_1 = Address_1.ToLower().Replace(City.ToLower(), "");
            if (Address_1.ToLower().Contains(StateName.ToLower()))
                Address_1 = Address_1.ToLower().Replace(StateName.ToLower(), "");
            if (Address_1.ToLower().Contains("state"))
                Address_1 = Address_1.ToLower().Replace("state", "");
            Address_1 = Address_1.ToLower().Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(", ,", ",");
            City = string.IsNullOrEmpty(City) ? "" : City.Trim().EndsWith(",") ? City + " " : City + ", ";
            StateName = StateName.Trim().EndsWith(",") ? StateName.Substring(0, StateName.Length - 2) + " State" : StateName + " State";
            return Address_1 + City + StateName;
        }


    }
    public class TankModel
    {

        public int Id { get; set; }
        public int FacilityId { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool HasATG { get; set; }
        public string MaxCapacity { get; set; }
        public DateTime? CreateAt { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string Position { get; set; }
        public double Height { get; set; }
        public double Diameter { get; set; }
        public bool Decommissioned { get; set; }
        public string FriendlyName { get; set; }
        [NotMapped]
        public string Recalibrate { get; set; }
        [NotMapped]
        public string ModifyType { get; set; }
        [NotMapped]
        public string AppTank { get; set; }
    }

    public class ExtraPaymentsModel
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Type { get; set; }
        public decimal Amount { get; set; }
        public string RRR { get; set; }
        public string Reference { get; set; }
        public string Category { get; set; }
        public string Phase { get; set; }
        public string Comment { get; set; }
        public DateTime? Date { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
        public string DatePaid { get; set; }
    }
    public class InspectionDataModel
    {
        public applications app { get; set; }
        public MeetingSchedules meetingSchedules { get; set; }
        public ApplicationForms appForm { get; set; }
        public List<TankModel> AppTanks { get; set; }
        public List<Fields> Fields { get; set; }
        public List<FieldValues> FieldValues { get; set; }
        public List<JointApplications> jointApplications { get; set; }
        public List<JointAccounts> jointAccounts { get; set; }
        public application_desk_histories appHistory { get; set; }
        public int appID { get; set; }
        public int Company_Id { get; set; }
        public int category_id { get; set; }
        public int PhaseId { get; set; }
        public string FacilityAddrss { get; set; } 
        public string Latitude { get; set; } 
        public string Longitude { get; set; } 
        public string StaffName { get; set; } 
        public string StaffRole{ get; set; } 
        public string StaffComment{ get; set; } 
        public string StaffOffice{ get; set; } 
        public string CategoryName { get; set; } 
        public string PhaseName { get; set; }
        public string Current_Permit { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public decimal Fee_Payable { get; set; }
        public decimal? TransferCost { get; set; }
        public int? payment_id { get; set; }
        public string Status { get; set; }
        public string Reference { get; set; }
        public string current_Permit { get; set; }
        public string CompanyDetails { get; set; }
        public string CompanyName { get; set; }
        public string FacilityName { get; set; }
        public string FacilityDetails { get; set; }
        public bool? Migrated { get; set; }
        public int? FacilityId { get; set; }
        public DateTime Date_Added { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime FormCreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<ApplicationJobSpecPresentations> AppJobSpecs { get; set; }


        private int _AddressId;

        public int AddressId
        {
            get { return _AddressId; }
            set { _AddressId = value; }
        }

        public string Address_1 { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public string CountryName { get; set; }
        public string LGA { get; set; }
        public string FacIdentificationCode { get; set; }

        public string CategoryCode { get; set; }

        public string FacilityAddress()
        {
            Address_1 = Address_1.EndsWith(",") ? Address_1.Substring(0, Address_1.Length - 1) : Address_1;
            return Address_1;
        }

        /// <summary>
        /// Returns the Facility full address including COUNTRY
        /// </summary>
        /// <returns></returns>
        public string FacilityFullAddress()
        {
            Address_1 = Address_1.EndsWith(",") ? Address_1 : Address_1 + ",";
            City = City.EndsWith(",") ? City : City + ", ";
            StateName = StateName.EndsWith(",") ? StateName : StateName + ", ";
            return Address_1 + City + StateName + CountryName;
        }

        public DateTime DateSubmitted { get; set; }

        /// <summary>
        /// Returns the Facility full address excluding COUNTRY
        /// </summary>
        /// <returns></returns>
        public string FacilityFullAddress2()
        {
            Address_1 = Address_1.Trim().EndsWith(",") ? Address_1 + " " : Address_1 + ",";
            if (!string.IsNullOrEmpty(City) && Address_1.ToLower().Contains(City.ToLower()))
                Address_1 = Address_1.ToLower().Replace(City.ToLower(), "");
            if (Address_1.ToLower().Contains(StateName.ToLower()))
                Address_1 = Address_1.ToLower().Replace(StateName.ToLower(), "");
            if (Address_1.ToLower().Contains("state"))
                Address_1 = Address_1.ToLower().Replace("state", "");
            Address_1 = Address_1.ToLower().Replace(",,", ",").Replace(",,", ",").Replace(",,", ",").Replace(", ,", ",");
            City = string.IsNullOrEmpty(City) ? "" : City.Trim().EndsWith(",") ? City + " " : City + ", ";
            StateName = StateName.Trim().EndsWith(",") ? StateName.Substring(0, StateName.Length - 2) + " State" : StateName + " State";
            return Address_1 + City + StateName;
        }

    }
    public class MeetingCalModel
    {
        public int Day { get; set; }
        public List<MeetingSchedulesModel> MyMeetings { get; set; }
        public List<MeetingSchedulesModel> ColleagueMeeting { get; set; }
    }
    public class MeetingSchedulesModel
    {
        public int Id { get; set; } 
        public int CompanyId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string CompanyName { get; set; }
        public string Reference { get; set; }
        public string CategoryName { get; set; }

        public int VenueId { get; set; }
        public string Message { get; set; }
        public string Venue { get; set; }
        public int ApplicationId { get; set; }
        public string StaffUserName { get; set; }
        public string Address { get; set; }
        public DateTime Date { get; set; }
        public string? UpdatedAt { get; set; }
        public bool? Accepted { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? AcceptanceDate { get; set; }

        public DateTime MeetingDate { get; set; }
        public string DeclineReason { get; set; }
        public bool? WaiverRequest { get; set; }
        public string WaiverReason { get; set; }
        public bool? ScheduleExpired { get; set; }
        public bool? Completed { get; set; }
        public string FinalComment { get; set; }
    }
    public class ApplicationInformation
    {
        public List<MyApps> ApplicationDetails { get; set; }
        public List<ApplicationDocs> ApplicationDocs { get; set; }
        public List<ApplicationProccess> ApplicationProccess { get; set; }
        public List<History> histories { get; set; }
        //public List<ApplicationDetails> AppDetails { get; set; }
        public List<AppReport> StaffAppReports { get; set; }
        public List<MeetingSchedules> 
            ules { get; set; }
        public List<OtherDocuments> otherDocuments { get; set; }
        public List<BothDocuments> bothDocuments { get; set; }

        public List<CurrentDesk> currentDesks { get; set; }
        public List<document_types> ApplicationDocuments { get; set; }

    }
    public class ApplicationDetailsModel
    {
        public int ApplicationId { get; set; }
        public int CompanyId { get; set; }
        public int FacilityId { get; set; }
        public string FormId { get; set; }
        public string FacilityName { get; set; }
        public string FacilityLocation { get; set; }
        public string CompanyName { get; set; }
        public string ApplicationType { get; set; }
        public string FormType { get; set; }
        public string InspectorEmail { get; set; }
        public DateTime InspectionDate { get; set; }
        public string InspectionContactName { get; set; }
        public string InspectionContactNumber { get; set; }


    }
    public class MySchdule
    {
        public bool? WaiverRequest { get; set; }
        public bool? Accepted { get; set; }
        public bool? Approved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? AcceptanceDate { get; set; }

        public int ScheduleID { get; set; }
        public int ApplicationID { get; set; }
        public string ScheduleDate { get; set; }
        public string ScheduleBy { get; set; }
        public int FacilityID { get; set; }
        public int FieldOfficeID { get; set; }
        public string AppOffice { get; set; }
        public string FacilityName { get; set; }
        public string FacilityAddress { get; set; }
        public string ContactName { get; set; }public string ApprovedBy { get; set; }
        public string ContactPhone { get; set; }
        public string CompanyName { get; set; }
        public int CompanyID { get; set; }
        public int staffID { get; set; }
        public int? CustomerRespons { get; set; }
        public string? CustomerResponse { get; set; }
        public string StaffComment { get; set; }
        public string SupervisorComment { get; set; }
        public string CustomerComment { get; set; }
        public string ScheduleType { get; set; }
        public string AppRef { get; set; }
        public string ScheduleLocation { get; set; } public string Phase { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Supervisor { get; set; }
        public string SupervisorApprovedd { get; set; }
        public int? SupervisorApproved { get; set; }
        public int ProposedYear { get; set; }
    }


    public class StaffDesk
    {
        public string StaffName { get; set; }
        public string Location { get; set; }
        public string StaffEmail { get; set; }
        public string FieldOffice { get; set; }
        public string ZonalOffice { get; set; }
        public int StaffID { get; set; }
        public int OfficeId { get; set; }
        public int ZoneId { get; set; }
        public string StaffRole { get; set; }
        public int AppCount { get; set; }
        public string ActiveStatus { get; set; }
        public string DeletedStatus { get; set; }
        public int AllAppCount { get; set; }
    }



    public class PaymentDetailsSubmit
    {
        public int RequestID { get; set; }
        public string RefNo { get; set; }
        public string Status { get; set; }
        public string AppType { get; set; }
        public string AppStage { get; set; }
        public string CompanyName { get; set; }
        public int? Amount { get; set; }
        public int? ServiceCharge { get; set; }
        public int? TotalAmount { get; set; }
        public string ShortName { get; set; }
        public int FacID { get; set; }
        public DateTime DateApplied { get; set; }
        public string rrr { get; set; }
        public int SurChargeAmount { get; set; }
        public string Description { get; set; }
    }




    public class MyDeskApps
    {
        public int DeskID { get; set; }
        public int RequestID { get; set; }
        public bool HasWork { get; set; }
        public bool HasPushed { get; set; }
        public int ProcessID { get; set; }
        public string RefNo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Year { get; set; }
        public string Type { get; set; }
        public string DateApplied { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string Status { get; set; }
        public string Activity { get; set; }
        public int RequestDeskId { get; internal set; }
        public int DurationId { get; internal set; }
    }

    //Latest Addition

    public class PaymentSummaryModel
    {
        public string ReportTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ReportForm { get; set; }
        //public Highcharts SummaryChart { get; set; }
        public List<PaymentSummaryTable> SummaryTable { get; set; }
        public List<PaymentReportModel> ReportSummary { get; set; }
    }

    public class PaymentReportModel
    {
        public long ID { get; set; }
        public int ApplicationID { get; set; }
        public string ReferenceNo { get; set; }
        public string FacilityAddress { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public string Office { get; set; }
        public string ZonalOffice { get; set; }
        public string Type { get; set; }
        public string PaymentRef { get; set; }
        public string Channel { get; set; }
        public DateTime? Date { get; set; }
        public string Category { get; set; }
        public double Amount { get; set; }
        public string CompanyName { get; set; }
        public string FacilityName { get; set; }
        public string ReceiptNo { get; set; }
        public double TotalAmount { get; set; }
        public decimal? TotalExtraAmount { get; set; }
        public decimal? Fee { get; set; }
        public decimal? Charge { get; set; }
        public string StateName { get; set; }
        public string LGA { get; set; }

        //New
        public long TotalCatAmount { get; set; }
        public string FG { get; set; }
        public string NMDPRACORC { get; set; }
        public string NMDPRAIGR { get; set; }
        public string ProcessingFee { get; set; }
        public string Contractor { get; set; }
        public string PaymentBreakdown { get; set; }

    }


    public class ReportModel
    {

        public int count { get; set; }
        public int AppId { get; set; }
        public int PhaseId { get; set; }

        public string RefNo { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string State { get; set; }
        public string Lga { get; set; }
        public string CompanyName { get; set; }
        public int FacId { get; set; }
        public string Facility { get; set; }
        public string FacilityAddress { get; set; }
        public string Products { get; set; }
        public string FieldOffice { get; set; }
        public string CurrentDesk { get; set; }
        public string TotalDays { get; set; }
        public string ZonalOffice { get; set; }
        public int? OfficeId { get; set; }
        public int? ZonalId { get; set; }
        public int category_id { get; set; }
        public string Status { get; set; }
        public string DateApplied { get; set; }
        public string DateSubmitted { get; set; }

        public bool? Submitted { get; set; }
        public string issueType { get; set; }
        public string category { get; set; }
        public int Category_Id { get; set; }
        public string Current_Permit { get; set; }
        public int Id { get; set; }
        public string mindate { get; set; }
        public string maxdate { get; set; }
        public string type { get; set; }
        public List<string> state { get; set; }
        public List<applications> applications { get; set; }
        public List<MyApps> appModel { get; set; }
        public List<permits> permitsRepo { get; set; }

        public List<BasicReportModel> basicReportModel { get; set; }

    }
    public class PaymentFeeDescription
    {
        public decimal Fee { get; set; }
        public int NoOfYr { get; set; }
        public string FeeDescription { get; set; }
    }
    public class StaffProcessModel
    {
        public string StaffId { get; set; }
        public string StaffName { get; set; }
        public int Approved { get; set; }
        public int Processing { get; set; }
        public int Rejected { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class RemitaResponse
    {
        public string statusmessage { get; set; }
        public string merchantId { get; set; }
        public string status { get; set; }
        public string RRR { get; set; }
        public string Amount { get; set; }
        public string transactiontime { get; set; }
        public string orderId { get; set; }


    }


    public partial class ApplicationDeskHistoryModel 
    {

        public int id { get; set; }
        public int CurrentDesk { get; set; }
        public int application_id { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public DateTime? DateSubmitted { get; set; }
        public string Status { get; set; }
        public string type { get; set; }
        public int year { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string CategoryName { get; set; }
        public string CompanyName { get; set; }
        public string reference { get; set; }
    }

    public class AppProcessModel
    {
        public applications Application { get; set; }
        public bool InspectionRequired { get; set; }
        public bool InspectionDone { get; set; }

        public bool PresentationRequired { get; set; }
        public bool PresentationDone { get; set; }
        public bool UserInHSE { get; set; }
        public int? ApplicationProcessId { get; set; }

        public List<ApplicationForms> RequiredInspection { get; set; }

        public MeetingSchedules Meeting { get; set; }
        public int DepartmentId { get; set; }
    }

    public class ApplicationProcessingsModel
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
        public DateTime? DateProcessed { get; set; }
        public int? processor { get; set; }
        public int? ProcessingRule_Id { get; set; }
        public bool? Processed { get; set; }
        public int ApplicationId { get; set; }
        public int RoleId { get; set; }
        public int DepartmentId{ get; set; }
        public bool Assigned { get; set; }
        public string ProcessingLocation { get; set; }
        public bool? Holding { get; set; }
        public DateTime? Date_Assigned { get; set; }
        public bool AutoPushed { get; set; }
    }
    public class MistdoModel
    {
        public bool success { get; set; }
        public string message { get; set; }
        public data datas { get; set; }
    }


    public class data
    {
        public string fullname { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string certificateNo { get; set; }
        public DateTime certificateIssue { get; set; }
        public DateTime certificateExpiry { get; set; }
        public string mistdoId { get; set; }
    }

    public class MistdoStaffs
    {
        public string FullName { get; internal set; }
        public DateTime IssuedDate { get; internal set; }
        public string CompanyName { get; internal set; }
        public string PhoneNo { get; internal set; }
        public DateTime CreatedAt { get; internal set; }
        public DateTime ExpiryDate { get; internal set; }
        public string Email { get; internal set; }
        public string CertificateNo { get; internal set; }
        public string MistdoServiceId { get; internal set; }
    }

    public class OtherModel
    {
        public string frmLegacy { get; set; }
        public string functional { get; set; }
        public int facilityId { get; set; }
        public int phaseId { get; set; }
        public string PermitNo { get; set; }
        public string review { get; set; }
        public string FacilityName { get; set; }
        public string SanctionId { get; set; }

        public string ISAlongPipeLine { get; set; }
        public bool? IsUnderHighTension { get; set; }
        public string IsOnHighWay { get; set; }
        public string SizeOfLand { get; set; }

        public string category { get; set; }
        //string , string , string 
        public string ModificationType { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
    }

    public class RenewModel
    {
        public permits Permit { get; set; }
        public companies Company { get; set; }
        public Facilities Facility { get; set; }
        public List<Products> Products { get; set; }
        public List<ApplicationTanks> AppTanks { get; set; }
        public List<Tanks> Tanks { get; set; }
        public List<Pumps> Pumps { get; set; }
        public List<string> ATGParams { get; set; }
    }
    public class BranchFieldOfficeModel
    {
       public int Id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string stateName { get; set; }
        public string countryName { get; set; }
        public int countryId { get; set; }
        public int stateId { get; set; }
        public bool? isFieldOffice { get; set; }
        public bool? selected { get; set; }
    }
    public class BranchStateModel
    {
               public int stateId { get; set; }
               public int fdId { get; set; }
               public int zoneId { get; set; }

               public string zoneName { get; set; }
               public string stateName { get; set; }


    }
    public class BranchModel
    {
        [Display(Name = "Covered States")]
        public List<BranchStateModel> coveredStates { get; set; }

        
        [Display(Name = "Covered Field Offices")]
        public List<BranchFieldOfficeModel> coveredFieldOffices { get; set; }
        public string name { get; set; }
        public string branchName { get; set; }
        public string code { get; set; }

        public int id { get; set; }
        public int branchId { get; set; }
        public string address { get; set; }

        public string stateName { get; set; }

        public string countryName { get; set; }

        public int countryId { get; set; }
       
        public bool Selected { get; set; }
    }


    public class vBranch
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int StateId { get; set; }
        public string Address { get; set; }

        public string StateName { get; set; }

        public string CountryName { get; set; }

        public int CountryId { get; set; }
        [NotMapped]
        public bool Selected { get; set; }
    }
    public class MeetingVenue
    {
        public string Title { get; set; }
        public string Address { get; set; }
        public int Id { get; set; }
    }
    public class FacilityModel
    {
        public List<TankModel> Tanks { get; set; }
        public List<LoadingArmModel> LoadingArms { get; set; }

        public string? SizeOfLand { get; set; }
        public bool? ISAlongPipeLine { get; set; }
        public bool? IsUnderHighTension { get; set; }
        public int? StationsWithin2KM { get; set; }
        public string? DistanceFromExistingStation { get; set; }
        public bool? IsOnHighWay { get; set; }
        public int CompanyId { get; set; }
        public string? EIANMDPRAStaff { get; set; }
        public string Reference { get; set; }
        

        public int Id { get; set; }
        public string Name { get; set; }
        public string FacilityCode { get; set; }
        public string CompanyEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public int StateId { get; set; }
        public int? LGAId { get; set; }
        public int? NoOfPumps { get; set; }
        public int? NoOfTanks { get; set; }
        public string LGA { get; set; }
    }
    public class FacilityModelDetail
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FacilityCode { get; set; }
        public string CompanyEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string StateName { get; set; }
        public int NoOfPumps { get; set; }
        public int NoOfTanks { get; set; }
        public List<LoadingArmModel> LoadingArms { get; set; }
        public List<TankModel> Tanks { get; set; }
        public string LGA { get; set; }
    }
    
    public class LoadingArmModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TankId { get; set; }
    }
    public class AlertBox
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public AlertType ButtonType { get; set; }

        private string _GetTypeString;

        public string GetTypeString
        {
            get
            {
                switch (this.ButtonType)
                {
                    case AlertType.Success:
                        return "alert-success";
                    case AlertType.Failure:
                        return "alert-danger";
                    case AlertType.Warning:
                        return "alert-warning";
                    case AlertType.Info:
                        return "alert-info";
                    default:
                        return "";
                }
            }
        }
    }

    public enum AlertType
    {
        Success,
        Failure,
        Warning,
        Info
    }
    public class Staff_UserBranchModel
    {
        public int Id { get; set; }
        public int StaffId { get; set; }
        public string FieldOffice { get; set; }
        public string StaffEmail { get; set; }
        public string StaffFullName { get; set; }
        public string RoleName { get; set; }
        public string ZoneName { get; set; }
        public int FieldId { get; set; }
        public int ZoneId { get; set; }
        public int ZoneFieldOfficeID { get; set; }
        public int RoleId { get; set; }
        public int DepartmentId { get; set; }
        public int? DeskCount { get; set; }
        public bool? Active { get; set; }
        public bool? DeletedStatus { get; set; }
    }
    public class AppReportModel
    {
        public string category { get; set; }
        public string state { get; set; }
        public string status { get; set; }
        public string staffname { get; set; }
        public int value { get; set; }
        public int categoryvalue { get; set; }
        public double categoryamount { get; set; }
        public int statevalue { get; set; }
        public int statusvalue { get; set; }
        public int total { get; set; }

        public List<StatusCountModel> statusCountModel { get; set; }
    }

    public class StatusCountModel
    {
        public string value { get; set; }
        public int count { get; set; }

    }
    public class DropStaff
    {
        public int FieldOfficeId { get; set; }
        public int StaffId { get; set; }
        public string Role { get; set; }
        public int ZonalOfficeId { get; set; }
        public int ProcessId { get; set; }
        public int DeskCount { get; set; }
        public int Sort { get; set; }
        public string Process { get; set; }
    }


    public class ApplicationOffice
    {
        public string StateName { get; set; }
        public string ZonalOrField{ get; set; }
        public int FieldOfficeId { get; set; }
        public string OfficeName { get; set; }
        public int ZonalOfficeId { get; set; }
        public string ZonalOffice { get; set; }
        public int SateId { get; set; }
    }

    public class BasicReportModel
    {
        public string Category { get; set; }
        public int Count { get; set; }
    }

    public class PaymentSummaryTable
    {
        public string Category { get; set; }
        public List<Distribution> Distribution { get; set; }
    }

    public struct Distribution
    {
        public string Field; public double Value;
    }

    public class JointReportModel
    {
        public List<JointAccountStaffs> FD_Inspectors { get; set; }
        public List<FieldOffices> FD_Branch { get; set; }
        public List<AppReport> Reports { get; set; }
        public List<JointStaffModel> JointStaff { get; set; }
    }

    public class JointStaffModel
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FieldOffice { get; set; }
        public bool? OperationsCompleted { get; set; }
        public bool? SignedOff { get; set; }
    }

    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        //public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


    public class RegisterViewModel
    {
        [Required, Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Required, Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; }
        [Required, Display(Name = "Business Type")]
        public string BusinessType { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Phone Number")]
        [MaxLength(13, ErrorMessage = "Phone number cannot be more than 13 characters")]
        [MinLength(11, ErrorMessage = "Phone number must be atleast 11 characters")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public int CompanyId { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }


}
