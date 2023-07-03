namespace NewDepot.ViewModels
{
    public class AppSummaryViewModel
    {
        public int Id { get; set; }
        public decimal LicenseFee { get; set; }
        public decimal InspectionFee { get; set; }
        public decimal ServiceCharge { get; set; }
        public decimal Arrears { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount => LicenseFee + InspectionFee + ServiceCharge + Arrears;
    }
}