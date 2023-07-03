using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NewDepot.Models.Stored_Procedures
{
    public class StoredProcedure : DbContext
    {

            public StoredProcedure(DbContextOptions<StoredProcedure> options)
                  : base(options)
            {
            }
            public DbSet<Payments> Payments { get; set; }
            public DbSet<MyApps_SP> AllApps { get; set; }
            public DbSet<ApplicationFieldZone> AppFieldZones { get; set; }

        }

    public class Payments
    {
        [Key]
        public int ID { get; set; }
        public int ApplicationID { get; set; }
        public string? ReferenceNo { get; set; }
        public string? FacilityAddress { get; set; }
        public string? Status { get; set; }
        public string? PaymentStatus { get; set; }
        public string? Office { get; set; }
        public string? ZonalOffice { get; set; }
        public string? Type { get; set; }
        public string? PaymentRef { get; set; }
        public string? Channel { get; set; }
        public string? Date { get; set; }
        public string? Category { get; set; }
        public string? Amount { get; set; }
        public string? CompanyName { get; set; }
        public string? FacilityName { get; set; }
        public string? ReceiptNo { get; set; }
        public decimal? Fee { get; set; }
        public decimal? service_charge { get; set; }
        public string? StateName { get; set; }
        public string? FacilityLGA { get; set; }
        public string? PaymentBreakdown { get; set; }
        public bool? DeleteStatus { get; set; }

    }
    public class MyApps_SP
    {
        [Key]
        public int appID { get; set; }
        public string? RRR { get; set; }
        public string? ApprovalType { get; set; }
        public string? Contact { get; set; }
        //public string? Products { get; set; }
        public int? CategoryId { get; set; }
       
        //public int? TanksCount { get; set; }
        //public int? ProductsCount { get; set; }
        //public decimal? Capacity { get; set; }
        public int? currentDeskID { get; set; }
        public int? OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public string? ZoneName { get; set; }
        public string? ShortName { get; set; }
        public string? FlowType { get; set; }
        public int? ZoneId { get; set; }
        public int? CompanyId { get; set; }
        public int? PhaseId { get; set; }
        public string? CategoryName { get; set; }
        public string? CompanyEmail { get; set; }
        public string? PhaseName { get; set; }
        public string? Current_Permit { get; set; }
        public string? Type { get; set; }
        public int? Year { get; set; }
        public decimal? Fee_Payable { get; set; }
        public decimal? Service_Charge { get; set; }
        public decimal? TransferCost { get; set; }
//
        public string? Status { get; set; }
        public string? Reference { get; set; }
        public DateTime? Date_Added { get; set; }
        public bool? Submitted { get; set; }
        //public string? Staff { get; set; }
        public string? CompanyName { get; set; }
        public string? FacilityName { get; set; }
        public bool? Migrated { get; set; }
        public int? FacilityId { get; set; }
        public bool? AllowPush { get; set; }
        public string? PaymentDescription { get; set; }
        public bool? AppProcessed { get; set; }
        public int? processID { get; set; }
        public bool? SupervisorProcessed { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? DeletedBy { get; set; }
        public bool? DeleteStatus { get; set; }        
        public string? FacilityAddress { get; set; }
        public string? FacilityCity { get; set; }
        public string? StateName { get; set; }
        public string? FacilityLGA { get; set; }
        public string? FacIdentificationCode { get; set; }

        public string? CategoryCode { get; set; }

     
        public DateTime? DateSubmitted { get; set; }


    }
    public class ApplicationFieldZone
    {
        [Key]
        public int id { get; set; }
        public int? OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public string? ZoneName { get; set; }
        public int? ZoneId { get; set; }
       
    }
    
}
