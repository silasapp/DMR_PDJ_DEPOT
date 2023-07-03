using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace NewDepot.Payments
{
    [Serializable]
    public class CustomerInformationResponse
    {
        public string MerchantReference { get; set; }

        public List<Customer> Customers { get; set; }
        public CustomerInformationResponse()
        {
            Customers = new List<Customer>();
        }


        public string SerializeToXml()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            XmlSerializer serializer = new XmlSerializer(typeof(CustomerInformationResponse));
            StringBuilder builder = new StringBuilder();
            XmlWriterSettings settings = new XmlWriterSettings();
            //settings.OmitXmlDeclaration = true;
            settings.Encoding = Encoding.UTF8;
            serializer.Serialize(System.Xml.XmlWriter.Create(builder, settings), this, ns);
            return builder.ToString();
        }
    }
    public class Customer
    {

        public string CustReference { get; set; }
        public string Status { get; set; }
        public string CustomerReferenceAlternate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string OtherName { get; set; }
        public string Amount { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

    }
}



