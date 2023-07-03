using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewDepot.Payments
{
    [Serializable]
    public class CustomerInformationRequest
    {
        public string MerchantReference { get; set; }
        public string CustReference { get; set; }
        public string PaymentItemCode { get; set; }
    }
}