using System;
using NewDepot.Models;

namespace NewDepot.ViewModels
{
    public class AppointmentReportViewModel
    {
        public int AppointmentId { get; set; }
        public string UploadedDoc { get; set; }
        public string StaffList { get; set; }
        public string InspectorComment { get; set; }
        public string SupervisorComment { get; set; }
        public DateTime SubmittedDate { get; set; }
        public string InspectionBy { get; set; }
        
        public string PlantManagerName { get; set; }
        public string FireFightingFacilities { get; set; }
        public string PowerSource { get; set; }
        public string FirstAidBox { get; set; }
        public string MechanicalLeakTester { get; set; }
        public string GasDetector { get; set; }
        public string CylinderMaintenanceFacilities { get; set; }
        public string AdequateSafetyFacilities { get; set; }
        public string ProtectiveWears { get; set; }
        public string EarthingProtectiveDevices { get; set; }
        public string EmergencyShutDownSystem { get; set; }
        public string PerimeterFence { get; set; }
        public string TrainingForOperators { get; set; }
        public string LicenseType { get; set; }
        public string ApplicationType { get; set; }
        public string DecantingSystem { get; set; }
        public string ReferenceScale { get; set; }
        public string AdjourningProperties { get; set; }
        public string SetBackAccess { get; set; }
        public string TransversePipeline { get; set; }
        public string AnyOverheadTension { get; set; }
        public string SiteAccessibility { get; set; }
        public string LandSize { get; set; }
        public string AdequateHouseKeeping { get; set; }
        public string ProvisionOfWater { get; set; }
        public string ManufacturerDataSheet { get; set; }
        public string WeightMeasures { get; set; }
        public string FireAlarmSystem { get; set; }
        public string MusterPoint { get; set; }
        public string SafetyWarningSigns { get; set; }
        public string EmergencyNumbers { get; set; }
        public string PumpsNumber { get; set; }
        public string WaterSprinklersonStorageTanks { get; set; }
        public string NumberofRefillingPoint { get; set; }
        public string PublicToiletFacilityandCarParkingAreas { get; set; }
        public string FunctionalGauges { get; set; }
        public string AdjourningFeatures { get; set; }
        public string DistanceToPublicBuilding { get; set; }
        public string AnnualLPGPlantQuantityDispensed { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string TotalStorageCapacity { get; set; }
        public string ThirdPartyName { get; set; }
        public string ThirdPartyCompanyName { get; set; }
        public string DistancefromStorageTank { get; set; }
        public string DistancetoFourfances { get; set; }
        public string DistancetoFillingShed { get; set; }
        public string DistancetoBuilding { get; set; }
        public string DistancefromTanktoPublicBuilding { get; set; }
        public string DistancetoLPGFillingPlant { get; set; }
        public string TypeAndNatureOfNeighbouringBuilding { get; set; }
        public Appointment Appointment { get; set; }
        public string ReceiptNumber { get; set; }
        public string ReceiptIssuedDate { get; set; }
        public string FeesPaid { get; set; }
    }
}