using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Payments
{
    public class RemitaSplit
    {
        //public string merchantId { get; set; }
        public string serviceTypeId { get; set; }
        public string totalAmount { get; set; }
        public string hash { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string orderId { get; set; }
        public List<RPartner> lineItems { get; set; }
        public string ServiceCharge { get; set; }
        public string AmountDue { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }
        public List<int> DocumentTypes { get; set; }
        public string CategoryName { get; set; }
        public List<ApplicationItem> ApplicationItems { get; set; }
    }

    public class RemitaPaymentModel
    {
        public string merchantId { get; set; }
        public string serviceTypeId { get; set; }
        public string orderId { get; set; }
        public string hash { get; set; }
        public string payerName { get; set; }
        public string payerEmail { get; set; }
        public string payerPhone { get; set; }
        public string Amount { get; set; }
        public string responseurl { get; set; }
        public string ReturnSuccessUrl { get; set; }
        public string ReturnFailureUrl { get; set; }
        public string ReturnBankPaymentUrl { get; set; }
        public List<RPartner> LineItems { get; set; }
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

    public class ApplicationItem
    {
        public string Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}