using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class Legacies
    {
        public int Id { get; set; }
        public string CompId { get; set; }
        public string LicenseNo { get; set; }
        public string CompName { get; set; }
        public string CompAddress { get; set; }
        public string FacilityAddress { get; set; }
        public string LGA { get; set; }
        public string State { get; set; }
        public double? LPG_FIG_KG { get; set; }
        public int? LPG_Tanks { get; set; }
        public double? AGOVol { get; set; }
        public int? AGO_Tanks { get; set; }
        public double? PMSVol { get; set; }
        public int? PMS_Tanks { get; set; }
        public double? DPKVol { get; set; }
        public int? DPK_Tanks { get; set; }
        public int? BitumenTanks { get; set; }
        public double? BitumenVol { get; set; }
        public int? ATKTanks { get; set; }
        public double? ATKVol { get; set; }
        public int? BaseOilTanks { get; set; }
        public double? BaseOilVol { get; set; }
        public int? LubeOilGreaseTanks { get; set; }
        public double? LubeOilGreaseVol { get; set; }
        public int? FuelOilTanks { get; set; }
        public double? FuelOilVol { get; set; }
        public string Issue_Date { get; set; }
        public string Exp_Date { get; set; }
        public int? Maj { get; set; }
        public string AppType { get; set; }
        public bool? IsUsed { get; set; }
        public DateTime? DateUsed { get; set; }
        public string Status { get; set; }
        public bool? DeleteStatus { get; set; }
        public string FacilityName { get; set; }
        public string City { get; set; }
        public decimal? LandMeters { get; set; }
        public bool? IsPipeline { get; set; }
        public bool? IsHighTension { get; set; }
        public bool? IsHighway { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Products { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? ApprovedBy { get; set; }
        public string Comment { get; set; }
        public int? AppDocId { get; set; }
        public int? CompElpsDocId { get; set; }
        public string DocSource { get; set; }
        public int? DropStaffId { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? DeletedBy { get; set; }
    }
}
