using DotNet.Highcharts.Options;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using Newtonsoft.Json;
using System.Globalization;
using LpgLicense.Models;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using NewDepot.Models;
using RestSharp;
using System.Threading.Tasks;
using Rotativa.AspNetCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Net;
using System.Net.Http;
using NewDepot.Helpers;
using Microsoft.EntityFrameworkCore;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Cryptography;
using System.IO;

namespace NewDepot.Helpers
{
    public class GeneralClass : Controller
    {
        RestSharpServices restSharpServices = new RestSharpServices();

        public IConfiguration _configuration;

        Helpers.Authentications auth = new Helpers.Authentications();

        public static int elpsStateID;
        private readonly Depot_DBContext _context = new Depot_DBContext();


        // private static string EncryptionKey = "1234567890";


        public static string PaymentPending = "Payment Pending";
        public static string PaymentCompleted = "Payment Completed";
        public static string MistdoRequired = "Mistdo Required";
        public static string DocumentsRequired = "Documents Required";
        public static string TanksRequired = "Tanks Required";
        public static string Move = "Move";
        public static string InspectionSchedule = "Inspection Schedule";
        public static string AutoCompletion = "Auto Completion";
        public static string Approved = "Approved";
        public static string FinalApproval = "Final Approval";
        public static string Rejected = "Rejected";
        public static string Confirmed = "Confirmed";
        public static string Processing = "Processing";
        public static string Denied = "Denied";
        public static string NoStatus = "No Status";
        public static string ProcessCompleted = "Process Completed";

        public static string Depot = "295";

        public static string NewApplication = "NEW";
        public static string RenewApplication = "RENEWAL";
        public static string SupplimentryApplication = "SUPPLEMENTARY";


        public static string START = "START";
        public static string BEGIN = "BEGIN";
        public static string NEXT = "NEXT";
        public static string END = "END";
        public static string PASS = "PASS";
        public static string DONE = "DONE";


        public static bool OPEN_STATUS = true;
        public static bool CLOSED_STATUS = false;


        public static string COMPANY = "COMPANY";
        public static string REVIEWER = "REVIEWER";
        public static string PLANNING = "PLANNING";
        public static string SECTION_HEAD = "SECTION_HEAD";
        public static string ADPDJ = "AD";
        public static string PRINTER = "Printer";
        public static string INSPECTOR = "Inspector";
        public static string SUPERVISOR = "Supervisor";
        public static string TEAMLEAD = "TeamLead";
        public static string ADOPS = "AdOps";
        public static string OPSCON = "Opscon";

        public static string ZOPCON = "ZOPCON";
        public static string SUPER_ADMIN = "Super Admin";
        public static string ADMIN = "Admin";
        public static string IT_ADMIN = "ITAdmin";
        public static string ICT_ADMIN = "ICT Admin";
        public static string DIRECTOR = "Director";
        public static string AUTHORITY = "Authority CEO";
        public static string OOD = "OOD";
        //public static string OOD = "OOC";
        public static string ED = "Executive Director";
        public static string ED_STA = "ED STA";
        public static string ACE_STA = "CEO STA";
        public static string HDS = "HDS";
        public static string IT_SUPPORT = "IT Support";
        public static string SUPPORT = "Support";

        // Sending eamil parameters
        public static string STAFF_NOTIFY = "STAFF NOTIFY";
        public static string COMPANY_NOTIFY = "COMPANY NOTIFY";

        public static string _WAITING = "WAITING";
        public static string _STARTED = "STARTED";
        public static string _FINISHED = "FINISHED";

        public static string FD = "FO";
        public static string HQ = "HQ";
        public static string HQ_FLOW = "HQ Only";
        public static string FIELD_FLOW = "Field_HQ";

        public static DateTime PortalDate = new DateTime(2021, 12, 31, 12, 0, 0); // 2020/12/31 : 12:00 pm or am
        public static DateTime PortalUpdateDate = new DateTime(2021, 10, 31, 12, 0, 0); // 2020/12/31 : 12:00 pm or am
        public static DateTime DPR_ChangeDate = new DateTime(2021, 10, 20, 12, 0, 0); // 2020/12/31 : 12:00 pm or am


        private Object lockThis = new object();
        public class ChartHelper
        {
            public Highcharts LineChart(Series[] series, List<string> category, string title, string yAxis, string chartName, string tooltip)
            {
                Highcharts chart = new Highcharts(chartName)
                        .InitChart(new Chart
                        {
                            DefaultSeriesType = ChartTypes.Line,
                            MarginRight = 130,
                            MarginBottom = 25,
                            ClassName = chartName
                        })
                        .SetTitle(new Title
                        {
                            Text = title,
                            X = -20
                        })
                        .SetSubtitle(new Subtitle
                        {
                            Text = "Source: www.depot.dpr.gov.ng",
                            X = -20
                        })
                        .SetXAxis(new XAxis { Categories = category.ToArray() })// ChartsData.Categories
                        .SetYAxis(new YAxis
                        {
                            Title = new YAxisTitle { Text = yAxis },
                            PlotLines = new[]
                                {
                                new YAxisPlotLines
                                    {
                                        Value = 0,
                                        Width = 1,
                                        Color = System.Drawing.ColorTranslator.FromHtml("#808080")
                                    }
                                }
                        })
                        .SetTooltip(new Tooltip
                        {
                            Formatter = tooltip
                        })
                        .SetLegend(new Legend
                        {
                            Layout = Layouts.Vertical,
                            Align = HorizontalAligns.Right,
                            VerticalAlign = VerticalAligns.Top,
                            X = -10,
                            Y = 100,
                            BorderWidth = 0
                        })
                        .SetSeries(series);
                return chart;
            }

            public Highcharts pieChart(object[] dt, string title, string name, string ChartName, string tooltip)
            {
                Highcharts chart = new Highcharts(ChartName)
                  .InitChart(new Chart { PlotShadow = false, PlotBackgroundColor = null, PlotBorderWidth = null })
                  .SetTitle(new Title { Text = title })
               .SetSubtitle(new Subtitle { Text = "Source: www.depot.dpr.gov.ng" })
                  .SetTooltip(new Tooltip { PointFormat = tooltip })
                  .SetPlotOptions(new PlotOptions
                  {
                      Pie = new PlotOptionsPie
                      {
                          AllowPointSelect = true,
                          Cursor = Cursors.Pointer,
                          DataLabels = new PlotOptionsPieDataLabels { Enabled = true, Color = System.Drawing.Color.White, Distance = -20, Inside = true, Format = "{y}" },
                          ShowInLegend = true
                      //Colors = chartColor

                  }
                  })
                  .SetSeries(new Series
                  {
                      Type = ChartTypes.Pie,
                      Name = name,
                      Data = new Data(dt),

                  });
                return chart;
            }

            public Highcharts DpieChart(object[] dt, string title, string name, string ChartName, System.Drawing.Color[] chartColor)
            {
                Highcharts chart = new Highcharts(ChartName)
                  .InitChart(new Chart { PlotShadow = false, PlotBackgroundColor = null, PlotBorderWidth = null })
                  .SetTitle(new Title { Text = title })
               .SetSubtitle(new Subtitle { Text = "Source: www.depot.dpr.gov.ng" })
                  .SetTooltip(new Tooltip { Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.percentage.toFixed(2) +' %'; }" })
                  .SetPlotOptions(new PlotOptions
                  {
                      Pie = new PlotOptionsPie
                      {
                          AllowPointSelect = true,
                          Cursor = Cursors.Pointer,
                          DataLabels = new PlotOptionsPieDataLabels { Enabled = true, Color = System.Drawing.Color.White, Distance = -20, Inside = true, Format = "{y}" },
                          ShowInLegend = true,
                          Colors = chartColor

                      }
                  })
                  .SetSeries(new Series
                  {
                      Type = ChartTypes.Pie,
                      Name = name,
                      Data = new Data(dt),

                  });
                return chart;
            }

            public Highcharts BarChartWithDrillDown(List<object> points, string[] categories, string name, string title, string yAxisTitle, string subTitle)
            {

                Highcharts chart = new Highcharts("chart")
                    .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
                    .SetTitle(new Title { Text = title })
                    .SetSubtitle(new Subtitle { Text = subTitle })
                    .SetXAxis(new XAxis { Categories = categories })
                    .SetYAxis(new YAxis { Title = new YAxisTitle { Text = yAxisTitle } })
                    .SetLegend(new Legend { Enabled = true })
                    .SetTooltip(new Tooltip { Formatter = "TooltipFormatter" })
                    .SetPlotOptions(new PlotOptions
                    {
                        Column = new PlotOptionsColumn
                        {
                            Cursor = Cursors.Pointer,
                        //Stacking = Stackings.Normal,
                        Point = new PlotOptionsColumnPoint
                            {

                                Events = new PlotOptionsColumnPointEvents { Click = "ColumnPointClick" }
                            },
                            DataLabels = new PlotOptionsColumnDataLabels
                            {
                                Enabled = true,
                                Color =System.Drawing.Color.FromName("colors[0]"),
                                Formatter = "function() { return this.y +'Ltr'; }",
                                Style = "fontWeight: 'bold'"
                            }
                        }
                    })
                    .SetSeries(new Series
                    {
                        Name = name,
                        Data = new Data(points.ToArray()),
                        Color = System.Drawing.Color.FromName("colors[0]")
                    })
                    .SetExporting(new Exporting { Enabled = false })
                    .AddJavascripFunction(
                        "TooltipFormatter",
                        @"var point = this.point, s = this.x +':<b>'+ this.y +'Ltr Sales</b><br/>';
                      if (point.drilldown) {
                        s += 'Click to view '+ point.category +' Branches';
                      } else {
                        s += 'Click to return to Dealers';
                      }
                      return s;"
                    )
                    .AddJavascripFunction(
                        "ColumnPointClick",
                        @"var drilldown = this.drilldown;
                      if (drilldown) { // drill down
                        setChart(drilldown.name, drilldown.categories, drilldown.data.data, drilldown.color);
                      } else { // restore
                        setChart(name, categories, data);
                      }"
                    )
                    .AddJavascripFunction(
                        "setChart",
                        @"chart.xAxis[0].setCategories(categories);
                      chart.series[0].remove();
                      chart.addSeries({
                         name: name,
                         data: data,
                         color: color || 'white'
                      });",
                        "name", "categories", "data", "color"
                    )
                    .AddJavascripVariable("colors", "Highcharts.getOptions().colors")
                    .AddJavascripVariable("name", "'{0}'".FormatWith(name))
                    .AddJavascripVariable("categories",DotNet.Highcharts.JsonSerializer.Serialize(categories))
                    .AddJavascripVariable("data", DotNet.Highcharts.JsonSerializer.Serialize(points.ToArray()));
                return chart;
            }

            public Highcharts MultiBarChart(Series[] series, List<string> category, string yAxis, string title, string chartName, string tooltip, string pointToolip)
            {

                Highcharts chart = new Highcharts(chartName)
               .InitChart(new Chart { DefaultSeriesType = ChartTypes.Column })
               .SetTitle(new Title { Text = title })
               .SetSubtitle(new Subtitle { Text = "Source: www.depot.dpr.gov.ng" })
               .SetXAxis(new XAxis { Categories = category.ToArray() })
               .SetYAxis(new YAxis
               {
                   Min = 0,
                   Title = new YAxisTitle { Text = yAxis }
               })
               .SetLegend(new Legend
               {
                   Layout = Layouts.Horizontal,
                   Align = HorizontalAligns.Center,
               })
               //.SetTooltip(new Tooltip { Formatter = @"function() { return ''+ this.x +': '+ this.y +' Ltr'; }" })
               .SetTooltip(new Tooltip
               {
                   HeaderFormat = @"<span style=""font-size:10px"">{point.key}</span><table>",
                   PointFormat = pointToolip,
                   FooterFormat = @"</table>",
                   Shared = true,
                   UseHTML = true
               })
               .SetPlotOptions(new PlotOptions
               {
                   Column = new PlotOptionsColumn
                   {
                       PointPadding = 0.2,
                       BorderWidth = 0
                   }
               })
               .SetSeries(series);
                return chart;
            }
        }
        public List<MyApps> FacWithApplication()
        {
            //List<MyApps> myApps =  ApplicationDetails_FZone();


            var facilitiesApp = (from app in _context.applications.AsEnumerable()
                        join c in _context.companies.AsEnumerable() on app.company_id equals c.id
                        join fac in _context.Facilities.AsEnumerable() on app.FacilityId equals fac.Id
                        join cat in _context.Categories.AsEnumerable() on app.category_id equals cat.id
                        join phs in _context.Phases.AsEnumerable() on app.PhaseId equals phs.id
                        join ad in _context.addresses on fac.AddressId equals ad.id
                        join sf in _context.States_UT.AsEnumerable() on ad.StateId equals sf.State_id
                        where app.DeleteStatus != true && c.DeleteStatus != true && app.submitted == true
                        select new MyApps
                        {
                            fac=fac,
                            Address_1 = ad.address_1,
                            Date_Added = Convert.ToDateTime(app.date_added),
                            StateName = sf.StateName,
                            City = ad.city,
                            LGA = ad.LgaId > 0 ? _context.Lgas.Where(l => l.Id == ad.LgaId).FirstOrDefault().Name : ad.city,
                            FacilityName = fac.Name,
                        });
            return facilitiesApp.Distinct().ToList();
            

        }
        public static string NumberToWords(double n, string type = "Naira")
        {
            string words = "";
            double intPart;
            double decPart = 0;
            if (n == 0)
                return "Zero";
            string zroP = "";
            try
            {
                n = Math.Round(n, 2);
                string[] splitter = n.ToString("N2").Split('.');
                intPart = double.Parse(splitter[0]);

                decPart = double.Parse(splitter[1]);
            }
            catch
            {
                intPart = n;
            }

            words = NumWords(intPart) + " " + type;

            if (decPart > 0)
            {

                //int counter = decPart.ToString().Length;

                words += ", " + NumWords(decPart) + " " + type == "Naira" ? "Kobo" : "";
                //switch (counter)
                //{
                //    case 1: words += NumWords(decPart) ; break;
                //    case 2: words += NumWords(decPart) ; break;
                //    case 3: words += NumWords(decPart) ; break;
                //    case 4: words += NumWords(decPart) ; break;
                //    case 5: words += NumWords(decPart) ; break;
                //    case 6: words += NumWords(decPart) ; break;
                //    case 7: words += NumWords(decPart) ; break;
                //}
            }
            return words + ".";
        }
        public static String NumWords(double n) //converts double to words
        {
            string[] numbersArr = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensArr = new string[] { "Twenty", "Thirty", "Fourty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
            string[] suffixesArr = new string[] { "Thousand", "Million", "Billion", "Trillion", "Quadrillion", "Quintillion", "Sextillion", "Septillion", "Octillion", "Nonillion", "Decillion", "Undecillion", "Duodecillion", "Tredecillion", "Quattuordecillion", "Quindecillion", "Sexdecillion", "Septdecillion", "Octodecillion", "Novemdecillion", "Vigintillion" };
            string words = "";

            bool tens = false;

            if (n < 0)
            {
                words += "Negative ";
                n *= -1;
            }

            int power = (suffixesArr.Length + 1) * 3;

            while (power > 3)
            {
                double pow = Math.Pow(10, power);
                if (n >= pow)
                {
                    if (n % pow > 0)
                    {
                        words += NumWords(Math.Floor(n / pow)) + " " + suffixesArr[(power / 3) - 1] + ", ";
                    }
                    else if (n % pow == 0)
                    {
                        words += NumWords(Math.Floor(n / pow)) + " " + suffixesArr[(power / 3) - 1];
                    }
                    n %= pow;
                }
                power -= 3;
            }
            if (n >= 1000)
            {
                if (n % 1000 > 0) words += NumWords(Math.Floor(n / 1000)) + " Thousand, ";
                else words += NumWords(Math.Floor(n / 1000)) + " Thousand";
                n %= 1000;
            }
            if (0 <= n && n <= 999)
            {
                if ((int)n / 100 > 0)
                {
                    words += NumWords(Math.Floor(n / 100)) + " Hundred";
                    n %= 100;
                    if (n > 0)
                        words += " and";
                }
                if ((int)n / 10 > 1)
                {
                    if (words != "")
                        words += " ";
                    words += tensArr[(int)n / 10 - 2];
                    tens = true;
                    n %= 10;
                }

                if (n < 20 && n > 0)
                {
                    if (words != "" && tens == false)
                        words += " ";
                    words += (tens ? "-" + numbersArr[(int)n - 1] : numbersArr[(int)n - 1]);
                    n -= Math.Floor(n);
                }
            }

            return words;
        }

        public class ProductOrder
        {
            public string product { get; set; }
            public int Order { get; set; }

            public static List<ProductOrder> GetOrdered()
            {
                var po = new List<ProductOrder>();
                po.Add(new ProductOrder { Order = 1, product = "PMS" });
                po.Add(new ProductOrder { Order = 2, product = "ATK" });
                po.Add(new ProductOrder { Order = 3, product = "DPK" });
                po.Add(new ProductOrder { Order = 4, product = "AGO" });
                po.Add(new ProductOrder { Order = 5, product = "LPFO" });
                po.Add(new ProductOrder { Order = 6, product = "HPFO" });
                po.Add(new ProductOrder { Order = 7, product = "BASE OIL" });
                po.Add(new ProductOrder { Order = 8, product = "BITUMEN" });

                po.Add(new ProductOrder { Order = 9, product = "LPG" });
                po.Add(new ProductOrder { Order = 10, product = "Lube Oil/Grease" });
                po.Add(new ProductOrder { Order = 11, product = "Fuel Oil" });
                po.Add(new ProductOrder { Order = 12, product = "LNG" });
                po.Add(new ProductOrder { Order = 13, product = "HHK" });
                po.Add(new ProductOrder { Order = 14, product = "SLOP" });
                return po;
            }
        }
        public string Encrypt(string clearText)
        {
            try
            {
                byte[] b = ASCIIEncoding.ASCII.GetBytes(clearText);
                string crypt = Convert.ToBase64String(b);
                byte[] c = ASCIIEncoding.ASCII.GetBytes(crypt);
                string encrypt = Convert.ToBase64String(c);

                return encrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                byte[] b;
                byte[] c;
                string decrypt;
                b = Convert.FromBase64String(cipherText);
                string crypt = ASCIIEncoding.ASCII.GetString(b);
                c = Convert.FromBase64String(crypt);
                decrypt = ASCIIEncoding.ASCII.GetString(c);

                return decrypt;
            }
            catch (Exception ex)
            {
                return "Error";
                throw ex;
            }

        }

        public string Encrypty(string clearText, string key)
        {
            string EncryptionKey = key;
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public string Decrypty(string cipherText, string key)
        {

            string EncryptionKey = key;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static class CalendarHelper
        {
            public static int Index(DayOfWeek dayOfWeek)
            {
                switch (dayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        return 1;
                    case DayOfWeek.Monday:
                        return 2;
                    case DayOfWeek.Tuesday:
                        return 3;
                    case DayOfWeek.Wednesday:
                        return 4;
                    case DayOfWeek.Thursday:
                        return 5;
                    case DayOfWeek.Friday:
                        return 6;
                    default:
                        return 7;
                }
            }

            public static string DayOfTheWeek(DayOfWeek dayOfWeek, bool shortDay = false)
            {
                switch (dayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        return shortDay ? "Sun" : "Sunday";
                    case DayOfWeek.Monday:
                        return shortDay ? "Mon" : "Monday";
                    case DayOfWeek.Tuesday:
                        return shortDay ? "Tue" : "Tuesday";
                    case DayOfWeek.Wednesday:
                        return shortDay ? "Wed" : "Wednesday";
                    case DayOfWeek.Thursday:
                        return shortDay ? "Thur" : "Thursday";
                    case DayOfWeek.Friday:
                        return shortDay ? "Fri" : "Friday";
                    default:
                        return shortDay ? "Sat" : "Saturday";
                }
            }

            public static string MonthOfTheYear(int month, bool shortMonth = false)
            {
                switch (month)
                {
                    case 1:
                        return shortMonth ? "Jan" : "JANUARY";
                    case 2:
                        return shortMonth ? "Feb" : "FEBRUARY";
                    case 3:
                        return shortMonth ? "Mar" : "MARCH";
                    case 4:
                        return shortMonth ? "Apr" : "APRIL";
                    case 5:
                        return shortMonth ? "May" : "MAY";
                    case 6:
                        return shortMonth ? "Jun" : "JUNE";
                    case 7:
                        return shortMonth ? "Jul" : "JULY";
                    case 8:
                        return shortMonth ? "Aug" : "AUGUST";
                    case 9:
                        return shortMonth ? "Sep" : "SEPTEMBER";
                    case 10:
                        return shortMonth ? "Oct" : "OCTOBER";
                    case 11:
                        return shortMonth ? "Nov" : "NOVEMBER";
                    default:
                        return shortMonth ? "Dec" : "DECEMBER";
                }
            }

            public static string PrevMonth(int month)
            {
                switch (month)
                {
                    case 2:
                        return "JANUARY";
                    case 3:
                        return "FEBRUARY";
                    case 4:
                        return "MARCH";
                    case 5:
                        return "APRIL";
                    case 6:
                        return "MAY";
                    case 7:
                        return "JUNE";
                    case 8:
                        return "JULY";
                    case 9:
                        return "AUGUST";
                    case 10:
                        return "SEPTEMBER";
                    case 11:
                        return "OCTOBER";
                    case 12:
                        return "NOVEMBER";
                    default:
                        return "DECEMBER";
                }
            }

            public static string NextMonth(int month)
            {
                switch (month)
                {
                    case 1:
                        return "FEBRUARY";
                    case 2:
                        return "MARCH";
                    case 3:
                        return "APRIL";
                    case 4:
                        return "MAY";
                    case 5:
                        return "JUNE";
                    case 6:
                        return "JULY";
                    case 7:
                        return "AUGUST";
                    case 8:
                        return "SEPTEMBER";
                    case 9:
                        return "OCTOBER";
                    case 10:
                        return "NOVEMBER";
                    case 11:
                        return "DECEMBER";
                    default:
                        return "JANUARY";
                }
            }

        }
        public List<Phases> GetAppCategory()
        {
            var _context = new Depot_DBContext();
            var phases = _context.Phases.Where(p => p.ShortName == "ATC" || p.ShortName == "SI");
            return (phases.ToList());
        }
        public static class ElapseTime
        {
            public static string GetElapse(DateTime time)
            {
                var diff = DateTime.Now - time;
             
                if (diff.Days >= 7)
                {
                    var weeks = (int)(diff.Days / 7);

                    if (weeks > 1)
                    {
                        return weeks.ToString() + " weeks ago";
                    }
                    else // if (weeks == 1)
                    {
                        return weeks.ToString() + " week ago";
                    }
                }
                else if (diff.Days >= 1)
                {
                    if (diff.Days > 1)
                    {
                        return diff.Days.ToString() + " days ago";
                    }
                    else //if (diff.Days == 1)
                    {
                        return "Yesterday";
                    }
                }
                else if (diff.Hours >= 1)
                {
                    if (diff.Hours > 1)
                    {
                        return diff.Hours.ToString() + " Hours ago";
                    }
                    else //if (diff.Hours == 1)
                    {
                        return diff.Hours.ToString() + " Hour ago";
                    }
                }
                else
                {
                    return diff.Minutes.ToString() + " Minutes ago";
                }

            }

            public static string ShortDate(DateTime date, bool useShort = true)
            {
               
                var mth = CalendarHelper.MonthOfTheYear(date.Month, useShort);
                var day = CalendarHelper.DayOfTheWeek(date.DayOfWeek, useShort);

                return day + ", " + date.Day + " " + mth + ", " + date.Year;

            }
        }

        public JsonResult RestResult(string url, string method, List<ParameterData> parameterData = null, object app_object = null, string output = null, string endUrl = null)
        {
            AppModels appModel = new AppModels();

            var response = restSharpServices.Response("/api/" + url + "/{email}/{apiHash}" + endUrl, parameterData, method, app_object);

            if (response.ErrorException != null)
            {
                return Json("Network Error");
            }
            else
            {
                if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(output);
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(JsonConvert.SerializeObject(response.Content));
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
            }
        }
        public JsonResult RestResult2(string url, string method, List<ParameterData> parameterData = null, object app_object = null, string output = null, string endUrl = null)
        {
            AppModels appModel = new AppModels();

            var response = restSharpServices.Response("/api/" + url + "/{email}/{apiHash}" + endUrl, parameterData, method, app_object);

            if (response.ErrorException != null)
            {
                return Json("Network Error");
            }
            else
            {
                if (method == "POST" || method == "PUT" || method == "DELETE")
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(output);
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(response.Content))
                    {
                        return Json(response.Content);
                    }
                    else
                    {
                        return Json("Opps... an error occured, please try again. " + response.ErrorMessage);
                    }
                }
            }
        }

    

        public JsonResult GetLGAs(string State)
        {
            var LGAs = _context.Lgas.Where(x => x.StateId == Convert.ToInt16(State)).ToList();

            return Json(LGAs);
        }   
        
        public int GetStatesFromCountry(string State)
        {
            var paramData2 = restSharpServices.parameterData("Id", "156");
            var response2 = restSharpServices.Response("/api/Address/states/{Id}/{email}/{apiHash}", paramData2); // GET
            var stateName = _context.States_UT.Where(x => x.State_id == Convert.ToInt32(State)).FirstOrDefault();
            var res2 = JsonConvert.DeserializeObject<List<LpgLicense.Models.State>>(response2.Content);

            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string state = textInfo.ToTitleCase(stateName.StateName.ToLower());

            foreach (var s in res2)
            {
                if (s.Name.Contains(state))
                {
                    elpsStateID = s.Id;
                    break;
                }
            }

            return elpsStateID;
        }




        //Generating Application number
        public string Generate_Application_Number()
        {
            lock (lockThis)
            {
                Thread.Sleep(1000);
                return Depot + DateTime.Now.ToString("MMddyyHHmmss");
            }
        }


        public string Generate_Receipt_Number()
        {
            lock (lockThis)
            {
                Thread.Sleep(1000);
                return DateTime.Now.ToString("MMddyyHHmmss");
            }
        }
        //Get Document Types
        public List<Document_Type> GetDocumentTypes()
        {
            var compDocs = new List<Document_Type>();
            IRestResponse response;
            
            response = restSharpServices.Response("/api/Documents/Types/{email}/{apiHash}"); // GET
             
            if (response.IsSuccessful == false)
            {
                compDocs = null;
            }
            else
            {
                compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(response.Content);
            }
            
            return compDocs;
        }

        public List<Document_Type> GetElpsFacDocumentsTypes()
        {
            var compDocs = new List<Document_Type>();
            IRestResponse response;

            var paramData = restSharpServices.parameterData("Type", "facility");
            
            response = restSharpServices.Response("/api/Documents/Facility/{email}/{apiHash}/{Type}", paramData); // GET
           

            if (response.IsSuccessful == false)
            {
                compDocs = null;
            }
            else
            {
                compDocs = JsonConvert.DeserializeObject<List<Document_Type>>(response.Content);
            }
            return compDocs;

        }

        public List<Document> getCompanyDocuments(string companyID)
        {
            List<Document> documents = new List<Document>();

            var paramData = restSharpServices.parameterData("id", companyID);
            var response = restSharpServices.Response("/api/CompanyDocuments/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<List<Document>>(response.Content);
            }
            return documents;
        }



        // Get Company Documents
        public List<Company_Document> GetCompanyDocuments(string companyID)
        {
            List<Company_Document> documents = new List<Company_Document>();

            var paramData = restSharpServices.parameterData("id", companyID);
            var response = restSharpServices.Response("/api/CompanyDocuments/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<List<Company_Document>>(response.Content);
            }
            return documents;
        }



        /*
         * Decrypting all ID
         * 
         * ids => encrypted ids
         */
        public int DecryptIDs(string ids)
        {
            int id = 0;
            var ID = this.Decrypt(ids);

            if (ID == "Error")
            {
                id = 0;
            }
            else
            {
                id = Convert.ToInt32(ID);
            }

            return id;
        }


        /*
         * Getting single company document
         */
        public Document GetCompanyDocument(string compElpsDocID)
        {
            Document documents = new Document();

            var paramData = restSharpServices.parameterData("id", compElpsDocID);
            var response = restSharpServices.Response("/api/CompanyDocument/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                documents = null;
            }
            else
            {
                documents = JsonConvert.DeserializeObject<Document>(response.Content);
            }
            return documents;
        }


        /*
         * Getting single Facility document
         */
        public FacilityDocument getFacilityDocument(string FacilityDocID)
        {
            FacilityDocument facilityDocuments = new FacilityDocument();

            var paramData = restSharpServices.parameterData("id", FacilityDocID);
            var response = restSharpServices.Response("/api/FacilityFiles/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                facilityDocuments = null;
            }
            else
            {
                facilityDocuments = JsonConvert.DeserializeObject<FacilityDocument>(response.Content);
            }

            return facilityDocuments;
        }



        // Get Facility Documents

        public List<FacilityDocument> getFacilityDocuments(string facilityID)
        {
            List<FacilityDocument> facilityDocuments = new List<FacilityDocument>();

            var paramData = restSharpServices.parameterData("id", facilityID);
            var response = restSharpServices.Response("/api/FacilityFiles/{id}/{email}/{apiHash}", paramData); // GET

            if (response.IsSuccessful == false)
            {
                facilityDocuments = null;
            }
            else
            {
                facilityDocuments = JsonConvert.DeserializeObject<List<FacilityDocument>>(response.Content);
            }

            return facilityDocuments;
        }


        public string GetStateShortName(string state, string code)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>()
            {
                {"Abia", "AB" },
                {"Adamawa", "AD" },
                {"Akwa Ibom", "AK" },
                {"Anambra", "AN" },
                {"Bauchi", "BA" },
                {"Bayelsa", "BY" },
                {"Benue", "BE" },
                {"Borno", "BO" },
                {"Cross River", "CR" },
                {"Delta", "DE" },
                {"Ebonyi", "EB" },
                {"Edo", "ED" },
                {"Enugu", "EN" },
                {"Federal Capital Territory", "FC" },
                {"Abuja", "FC" },
                {"Gombe", "GO" },
                {"Imo", "IM" },
                {"Jigawa", "JI" },
                {"Kaduna", "KD" },
                {"Kano", "KN" },
                {"Katsina", "KT" },
                {"Kebbi", "KE" },
                {"Kogi", "KO" },
                {"Kwara", "KW" },
                {"Lagos", "LA" },
                {"Nasarawa", "NA" },
                {"Niger", "NI" },
                {"Ogun", "OG" },
                {"Ondo", "ON" },
                {"Osun", "OS" },
                {"Oyo", "OY" },
                {"Plateau", "PL" },
                {"Rivers", "RI" },
                {"Sokoto", "SO" },
                {"Taraba", "TA" },
                {"Yobe", "YO" },
                {"Zamfara", "ZA" },
            };
            var shortState = pairs.Where(x => x.Key.ToUpper() == state.ToUpper().Trim()).FirstOrDefault();
            var CompanyCode = "DEP-" + code.Trim() + "-" + shortState.Value;
            return CompanyCode;
        }


    }
    
}



