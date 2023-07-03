//using NewDepot.Models;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;

//namespace NewDepot.Helpers
//{
//    public class CategoryAService
//    {
//        public static string _baseUrl = "";
//        private readonly HttpClient _httpClient;
//        public ElpsServices elpsServices = new ElpsServices();



//        public CategoryAService(HttpClient httpClient)
//        {
//            _httpClient = httpClient;
//        }



//        private string URL(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            if(category == "CATEGORY A")
//            {
//                _baseUrl = "https://lpgstorage.dpr.gov.ng/LNGWebApi/";
//            }
//            else if(category == "CNG DOWNLOADING")
//            {
//                _baseUrl = "https://cngdownloading.dpr.gov.ng/LNGWebApi/";
//            }
//            else if(category == "CNG REFUELLING")
//            {
//                _baseUrl = "https://cngrefueling.dpr.gov.ng/LNGApi/";
//            }

//            var url = _baseUrl + prefix;

//            if (hasRefNo == true)
//            {
//                url = url + "?AppLicationRefNo=" + refNo;
//            }

//            return url;
//        }



//        private async Task<JObject> GetResults(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            string url = URL(prefix, hasRefNo, category, refNo);

//            HttpResponseMessage response = await _httpClient.GetAsync(url);

//            if (response.IsSuccessStatusCode == true)
//            {
//                string responseBody = await response.Content.ReadAsStringAsync();
//                var resultObject = JsonConvert.DeserializeObject<JObject>(responseBody.ToString());
//                return resultObject;
//            }
//            else
//            {
//                return null;
//            }
//        }



//        public async Task<List<AllApplications>> GetAllApplication(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            List<AllApplications> apps = new List<AllApplications>();

//            if (itemObject == null)
//            {
//                apps = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        apps.Add(new AllApplications
//                        {
//                            ApplicationID = d.SelectToken("ApplicationID").ToString(),
//                            ApplicantName = d.SelectToken("ApplicantName").ToString(),
//                            CompanyUserId = d.SelectToken("CompanyUserId").ToString(),
//                            ApplicationType = d.SelectToken("ApplicationType").ToString(),
//                            LicenseTypeCode = d.SelectToken("LicenseTypeCode").ToString(),
//                            Status = d.SelectToken("Status").ToString(),
//                            IsLegacy = d.SelectToken("IsLegacy").ToString(),
//                        });
//                    }
//                }
//                else
//                {
//                    apps = null;
//                }
//            }

//            return apps.ToList();
//        }



//        public async Task<CompanyDetails> GetCompanyDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            CompanyDetails com = new CompanyDetails();

//            if (itemObject == null)
//            {
//                com = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        com = new CompanyDetails
//                        {
//                            CompanyElpsID = Convert.ToInt32(d.SelectToken("CompanyElpsID").ToString()),
//                            CompanyName = d.SelectToken("CompanyName").ToString(),
//                            CompanyEmail = d.SelectToken("CompanyEmail").ToString(),
//                            Address = d.SelectToken("Address").ToString(),
//                            City = d.SelectToken("City").ToString(),
//                            StateName = d.SelectToken("StateName").ToString(),
//                        };
//                    }
//                }
//                else
//                {
//                    com = null;
//                }
//            }

//            return com;
//        }



//        public async Task<FacilityDetails> GetFacilityDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            FacilityDetails fac = new FacilityDetails();

//            if (itemObject == null)
//            {
//                fac = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        fac = new FacilityDetails
//                        {
//                            ElpsFacilityID = Convert.ToInt32(d?.SelectToken("ElpsFacilityID").ToString()),
//                            FacilityName = d?.SelectToken("FacilityName").ToString().Trim(),
//                            FacilityAddress = d?.SelectToken("FacilityAddress").ToString(),
//                            LGA = d?.SelectToken("LGA").ToString(),
//                            City = d?.SelectToken("City").ToString(),
//                            StateName = d?.SelectToken("StateName").ToString(),
//                            ContactPersonName = d?.SelectToken("ContactPersonName").ToString(),
//                            ContactPersonNo = d?.SelectToken("ContactPersonNo").ToString(),
//                        };
//                    }
//                }
//                else
//                {
//                    fac = null;
//                }
//            }

//            return fac;
//        }



//        public async Task<AppDetails> GetApplicationDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            AppDetails app = new AppDetails();

//            if (itemObject == null)
//            {
//                app = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var d = itemObject.SelectToken("data");

//                    app = new AppDetails
//                    {
//                        RefNo = d.SelectToken("RefNo").ToString(),
//                        DateSubmitted = d.SelectToken("DateSubmitted").ToString(),
//                        CurrentStaffDesk = d.SelectToken("CurrentStaffDesk").ToString(),
//                        HasSubmitted = d.SelectToken("HasSubmitted").ToString(),
//                        Location = d.SelectToken("Location").ToString(),
//                        FlowType = d.SelectToken("FlowType").ToString(),
//                        Status = d.SelectToken("Status").ToString(),
//                        AppType = d.SelectToken("AppType").ToString(),
//                        AppStage = d.SelectToken("AppStage").ToString(),
//                        CategoryType = d.SelectToken("CategoryType").ToString(),
//                    };
//                }
//                else
//                {
//                    app = null;
//                }
//            }

//            return app;
//        }



//        public async Task<List<TankDetails>> GetTankDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            List<TankDetails> tanks = new List<TankDetails>();

//            if (itemObject == null)
//            {
//                tanks = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        tanks.Add(new TankDetails
//                        {
//                            TankName = d.SelectToken("TankName").ToString(),
//                            Capacity = Convert.ToDecimal(d.SelectToken("Capacity").ToString()),
//                        });
//                    }

//                }
//            }

//            return tanks.ToList();
//        }



//        public async Task<ScheduleDetails> GetScheduleDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            ScheduleDetails objects = new ScheduleDetails();

//            if (itemObject == null)
//            {
//                objects = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        objects = new ScheduleDetails
//                        {
//                            SchedulerStaffName = d.SelectToken("SchedulerStaffName").ToString(),
//                            ScheduleDate = d.SelectToken("ScheduleDate").ToString(),
//                            SupervisorStaffName = d.SelectToken("SupervisorStaffName").ToString(),
//                            InspectionType = d.SelectToken("InspectionType").ToString(),
//                            SupervisorApproved = Convert.ToInt32(d.SelectToken("SupervisorApproved").ToString()),
//                            CustomerAccepted = Convert.ToInt32(d.SelectToken("CustomerAccepted").ToString()),
//                            CustomerComment = d.SelectToken("CustomerComment").ToString(),
//                            SupervisorComment = d.SelectToken("SupervisorComment").ToString(),
//                            IsDone = d.SelectToken("IsDone").ToString(),
//                        };
//                    }
//                }
//            }

//            return objects;
//        }



//        public async Task<PermitDetails> GetPermitDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            PermitDetails objects = new PermitDetails();

//            if (itemObject == null)
//            {
//                objects = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        objects = new PermitDetails
//                        {
//                            PermitNo = d.SelectToken("PermitNo").ToString(),
//                            IssuedDate = d.SelectToken("IssuedDate").ToString(),
//                            ExpiryDate = d.SelectToken("ExpiryDate").ToString(),
//                            ElpsPermitID = Convert.ToInt32(d.SelectToken("ElpsPermitID").ToString()),
//                        };
//                    }
//                }
//            }

//            return objects;
//        }



//        public async Task<List<SubmittedDocDetails>> GetSubmittedDocDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            List<SubmittedDocDetails> objects = new List<SubmittedDocDetails>();

//            if (itemObject == null)
//            {
//                objects = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        objects.Add(new SubmittedDocDetails
//                        {
//                            DocName = d.SelectToken("DocName").ToString(),
//                            DocSource = d.SelectToken("DocSource").ToString(),
//                            CompanyElpsDocID = Convert.ToInt32(d.SelectToken("CompanyElpsDocID").ToString()),
//                        });
//                    }
//                }
//            }

//            return objects;
//        }



//        public async Task<PaymentDetails> GetPaymentDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            PaymentDetails objects = new PaymentDetails();

//            if (itemObject == null)
//            {
//                objects = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        objects = new PaymentDetails
//                        {
//                            RRR = d.SelectToken("RRR").ToString(),
//                            TransactionDate = d.SelectToken("TransactionDate").ToString(),
//                            TransactionID = d.SelectToken("TransactionID").ToString(),
//                            TransactionStatus = d.SelectToken("TransactionStatus").ToString(),
//                            AmountPaid = Convert.ToInt32(d.SelectToken("AmountPaid").ToString()),
//                            TotalAmount = Convert.ToInt32(d.SelectToken("TotalAmount").ToString()),
//                        };
//                    }
//                }
//            }

//            return objects;
//        }



//        public async Task<List<HistoryDetails>> GetHistoryDetails(string prefix, bool hasRefNo, string category, string refNo = null)
//        {
//            var itemObject = await GetResults(prefix, hasRefNo, category, refNo);

//            List<HistoryDetails> objects = new List<HistoryDetails>();

//            if (itemObject == null)
//            {
//                objects = null;
//            }
//            else
//            {
//                var _message = itemObject.SelectToken("message");

//                if (_message.ToString() == "Success")
//                {
//                    var data = itemObject.SelectToken("data");

//                    foreach (var d in data.ToList())
//                    {
//                        objects.Add(new HistoryDetails
//                        {
//                            TriggeredBy = d.SelectToken("TriggeredBy").ToString(),
//                            TargetedTo = d.SelectToken("TargetedTo").ToString(),
//                            Action = d.SelectToken("Action").ToString(),
//                            Comment = d.SelectToken("Comment").ToString(),
//                            HistoryDate = d.SelectToken("HistoryDate").ToString(),
//                        });
//                    }
//                }
//            }

//            return objects;
//        }

//    }
//}
