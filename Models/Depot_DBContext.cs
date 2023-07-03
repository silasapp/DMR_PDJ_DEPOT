using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace NewDepot.Models
{
    public partial class Depot_DBContext : DbContext
    {
        public Depot_DBContext()
        {
        }

        public Depot_DBContext(DbContextOptions<Depot_DBContext> options)
            : base(options)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                              .AddJsonFile("appsettings.json")
                              .Build();
                optionsBuilder.UseSqlServer(configuration.GetConnectionString("Depot_DBConnectionString"));
            }
        }

        public virtual DbSet<ATCCompanies> ATCCompanies { get; set; }
        public virtual DbSet<ATCs> ATCs { get; set; }
        public virtual DbSet<ATGs> ATGs { get; set; }
        public virtual DbSet<Accounts> Accounts { get; set; }
        public virtual DbSet<AppDeskHistory> AppDeskHistory { get; set; }
        public virtual DbSet<AppPhaseDocuments> AppPhaseDocuments { get; set; }
        public virtual DbSet<ApplicationDocuments> ApplicationDocuments { get; set; }
        public virtual DbSet<ApplicationExtraPayments> ApplicationExtraPayments { get; set; }
        public virtual DbSet<ApplicationForms> ApplicationForms { get; set; }
        public virtual DbSet<ApplicationHistories> ApplicationHistories { get; set; }
        public virtual DbSet<ApplicationJobSpecPresentations> ApplicationJobSpecPresentations { get; set; }
        public virtual DbSet<ApplicationProccess> ApplicationProccess { get; set; }
        public virtual DbSet<ApplicationRequest> ApplicationRequest { get; set; }
        public virtual DbSet<ApplicationTanks> ApplicationTanks { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<AuditLogs> AuditLogs { get; set; }
        public virtual DbSet<AuditTrail> AuditTrail { get; set; }
        public virtual DbSet<Categories> Categories { get; set; }
        public virtual DbSet<CategoryRoutings> CategoryRoutings { get; set; }
        public virtual DbSet<CompanyTechnicalAllowances> CompanyTechnicalAllowances { get; set; }
        public virtual DbSet<CrudeStreams> CrudeStreams { get; set; }
        public virtual DbSet<Departments> Departments { get; set; }
        public virtual DbSet<Document_Type_Applications> Document_Type_Applications { get; set; }
        public virtual DbSet<ExpiredScheduledMeetings> ExpiredScheduledMeetings { get; set; }
        public virtual DbSet<Facilities> Facilities { get; set; }
        public virtual DbSet<FacilityConditions> FacilityConditions { get; set; }
        public virtual DbSet<FacilityModifications> FacilityModifications { get; set; }
        public virtual DbSet<FacilityPermit> FacilityPermit { get; set; }
        public virtual DbSet<FacilityPermits> FacilityPermits { get; set; }
        public virtual DbSet<FacilityTankModifications> FacilityTankModifications { get; set; }
        public virtual DbSet<FieldOfficeStates> FieldOfficeStates { get; set; }
        public virtual DbSet<FieldOffices> FieldOffices { get; set; }
        public virtual DbSet<FieldValues> FieldValues { get; set; }
        public virtual DbSet<Fields> Fields { get; set; }
        public virtual DbSet<Forms> Forms { get; set; }
        public virtual DbSet<InspectionMeetingAttendees> InspectionMeetingAttendees { get; set; }
        public virtual DbSet<InspectionSchedules> InspectionSchedules { get; set; }
        public virtual DbSet<JointAccountReports> JointAccountReports { get; set; }
        public virtual DbSet<JointAccountStaffs> JointAccountStaffs { get; set; }
        public virtual DbSet<JointAccounts> JointAccounts { get; set; }
        public virtual DbSet<JointApplications> JointApplications { get; set; }
        public virtual DbSet<Leaves> Leaves { get; set; }
        public virtual DbSet<Legacies> Legacies { get; set; }
        public virtual DbSet<Lgas> Lgas { get; set; }
        public virtual DbSet<Location> Location { get; set; }
        public virtual DbSet<Logins> Logins { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<ManagerReminders> ManagerReminders { get; set; }
        public virtual DbSet<ManagerScheduleMeetings> ManagerScheduleMeetings { get; set; }
        public virtual DbSet<ManualRemitaValues> ManualRemitaValues { get; set; }
        public virtual DbSet<MarketingCompanies> MarketingCompanies { get; set; }
        public virtual DbSet<MeetingSchedules> MeetingSchedules { get; set; }
        public virtual DbSet<MistdoStaff> MistdoStaff { get; set; }
        public virtual DbSet<Multi_Inspections> Multi_Inspections { get; set; }
        public virtual DbSet<MyDesk> MyDesk { get; set; }
        public virtual DbSet<Notifications> Notifications { get; set; }
        public virtual DbSet<OilTerminals> OilTerminals { get; set; }
        public virtual DbSet<OutOfOffice> OutOfOffice { get; set; }
        public virtual DbSet<PaymentApprovals> PaymentApprovals { get; set; }
        public virtual DbSet<PhaseFacilityDocuments> PhaseFacilityDocuments { get; set; }
        public virtual DbSet<PhaseRoutings> PhaseRoutings { get; set; }
        public virtual DbSet<Phases> Phases { get; set; }
        public virtual DbSet<ProcessingRules> ProcessingRules { get; set; }
        public virtual DbSet<Products> Products { get; set; }
        public virtual DbSet<Pumps> Pumps { get; set; }
        public virtual DbSet<Receipts> Receipts { get; set; }
        public virtual DbSet<RemitaPaymentStatuses> RemitaPaymentStatuses { get; set; }
        public virtual DbSet<Reports> Reports { get; set; }
        public virtual DbSet<RunTimes> RunTimes { get; set; }
        public virtual DbSet<Sanctions> Sanctions { get; set; }
        public virtual DbSet<Schdules> Schdules { get; set; }
        public virtual DbSet<ScheduleTransactions> ScheduleTransactions { get; set; }
        public virtual DbSet<Schedules> Schedules { get; set; }
        public virtual DbSet<Signatories> Signatories { get; set; }
        public virtual DbSet<Staff> Staff { get; set; }
        public virtual DbSet<Staffs> Staffs { get; set; }
        public virtual DbSet<States> States { get; set; }
        public virtual DbSet<States_UT> States_UT { get; set; }
        public virtual DbSet<GeoPoliticalStates> GeoPoliticalStates { get; set; }
        public virtual DbSet<GeoPoliticalZone> GeoPoliticalZone { get; set; }
        public virtual DbSet<SubmittedDocuments> SubmittedDocuments { get; set; }
        public virtual DbSet<SuitabilityInspections> SuitabilityInspections { get; set; }
        public virtual DbSet<TakeOvers> TakeOvers { get; set; }
        public virtual DbSet<TankInspections> TankInspections { get; set; }
        public virtual DbSet<TankLeakTests> TankLeakTests { get; set; }
        public virtual DbSet<Tanks> Tanks { get; set; }
        public virtual DbSet<TrainingPrograms> TrainingPrograms { get; set; }
        public virtual DbSet<UserBranches> UserBranches { get; set; }
        public virtual DbSet<UserRoles> UserRoles { get; set; }
        public virtual DbSet<Waivers> Waivers { get; set; }
        public virtual DbSet<WorkFlows> WorkFlows { get; set; }
        public virtual DbSet<WorkProccess> WorkProccess { get; set; }
        public virtual DbSet<WorkRoles> WorkRoles { get; set; }
        public virtual DbSet<ZonalFieldOffice> ZonalFieldOffice { get; set; }
        public virtual DbSet<ZonalOffice> ZonalOffice { get; set; }
        public virtual DbSet<ZoneStates> ZoneStates { get; set; }
        public virtual DbSet<__MigrationHistory> __MigrationHistory { get; set; }
        public virtual DbSet<addresses> addresses { get; set; }
        public virtual DbSet<application_Processings> application_Processings { get; set; }
        public virtual DbSet<application_desk_histories> application_desk_histories { get; set; }
        public virtual DbSet<application_documents> application_documents { get; set; }
        public virtual DbSet<application_services> application_services { get; set; }
        public virtual DbSet<applications> applications { get; set; }
        public virtual DbSet<branches> branches { get; set; }
        public virtual DbSet<companies> companies { get; set; }
        public virtual DbSet<company_directors> company_directors { get; set; }
        public virtual DbSet<company_documents> company_documents { get; set; }
        public virtual DbSet<company_expatriate_quotas> company_expatriate_quotas { get; set; }
        public virtual DbSet<company_key_staffs> company_key_staffs { get; set; }
        public virtual DbSet<company_medicals> company_medicals { get; set; }
        public virtual DbSet<company_nsitfs> company_nsitfs { get; set; }
        public virtual DbSet<company_proffessionals> company_proffessionals { get; set; }
        public virtual DbSet<company_technical_agreements> company_technical_agreements { get; set; }
        public virtual DbSet<countries> countries { get; set; }
        public virtual DbSet<currencies> currencies { get; set; }
        public virtual DbSet<document_type_categories> document_type_categories { get; set; }
        public virtual DbSet<document_types> document_types { get; set; }
        public virtual DbSet<faq_description> faq_description { get; set; }
        public virtual DbSet<faqs> faqs { get; set; }
        public virtual DbSet<files> files { get; set; }
        public virtual DbSet<invoices> invoices { get; set; }
        public virtual DbSet<key_staff_certificates> key_staff_certificates { get; set; }
        public virtual DbSet<messages> messages { get; set; }
        public virtual DbSet<pages> pages { get; set; }
        public virtual DbSet<payment_transactions> payment_transactions { get; set; }
        public virtual DbSet<permits> permits { get; set; }
        public virtual DbSet<remita_transactions> remita_transactions { get; set; }
        public virtual DbSet<reversal_transactions> reversal_transactions { get; set; }
        public virtual DbSet<vAddresses> vAddresses { get; set; }
        public virtual DbSet<vApplicationAddresses> vApplicationAddresses { get; set; }
        public virtual DbSet<vApplicationDeskHistories> vApplicationDeskHistories { get; set; }
        public virtual DbSet<vApplicationDocuments> vApplicationDocuments { get; set; }
        public virtual DbSet<vApplicationForms> vApplicationForms { get; set; }
        public virtual DbSet<vApplicationProcessingRules> vApplicationProcessingRules { get; set; }
        public virtual DbSet<vApplicationTanks> vApplicationTanks { get; set; }
        public virtual DbSet<vApplication_Processings> vApplication_Processings { get; set; }
        public virtual DbSet<vApplications> vApplications { get; set; }
        public virtual DbSet<vCategoryDocuments> vCategoryDocuments { get; set; }
        public virtual DbSet<vCategoryRoutings> vCategoryRoutings { get; set; }
        public virtual DbSet<vCompAddressesU> vCompAddressesU { get; set; }
        public virtual DbSet<vCompanies> vCompanies { get; set; }
        public virtual DbSet<vCompanyAspUsers> vCompanyAspUsers { get; set; }
        public virtual DbSet<vCompanyDirectors> vCompanyDirectors { get; set; }
        public virtual DbSet<vCompanyExpatriateQuotas> vCompanyExpatriateQuotas { get; set; }
        public virtual DbSet<vCompanyFile2> vCompanyFile2 { get; set; }
        public virtual DbSet<vCompanyMedicals> vCompanyMedicals { get; set; }
        public virtual DbSet<vCompanyNsitfs> vCompanyNsitfs { get; set; }
        public virtual DbSet<vCompanyProffessionals> vCompanyProffessionals { get; set; }
        public virtual DbSet<vCompanyTechnicalAgreements> vCompanyTechnicalAgreements { get; set; }
        public virtual DbSet<vCompanyfiles> vCompanyfiles { get; set; }
        public virtual DbSet<vFacilities> vFacilities { get; set; }
        public virtual DbSet<vFacilitiesWithAtleasOneApplications> vFacilitiesWithAtleasOneApplications { get; set; }
        public virtual DbSet<vFacilityTankModifications> vFacilityTankModifications { get; set; }
        public virtual DbSet<vFormFields> vFormFields { get; set; }
        public virtual DbSet<vFormValues> vFormValues { get; set; }
        public virtual DbSet<vInspectionScheduleApplications> vInspectionScheduleApplications { get; set; }
        public virtual DbSet<vJointAccountReports> vJointAccountReports { get; set; }
        public virtual DbSet<vJointAccountStaffs> vJointAccountStaffs { get; set; }
        public virtual DbSet<vJointApplications> vJointApplications { get; set; }
        public virtual DbSet<vManagerScheduleMeetings> vManagerScheduleMeetings { get; set; }
        public virtual DbSet<vMarketingCompanies> vMarketingCompanies { get; set; }
        public virtual DbSet<vMeetingScheduleApplications> vMeetingScheduleApplications { get; set; }
        public virtual DbSet<vPermits> vPermits { get; set; }
        public virtual DbSet<vPhaseRoutings> vPhaseRoutings { get; set; }
        public virtual DbSet<vProcessingRules> vProcessingRules { get; set; }
        public virtual DbSet<vReceipts> vReceipts { get; set; }
        public virtual DbSet<vRequiredFiles> vRequiredFiles { get; set; }
        public virtual DbSet<vTanks> vTanks { get; set; }
        public virtual DbSet<vUserBranches> vUserBranches { get; set; }
        public virtual DbSet<vZones> vZones { get; set; }
        public virtual DbSet<zones> zones { get; set; }

      

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ATCCompanies>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.RCNumber)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<ATGs>(entity =>
            {
                entity.Property(e => e.DPROfficial).HasMaxLength(250);

                entity.Property(e => e.Parameters)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<Accounts>(entity =>
            {
                entity.Property(e => e.BankAccName).HasMaxLength(50);

                entity.Property(e => e.BankAccNo).HasMaxLength(10);

                entity.Property(e => e.BankName).HasMaxLength(50);
            });

            modelBuilder.Entity<AppDeskHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryID);

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AppPhaseDocuments>(entity =>
            {
                entity.HasKey(e => e.DocID)
                    .HasName("PK_AppStageDocuments");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ApplicationDocuments>(entity =>
            {
                entity.HasKey(e => e.AppDocID)
                    .HasName("PK_ApplicationDocuments_UT");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DocName)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.docType)
                    .IsRequired()
                    .HasMaxLength(8)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ApplicationExtraPayments>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DatePaid).HasMaxLength(150);

                entity.Property(e => e.RRR).HasMaxLength(50);

                entity.Property(e => e.Reference)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Status).HasMaxLength(50);

                entity.Property(e => e.Type).HasMaxLength(50);

                entity.Property(e => e.UserName).HasMaxLength(250);
            });

            modelBuilder.Entity<ApplicationForms>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.ExtraReport1).HasMaxLength(1000);

                entity.Property(e => e.ExtraReport2).HasMaxLength(1000);

                entity.Property(e => e.FormId)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.FormTitle).HasMaxLength(250);

                entity.Property(e => e.InspectionScheduleDate).HasColumnType("datetime");

                entity.Property(e => e.StaffName).HasMaxLength(250);
            });

            modelBuilder.Entity<ApplicationJobSpecPresentations>(entity =>
            {
                entity.Property(e => e.Job_Specification_Id).HasMaxLength(50);
            });

            modelBuilder.Entity<ApplicationProccess>(entity =>
            {
                entity.HasKey(e => e.ProccessID)
                    .HasName("PK_ApplicationProccess_1");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Division)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.Process)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ApplicationRequest>(entity =>
            {
                entity.HasKey(e => e.RequestID)
                    .HasName("PK__RequestP__33A8519A967F4C5A");

                entity.Property(e => e.AcknowledgeAt).HasColumnType("datetime");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateApplied).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.GeneratedNumber)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RequestRefNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ApplicationTanks>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.TankName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey, e.UserId })
                    .HasName("PK_dbo.AspNetUserLogins");

                entity.Property(e => e.LoginProvider).HasMaxLength(128);

                entity.Property(e => e.ProviderKey).HasMaxLength(128);

                entity.Property(e => e.UserId).HasMaxLength(128);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PK_dbo.AspNetUserRoles");

                entity.Property(e => e.UserId).HasMaxLength(128);

                entity.Property(e => e.RoleId).HasMaxLength(128);
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.Property(e => e.Id).HasMaxLength(128);

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<AuditLogs>(entity =>
            {
                entity.HasKey(e => e.AuditLogId)
                    .HasName("PK_AuditLogs_1");

                entity.Property(e => e.AuditLogId).ValueGeneratedNever();

                entity.Property(e => e.ColumnName).HasMaxLength(100);

                entity.Property(e => e.EventDateUTC).HasColumnType("datetime");

                entity.Property(e => e.EventType)
                    .HasMaxLength(1)
                    .IsFixedLength();

                entity.Property(e => e.IP).HasMaxLength(50);

                entity.Property(e => e.RecordId).HasMaxLength(100);

                entity.Property(e => e.TableName).HasMaxLength(100);

                entity.Property(e => e.UserId).HasMaxLength(50);
            });

            modelBuilder.Entity<AuditTrail>(entity =>
            {
                entity.HasKey(e => e.AuditLogID);

                entity.Property(e => e.AuditAction)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UserID)
                    .HasMaxLength(80)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Categories>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName).HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<CategoryRoutings>(entity =>
            {
                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<CompanyTechnicalAllowances>(entity =>
            {
                entity.HasKey(e => e.CompanyId);

                entity.Property(e => e.CompanyId).ValueGeneratedNever();
            });

            modelBuilder.Entity<CrudeStreams>(entity =>
            {
                entity.HasKey(e => e.Name);

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Departments>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(100);
            });

            modelBuilder.Entity<Document_Type_Applications>(entity =>
            {
                entity.Property(e => e.UniqueId).HasMaxLength(50);
            });

            modelBuilder.Entity<ExpiredScheduledMeetings>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<Facilities>(entity =>
            {
                entity.Property(e => e.CategoryCode).HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(70);

                entity.Property(e => e.ContactNumber).HasMaxLength(50);

                entity.Property(e => e.CountryName)
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.IdentificationCode).HasMaxLength(50);

                entity.Property(e => e.LGA)
                    .HasMaxLength(90)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.StateName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.address_1).HasMaxLength(100);

                entity.Property(e => e.city)
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FacilityConditions>(entity =>
            {
                entity.Property(e => e.DPRStaff)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<FacilityModifications>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.PrevProduct)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(50);
            });

            modelBuilder.Entity<FacilityPermit>(entity =>
            {
                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.DateExpired).HasColumnType("date");

                entity.Property(e => e.DateIssued).HasColumnType("date");

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PermitNo)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsFixedLength();
            });

            modelBuilder.Entity<FacilityPermits>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("FacilityPermits");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.date_expire).HasColumnType("datetime2(0)");

                entity.Property(e => e.date_issued).HasColumnType("datetime2(0)");

                entity.Property(e => e.is_renewed).HasMaxLength(130);

                entity.Property(e => e.permit_no)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<FacilityTankModifications>(entity =>
            {
                entity.Property(e => e.ModifyType).HasMaxLength(50);

                entity.Property(e => e.PrevProduct)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Type).HasMaxLength(50);
            });

            modelBuilder.Entity<FieldOfficeStates>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                //entity.has(e => e.FieldOfficeStatesId);

                //entity.HasAlternateKey(e => e.StateId);
            });

            modelBuilder.Entity<FieldOffices>(entity =>
            {
                entity.HasKey(e => e.FieldOffice_id)
                    .HasName("PK_FieldOffice");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.FieldType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeAddress)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.OfficeName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<FieldValues>(entity =>
            {
                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<Fields>(entity =>
            {
                entity.Property(e => e.CreatedByUserId).HasMaxLength(150);

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.DataType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DisplayLabel).HasMaxLength(500);

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LastModifiedByUserId).HasMaxLength(150);

                entity.Property(e => e.OptionValue).HasMaxLength(500);

                entity.Property(e => e.Validation).HasMaxLength(150);

                entity.Property(e => e.hiddenValue)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.lastModifiedOnDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Forms>(entity =>
            {
                entity.Property(e => e.CreatedByUserId).HasMaxLength(50);

                entity.Property(e => e.CreatedOnDate).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName).HasMaxLength(50);

                entity.Property(e => e.OtherPhases)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.TableName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(50);
            });

            modelBuilder.Entity<InspectionMeetingAttendees>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.StaffEmail)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<InspectionSchedules>(entity =>
            {
                entity.Property(e => e.AcceptanceDate).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DeclineReason).HasMaxLength(2000);

                entity.Property(e => e.MeetingDate).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(4000);

                entity.Property(e => e.StaffUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Venue).HasMaxLength(200);
            });

            modelBuilder.Entity<JointAccountReports>(entity =>
            {
                entity.Property(e => e.ReportDate).HasColumnType("datetime");

                entity.Property(e => e.Reportby).HasMaxLength(250);
            });

            modelBuilder.Entity<JointAccountStaffs>(entity =>
            {
                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.Staff).HasMaxLength(250);
            });

            modelBuilder.Entity<JointAccounts>(entity =>
            {
                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.Opscon)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<JointApplications>(entity =>
            {
                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.FacilityName).HasMaxLength(120);

                entity.Property(e => e.Opscon).HasMaxLength(100);

                entity.Property(e => e.PhaseName).HasMaxLength(50);

                entity.Property(e => e.Staff).HasMaxLength(100);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Leaves>(entity =>
            {
                entity.Property(e => e.DateEnd).HasColumnType("datetime");

                entity.Property(e => e.DateLogged).HasColumnType("datetime");

                entity.Property(e => e.DateStart).HasColumnType("datetime");

                entity.Property(e => e.ReasonForDisapproval).HasMaxLength(2000);
            });

            modelBuilder.Entity<Legacies>(entity =>
            {
                //entity.HasKey(e => new { e.Id, e.CompId, e.LicenseNo })
                entity.HasKey(e => new { e.Id, e.CompId})
                    .HasName("PK_Legacy_1");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.CompId).HasMaxLength(15);

                entity.Property(e => e.LicenseNo).HasMaxLength(50);

                entity.Property(e => e.AppType).HasMaxLength(50);

                entity.Property(e => e.ApprovedAt).HasColumnType("datetime");

                entity.Property(e => e.City)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CompAddress).HasMaxLength(250);

                entity.Property(e => e.CompName).HasMaxLength(150);

                entity.Property(e => e.ContactName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ContactPhone)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateUsed).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DocSource)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Exp_Date).HasMaxLength(50);

                entity.Property(e => e.FacilityAddress).HasMaxLength(250);

                entity.Property(e => e.FacilityName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.Issue_Date).HasMaxLength(50);

                entity.Property(e => e.LGA).HasMaxLength(100);

                entity.Property(e => e.LandMeters).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Products)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.State).HasMaxLength(50);

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Lgas>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.LocationName)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Logins>(entity =>
            {
                entity.HasKey(e => e.LoginID);

                entity.Property(e => e.HostName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Local_Ip)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LoginStatus)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.LoginTime).HasColumnType("datetime");

                entity.Property(e => e.LogoutTime).HasColumnType("datetime");

                entity.Property(e => e.MacAddress)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Remote_Ip)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UserAgent)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ManagerReminders>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.DateAdded).HasColumnType("datetime");
            });

            modelBuilder.Entity<ManagerScheduleMeetings>(entity =>
            {
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<ManualRemitaValues>(entity =>
            {
                entity.Property(e => e.AddedBy).HasMaxLength(250);

                entity.Property(e => e.Beneficiary).HasMaxLength(250);

                entity.Property(e => e.Company).HasMaxLength(250);

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.FundingBank).HasMaxLength(150);

                entity.Property(e => e.PaymentSource).HasMaxLength(150);

                entity.Property(e => e.RRR).HasMaxLength(15);

                entity.Property(e => e.Status).HasMaxLength(50);
            });

            modelBuilder.Entity<MarketingCompanies>(entity =>
            {
                entity.Property(e => e.ApprovedBy).HasMaxLength(250);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Reason).HasMaxLength(500);
            });

            modelBuilder.Entity<MeetingSchedules>(entity =>
            {
                entity.Property(e => e.AcceptanceDate).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.ApprovedBy).HasMaxLength(400);

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DeclineReason).HasMaxLength(2000);

                entity.Property(e => e.FinalComment).HasMaxLength(2000);

                entity.Property(e => e.MeetingDate).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(400);

                entity.Property(e => e.StaffUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.Venue).HasMaxLength(200);

                entity.Property(e => e.WaiverReason).HasMaxLength(1000);
            });

            modelBuilder.Entity<MistdoStaff>(entity =>
            {
                entity.HasKey(e => e.MistdoId);

                entity.Property(e => e.CertificateNo)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.FullName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.IssuedDate).HasColumnType("datetime");

                entity.Property(e => e.MistdoServerId)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNo)
                    .HasMaxLength(20)
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<MyDesk>(entity =>
            {
                entity.HasKey(e => e.DeskID)
                    .HasName("PK_MyDesk_UT");

                entity.Property(e => e.Comment).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Notifications>(entity =>
            {
                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DateDeleted).HasColumnType("datetime");

                entity.Property(e => e.DateRead).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(2000);

                entity.Property(e => e.ToStaff).HasMaxLength(250);
            });

            modelBuilder.Entity<OilTerminals>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.OperationalZone).HasMaxLength(50);

                entity.Property(e => e.Platform).HasMaxLength(50);

                entity.Property(e => e.Product)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.SupervisedBy).HasMaxLength(50);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<OutOfOffice>(entity =>
            {
                entity.HasKey(e => e.OutID);

                entity.Property(e => e.ApprovedBy)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.ApproverComment)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ApproverRole)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DateFrom).HasColumnType("datetime");

                entity.Property(e => e.DateTo).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<PaymentApprovals>(entity =>
            {
                entity.Property(e => e.Bank)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.Comment)
                    .HasMaxLength(450)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.PaymentId)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentUrl)
                    .IsRequired()
                    .HasMaxLength(450)
                    .IsUnicode(false);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(150)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<PhaseRoutings>(entity =>
            {
                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<Phases>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(2000);

                entity.Property(e => e.FlowType).HasMaxLength(50);

                entity.Property(e => e.IssueType).HasMaxLength(50);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ProcessingFee).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SanctionFee).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ServiceCharge).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.ShortName).HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Pumps>(entity =>
            {
                entity.Property(e => e.Manufacturer)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Model).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Version).HasMaxLength(50);
            });

            modelBuilder.Entity<Receipts>(entity =>
            {
                entity.Property(e => e.RRR).HasMaxLength(15);

                entity.Property(e => e.applicationreference)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.companyname)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.date_paid).HasColumnType("datetime");

                entity.Property(e => e.receiptno).HasMaxLength(30);

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<RemitaPaymentStatuses>(entity =>
            {
                entity.Property(e => e.OrderRef).HasMaxLength(50);

                entity.Property(e => e.PayerEmail).HasMaxLength(50);

                entity.Property(e => e.PayerName).HasMaxLength(50);

                entity.Property(e => e.PayerPhoneNumber).HasMaxLength(50);

                entity.Property(e => e.amount).HasMaxLength(50);

                entity.Property(e => e.bank).HasMaxLength(50);

                entity.Property(e => e.branch).HasMaxLength(50);

                entity.Property(e => e.channnel).HasMaxLength(50);

                entity.Property(e => e.daterequested).HasMaxLength(50);

                entity.Property(e => e.datesent).HasMaxLength(50);

                entity.Property(e => e.debitdate).HasMaxLength(50);

                entity.Property(e => e.responseCode).HasMaxLength(50);

                entity.Property(e => e.rrr).HasMaxLength(50);

                entity.Property(e => e.serviceTypeId).HasMaxLength(50);

                entity.Property(e => e.transactiondate).HasMaxLength(50);

                entity.Property(e => e.uniqueidentifier).HasMaxLength(50);
            });

            modelBuilder.Entity<Reports>(entity =>
            {
                entity.HasKey(e => e.ReportId);

                entity.Property(e => e.Comment)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DocSource)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.StaffEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<RunTimes>(entity =>
            {
                entity.Property(e => e.LastRunTime).HasColumnType("datetime");
            });

            modelBuilder.Entity<Sanctions>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.ProcessingFee).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.SanctionFee).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.StatutoryFee).HasColumnType("decimal(18, 2)");
            });

            modelBuilder.Entity<Schdules>(entity =>
            {
                entity.HasKey(e => e.SchduleID);

                entity.Property(e => e.Comment).IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.CustomerComment).IsUnicode(false);

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.SchduleDate).HasColumnType("datetime");

                entity.Property(e => e.SchduleLocation)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.SchduleType)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.SupervisorComment).IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ScheduleTransactions>(entity =>
            {
                entity.Property(e => e.ContributionType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.EmployeeAmount).HasColumnType("money");

                entity.Property(e => e.EmployerAmount).HasColumnType("money");

                entity.Property(e => e.TotalAmount).HasColumnType("money");
            });

            modelBuilder.Entity<Schedules>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Month).HasMaxLength(50);

                entity.Property(e => e.TotalAmount).HasColumnType("money");

                entity.Property(e => e.fileUrl).HasMaxLength(250);
            });

            modelBuilder.Entity<Signatories>(entity =>
            {
                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Position)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Signature)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.SignatureName)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.SignaturePath)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.StaffElpsID)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.StaffEmail)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Theme)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Staffs>(entity =>
            {
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.PhoneNo).HasMaxLength(50);

                entity.Property(e => e.Signature).HasMaxLength(250);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<States>(entity =>
            {
                entity.Property(e => e.Code).HasMaxLength(10);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<States_UT>(entity =>
            {
                entity.HasKey(e => e.State_id);

                entity.Property(e => e.Code).HasMaxLength(10);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<GeoPoliticalStates>(entity =>
            {
                entity.HasKey(e => e.GeoStateId)
                    .HasName("PK__GeoPolit__7DB2F7C061D3554B");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });
            modelBuilder.Entity<GeoPoliticalZone>(entity =>
            {
                entity.HasKey(e => e.GeoId)
                    .HasName("PK__GeoPolit__EC445C5F7B6758B6");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.GeoName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<SubmittedDocuments>(entity =>
            {
                entity.HasKey(e => e.SubDocID)
                    .HasName("PK__Submitte__ADE7CAAC122BF9FC");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.DocSource).IsUnicode(false);

                entity.Property(e => e.DocumentName)
                    .HasMaxLength(300)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<SuitabilityInspections>(entity =>
            {
                entity.Property(e => e.DistanceFromExistingStation).HasMaxLength(50);

                entity.Property(e => e.EIADPRStaff).HasMaxLength(250);

                entity.Property(e => e.SizeOfLand)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<TakeOvers>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.OldCompanyDPRId).HasMaxLength(100);
            });

            modelBuilder.Entity<TankInspections>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            modelBuilder.Entity<TankLeakTests>(entity =>
            {
                entity.Property(e => e.DateAdded).HasColumnType("datetime");
            });

            modelBuilder.Entity<Tanks>(entity =>
            {
                entity.Property(e => e.AppTank)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.MaxCapacity).HasMaxLength(50);

                entity.Property(e => e.ModifyType)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.NewTankDetails)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Position).HasMaxLength(20);

                entity.Property(e => e.ProductName).HasMaxLength(20);

                entity.Property(e => e.Status)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<TrainingPrograms>(entity =>
            {
                entity.Property(e => e.CourseTitle).HasMaxLength(250);

                entity.Property(e => e.Duration).HasMaxLength(250);

                entity.Property(e => e.Role).HasMaxLength(250);

                entity.Property(e => e.StaffName).HasMaxLength(250);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserBranches>(entity =>
            {
                entity.HasNoKey();

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<UserRoles>(entity =>
            {
                entity.HasKey(e => e.Role_id);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<Waivers>(entity =>
            {
                entity.Property(e => e.AssignedManager)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.ManagerResponseDate).HasColumnType("datetime");

                entity.Property(e => e.RequestDate).HasColumnType("datetime");

                entity.Property(e => e.RequestFrom).HasMaxLength(250);

                entity.Property(e => e.RequestReason).IsRequired();

                entity.Property(e => e.WaiverFor).HasMaxLength(50);
            });

            modelBuilder.Entity<WorkFlows>(entity =>
            {
                entity.Property(e => e.IsActive).HasDefaultValueSql("(CONVERT([bit],(0)))");
            });

            modelBuilder.Entity<WorkProccess>(entity =>
            {
                entity.HasKey(e => e.ProccessID)
                    .HasName("PK_WorkProccess_");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.ModificationStage)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<WorkRoles>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<ZonalFieldOffice>(entity =>
            {
                entity.HasKey(e => e.ZoneFieldOffice_id);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ZonalOffice>(entity =>
            {
                entity.HasKey(e => e.Zone_id);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.ZoneName)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ZoneFieldOffice>(entity =>
            {
                entity.HasKey(e => e.ZoneFieldOffice_id);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<ZoneStates>(entity =>
            {
                entity.HasKey(e => e.ZoneStates_id);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<__MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");

                entity.Property(e => e.MigrationId).HasMaxLength(150);

                entity.Property(e => e.ContextKey).HasMaxLength(300);

                entity.Property(e => e.Model).IsRequired();

                entity.Property(e => e.ProductVersion)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<addresses>(entity =>
            {
                //entity.HasOne<FieldOfficeStates>()
                //  .WithMany()
                //  .HasForeignKey(p => p.StateId)
                //  .HasPrincipalKey(p => p.StateId);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.address_2).HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.postal_code).HasMaxLength(10);
            });

            modelBuilder.Entity<application_Processings>(entity =>
            {
                entity.Property(e => e.DateProcessed).HasColumnType("datetime");

                entity.Property(e => e.Date_Assigned).HasColumnType("datetime");

                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.StaffEmail).HasMaxLength(50);
            });

            modelBuilder.Entity<application_desk_histories>(entity =>
            {
                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.comment)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.date).HasColumnType("datetime2(0)");

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<application_documents>(entity =>
            {
                entity.Property(e => e.UniqueId).HasMaxLength(15);
            });

            modelBuilder.Entity<applications>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.LastAssignedUser)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.PaymentDescription).HasMaxLength(500);

                entity.Property(e => e.TransferCost).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.current_Permit).HasMaxLength(50);

                entity.Property(e => e.date_added).HasColumnType("datetime");

                entity.Property(e => e.date_modified).HasColumnType("datetime");

                entity.Property(e => e.fee_payable).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.service_charge).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

              
            });

            modelBuilder.Entity<branches>(entity =>
            {
                 
                entity.Property(e => e.Address).HasMaxLength(250);

                entity.Property(e => e.branchCode)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.create_at).HasColumnType("datetime");

                entity.Property(e => e.lastedit_at).HasColumnType("datetime");

                entity.Property(e => e.location)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<companies>(entity =>
            {
                entity.Property(e => e.Address)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Avarta)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.City)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyCode).HasMaxLength(50);

                entity.Property(e => e.CompanyEmail)
                    .HasMaxLength(80)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.DPR_Id).HasMaxLength(50);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DeletedAt).HasColumnType("datetime");

                entity.Property(e => e.StateName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

                entity.Property(e => e.accident_report).IsUnicode(false);

                entity.Property(e => e.affiliate)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.business_type)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.contact_firstname)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.contact_lastname)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.contact_phone)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.hse).IsUnicode(false);

                entity.Property(e => e.mission_vision).IsUnicode(false);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.nationality)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.rc_number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.tin_number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.total_asset)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.training_program).IsUnicode(false);

                entity.Property(e => e.user_id)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.yearly_revenue)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_directors>(entity =>
            {
                entity.Property(e => e.firstname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.lastname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.telephone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_documents>(entity =>
            {
                entity.Property(e => e.UniqueId).HasMaxLength(15);

                entity.Property(e => e.date_added).HasColumnType("datetime2(0)");

                entity.Property(e => e.date_modified).HasColumnType("datetime2(0)");

                entity.Property(e => e.document_name).HasMaxLength(250);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.source)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_expatriate_quotas>(entity =>
            {
                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_key_staffs>(entity =>
            {
                entity.Property(e => e.designation)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.firstname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.lastname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.nationality)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.qualification)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.skills).IsUnicode(false);

                entity.Property(e => e.training_certificates).IsUnicode(false);
            });

            modelBuilder.Entity<company_medicals>(entity =>
            {
                entity.Property(e => e.address)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.email)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.medical_organisation)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_nsitfs>(entity =>
            {
                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.policy_no)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_proffessionals>(entity =>
            {
                entity.Property(e => e.cert_no)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.proffessional_organisation)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<company_technical_agreements>(entity =>
            {
                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<countries>(entity =>
            {
                entity.Property(e => e.id).ValueGeneratedNever();

                entity.Property(e => e.address_format).IsRequired();

                entity.Property(e => e.iso_code_2)
                    .IsRequired()
                    .HasMaxLength(2);

                entity.Property(e => e.iso_code_3)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<currencies>(entity =>
            {
                entity.Property(e => e.code)
                    .IsRequired()
                    .HasMaxLength(3);

                entity.Property(e => e.date_modified).HasColumnType("datetime2(0)");

                entity.Property(e => e.decimal_place)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsFixedLength();

                entity.Property(e => e.symbol_left)
                    .IsRequired()
                    .HasMaxLength(12);

                entity.Property(e => e.symbol_right)
                    .IsRequired()
                    .HasMaxLength(12);

                entity.Property(e => e.title)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.value).HasColumnType("numeric(15, 8)");
            });

            modelBuilder.Entity<document_types>(entity =>
            {
                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<faq_description>(entity =>
            {
                entity.Property(e => e.description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.locale_code)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.title)
                    .IsRequired()
                    .HasMaxLength(70)
                    .IsUnicode(false)
                    .IsFixedLength();
            });

            modelBuilder.Entity<faqs>(entity =>
            {
                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(70)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<files>(entity =>
            {
                entity.Property(e => e.mime)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.model_name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.size)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.source)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<invoices>(entity =>
            {
                entity.Property(e => e.date_added).HasColumnType("datetime");

                entity.Property(e => e.date_paid).HasColumnType("datetime");

                entity.Property(e => e.payment_code)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.payment_type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.receipt_no).HasMaxLength(20);

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<key_staff_certificates>(entity =>
            {
                entity.Property(e => e.cert_no)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.issuer).HasMaxLength(100);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<messages>(entity =>
            {
                entity.Property(e => e.UserType)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.content).IsUnicode(false);

                entity.Property(e => e.sender_id).HasMaxLength(200);

                entity.Property(e => e.subject)
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<pages>(entity =>
            {
                entity.Property(e => e.content)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.meta_description)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.meta_keyword)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<payment_transactions>(entity =>
            {
                entity.Property(e => e.PaymentSource).HasMaxLength(50);

                entity.Property(e => e.RRR).HasMaxLength(50);

                entity.Property(e => e.Webpay_Reference).HasMaxLength(50);

                entity.Property(e => e.approved_amount)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.customer_name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.online_reference)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.order_id)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.payment_log_id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.payment_reference)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.query_date).HasColumnType("datetime2(0)");

                entity.Property(e => e.reference_number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_description)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_amount)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_currency)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_date)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<permits>(entity =>
            {
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.categoryName).HasMaxLength(150);

                entity.Property(e => e.dateString)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.date_expire).HasColumnType("datetime2(0)");

                entity.Property(e => e.date_issued).HasColumnType("datetime2(0)");

                entity.Property(e => e.is_renewed).HasMaxLength(130);

                entity.Property(e => e.permit_no)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<remita_transactions>(entity =>
            {
                entity.Property(e => e.PaymentSource).HasMaxLength(50);

                entity.Property(e => e.RRR).HasMaxLength(50);

                entity.Property(e => e.Webpay_Reference).HasMaxLength(50);

                entity.Property(e => e.approved_amount)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.customer_name)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.online_reference)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.order_id)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.payment_log_id)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.payment_reference)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.query_date).HasColumnType("datetime2(0)");

                entity.Property(e => e.reference_number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_code)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_description)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_amount)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_currency)
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_date)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<reversal_transactions>(entity =>
            {
                entity.Property(e => e.approved_amount)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.customer_name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.order_id)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.original_payment_log_id)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.original_payment_reference)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.payment_log_id)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.payment_reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.query_date).HasColumnType("datetime2(0)");

                entity.Property(e => e.reference_number)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_code)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.response_description)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_amount)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_currency)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.transaction_date)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vAddresses>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vAddresses");

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Lga).HasMaxLength(50);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.address_2).HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.postal_code).HasMaxLength(10);
            });

            modelBuilder.Entity<vApplicationAddresses>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationAddresses");

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.State)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vApplicationDeskHistories>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationDeskHistories");

                entity.Property(e => e.ApplicationDate).HasColumnType("datetime");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.comment)
                    .IsRequired()
                    .IsUnicode(false);

                entity.Property(e => e.date).HasColumnType("datetime2(0)");

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vApplicationDocuments>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationDocuments");

                entity.Property(e => e.FileName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UniqueId).HasMaxLength(15);

                entity.Property(e => e.documentTypeName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.document_name).HasMaxLength(250);

                entity.Property(e => e.source)
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vApplicationForms>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationForms");

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.DateModified).HasColumnType("datetime");

                entity.Property(e => e.ExtraReport1).HasMaxLength(1000);

                entity.Property(e => e.ExtraReport2).HasMaxLength(1000);

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.FormId)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.FormTitle).HasMaxLength(250);

                entity.Property(e => e.StaffName).HasMaxLength(250);
            });

            modelBuilder.Entity<vApplicationProcessingRules>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationProcessingRules");

                entity.Property(e => e.DateProcessed).HasColumnType("datetime");

                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<vApplicationTanks>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplicationTanks");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.TankName)
                    .IsRequired()
                    .HasMaxLength(150);
            });

            modelBuilder.Entity<vApplication_Processings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplication_Processings");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DateProcessed).HasColumnType("datetime");

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.date_added).HasColumnType("datetime");

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vApplications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vApplications");

                entity.Property(e => e.CategoryCode).HasMaxLength(50);

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.FacIdentificationCode).HasMaxLength(50);

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.IssueType).HasMaxLength(50);

                entity.Property(e => e.PaymentDescription).HasMaxLength(500);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.PhaseShortName).HasMaxLength(50);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.current_Permit).HasMaxLength(50);

                entity.Property(e => e.date_added).HasColumnType("datetime");

                entity.Property(e => e.date_modified).HasColumnType("datetime");

                entity.Property(e => e.fee_payable).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.service_charge).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.user_id)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<vCategoryDocuments>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCategoryDocuments");

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCategoryRoutings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCategoryRoutings");

                entity.Property(e => e.DepartmentName).HasMaxLength(100);

                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<vCompAddressesU>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompAddressesU");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.address_2).HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.postal_code).HasMaxLength(10);
            });

            modelBuilder.Entity<vCompanies>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanies");

                entity.Property(e => e.Business_Type)
                    .IsRequired()
                    .HasMaxLength(25)
                    .IsUnicode(false);

                entity.Property(e => e.Contact_FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Contact_LastName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Contact_Phone)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Nationality)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.RC_Number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Tin_Number)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.User_Id)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<vCompanyAspUsers>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyAspUsers");

                entity.Property(e => e.AspUserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(256);

                entity.Property(e => e.LockoutEndDateUtc).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(256);
            });

            modelBuilder.Entity<vCompanyDirectors>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyDirectors");

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.PostalCode).HasMaxLength(10);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.address_2).HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.firstname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.lastname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.telephone)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyExpatriateQuotas>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyExpatriateQuotas");

                entity.Property(e => e.FileName).HasMaxLength(50);

                entity.Property(e => e.FileSource).HasMaxLength(255);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyFile2>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyFile2");

                entity.Property(e => e.DocumentTypeName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.source)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<vCompanyMedicals>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyMedicals");

                entity.Property(e => e.FileName).HasMaxLength(50);

                entity.Property(e => e.FileSource).HasMaxLength(255);

                entity.Property(e => e.address)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.email)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.medical_organisation)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.phone)
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyNsitfs>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyNsitfs");

                entity.Property(e => e.FileName).HasMaxLength(50);

                entity.Property(e => e.FileSource).HasMaxLength(255);

                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.policy_no)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyProffessionals>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyProffessionals");

                entity.Property(e => e.FileName).HasMaxLength(50);

                entity.Property(e => e.FileSource).HasMaxLength(255);

                entity.Property(e => e.cert_no)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.date_issued).HasColumnType("date");

                entity.Property(e => e.proffessional_organisation)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyTechnicalAgreements>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyTechnicalAgreements");

                entity.Property(e => e.FileName).HasMaxLength(50);

                entity.Property(e => e.FileSource).HasMaxLength(255);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vCompanyfiles>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vCompanyfiles");

                entity.Property(e => e.DocumentTypeName)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.UniqueId).HasMaxLength(15);

                entity.Property(e => e.date_added).HasColumnType("datetime2(0)");

                entity.Property(e => e.date_modified).HasColumnType("datetime2(0)");

                entity.Property(e => e.document_name).HasMaxLength(250);

                entity.Property(e => e.source)
                    .IsRequired()
                    .HasMaxLength(200)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vFacilities>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vFacilities");

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.ContactName).HasMaxLength(70);

                entity.Property(e => e.ContactNumber).HasMaxLength(50);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.IdentificationCode).HasMaxLength(50);

                entity.Property(e => e.Lga).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.contact_phone)
                    .IsRequired()
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.user_id)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<vFacilitiesWithAtleasOneApplications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vFacilitiesWithAtleasOneApplications");

                entity.Property(e => e.CategoryCode).HasMaxLength(50);

                entity.Property(e => e.ContactName).HasMaxLength(70);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.IdentificationCode).HasMaxLength(50);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.StreetAddress)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(250);
            });

            modelBuilder.Entity<vFacilityTankModifications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vFacilityTankModifications");

                entity.Property(e => e.MaxCapacity).HasMaxLength(50);

                entity.Property(e => e.ModifyType).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.PrevProduct)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<vFormFields>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vFormFields");

                entity.Property(e => e.DataType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.DisplayLabel).HasMaxLength(500);

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.OptionValue).HasMaxLength(500);

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.Validation).HasMaxLength(150);
            });

            modelBuilder.Entity<vFormValues>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vFormValues");

                entity.Property(e => e.DataType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FriendlyName).HasMaxLength(50);

                entity.Property(e => e.Label)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.Value).IsRequired();
            });

            modelBuilder.Entity<vInspectionScheduleApplications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vInspectionScheduleApplications");

                entity.Property(e => e.AcceptanceDate).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DeclineReason).HasMaxLength(2000);

                entity.Property(e => e.MeetingDate).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(4000);

                entity.Property(e => e.StaffUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Venue).HasMaxLength(200);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vJointAccountReports>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vJointAccountReports");

                entity.Property(e => e.Opscon)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.ReportDate).HasColumnType("datetime");

                entity.Property(e => e.Reportby).HasMaxLength(250);
            });

            modelBuilder.Entity<vJointAccountStaffs>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vJointAccountStaffs");

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.DateStaffAdded).HasColumnType("datetime");

                entity.Property(e => e.FirstName).HasMaxLength(50);

                entity.Property(e => e.LastName).HasMaxLength(50);

                entity.Property(e => e.Opscon)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Staff).HasMaxLength(250);
            });

            modelBuilder.Entity<vJointApplications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vJointApplications");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CountryName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.DateAdded).HasColumnType("datetime");

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Opscon)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.address_1)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.city)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.date_added).HasColumnType("datetime");

                entity.Property(e => e.date_modified).HasColumnType("datetime");

                entity.Property(e => e.fee_payable).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.service_charge).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(25);

                entity.Property(e => e.type)
                    .IsRequired()
                    .HasMaxLength(5)
                    .IsUnicode(false);

                entity.Property(e => e.user_id)
                    .IsRequired()
                    .HasMaxLength(200);
            });

            modelBuilder.Entity<vManagerScheduleMeetings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vManagerScheduleMeetings");

                entity.Property(e => e.AcceptanceDate).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.MeetingDate).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(400);

                entity.Property(e => e.StaffUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Venue).HasMaxLength(200);

                entity.Property(e => e.WaiverReason).HasMaxLength(1000);
            });

            modelBuilder.Entity<vMarketingCompanies>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vMarketingCompanies");

                entity.Property(e => e.ApprovedBy).HasMaxLength(250);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Reason).HasMaxLength(500);
            });

            modelBuilder.Entity<vMeetingScheduleApplications>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vMeetingScheduleApplications");

                entity.Property(e => e.AcceptanceDate).HasColumnType("datetime");

                entity.Property(e => e.Address).HasMaxLength(400);

                entity.Property(e => e.AppUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.ApprovedDate).HasColumnType("datetime");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.Property(e => e.MeetingDate).HasColumnType("datetime");

                entity.Property(e => e.Message).HasMaxLength(400);

                entity.Property(e => e.StaffUserName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Venue).HasMaxLength(200);

                entity.Property(e => e.companyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.reference)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vPermits>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vPermits");

                entity.Property(e => e.CategoryName).HasMaxLength(50);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.FacilityAddress)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.FacilityName)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.Fees).HasColumnType("decimal(18, 2)");

                entity.Property(e => e.IssueType).HasMaxLength(50);

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PhaseName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(e => e.StateName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.date_expire).HasColumnType("datetime2(0)");

                entity.Property(e => e.date_issued).HasColumnType("datetime2(0)");

                entity.Property(e => e.is_renewed).HasMaxLength(130);

                entity.Property(e => e.permit_no)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vPhaseRoutings>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vPhaseRoutings");

                entity.Property(e => e.DepartmentName).HasMaxLength(100);

                entity.Property(e => e.ProcessingLocation)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<vProcessingRules>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vProcessingRules");

                entity.Property(e => e.DepartmentName).HasMaxLength(100);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<vReceipts>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vReceipts");

                entity.Property(e => e.Invoice_open_date).HasColumnType("datetime");

                entity.Property(e => e.RRR).HasMaxLength(15);

                entity.Property(e => e.applicationreference)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.Property(e => e.companyname)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(e => e.date_paid).HasColumnType("datetime");

                entity.Property(e => e.payment_code)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.payment_type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.receiptno).HasMaxLength(30);

                entity.Property(e => e.status)
                    .IsRequired()
                    .HasMaxLength(10);
            });

            modelBuilder.Entity<vRequiredFiles>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vRequiredFiles");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.DocumentTypeName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<vTanks>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vTanks");

                entity.Property(e => e.FriendlyName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.MaxCapacity).HasMaxLength(50);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Position).HasMaxLength(20);

                entity.Property(e => e.ProductName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<vUserBranches>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vUserBranches");

                entity.Property(e => e.BranchName).HasMaxLength(128);

                entity.Property(e => e.DepartmentName).HasMaxLength(100);

                entity.Property(e => e.Role)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UserEmail)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.location).HasMaxLength(128);
            });

            modelBuilder.Entity<vZones>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("vZones");

                entity.Property(e => e.BranchName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.CountryName).HasMaxLength(128);

                entity.Property(e => e.code)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(128);
            });

            modelBuilder.Entity<zones>(entity =>
            {
                entity.Property(e => e.code)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.Property(e => e.name)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.status).HasDefaultValueSql("((1))");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
