using System;
using System.Collections.Generic;

namespace NewDepot.Models
{
    public partial class RemitaPaymentStatuses
    {
        public int Id { get; set; }
        public string rrr { get; set; }
        public string channnel { get; set; }
        public string amount { get; set; }
        public string responseCode { get; set; }
        public string transactiondate { get; set; }
        public string bank { get; set; }
        public string branch { get; set; }
        public string serviceTypeId { get; set; }
        public string datesent { get; set; }
        public string daterequested { get; set; }
        public string OrderRef { get; set; }
        public string PayerName { get; set; }
        public string PayerEmail { get; set; }
        public string PayerPhoneNumber { get; set; }
        public string uniqueidentifier { get; set; }
        public string debitdate { get; set; }
    }
}
