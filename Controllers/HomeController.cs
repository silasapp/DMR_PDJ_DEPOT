using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NewDepot.Models;
using NewDepot.Helpers;
using System.IO;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
//using NewDepot.Payments;

using System.Transactions;
using Rotativa;
using System.Text;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NewDepot.Controllers;
using Microsoft.AspNetCore.Http;
using LpgLicense.Models;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Controllers.Authentications;
using static NewDepot.Models.Payment;
using Microsoft.AspNetCore.Hosting.Server;
using System.Globalization;
using System.Drawing;
using QRCoder;

namespace NewDepot.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly Depot_DBContext _context;

    RestSharpServices _restService = new RestSharpServices();

    public IConfiguration _configuration;

    ElpsResponse elpsResponse = new ElpsResponse();
    Helpers.Authentications auth = new Helpers.Authentications();
    GeneralClass generalClass = new GeneralClass();
    IHttpContextAccessor _httpContextAccessor;
    HelpersController _helpersController;


    public HomeController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;

        _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
    }

        [AllowAnonymous]
        public IActionResult Index()
        {
            
            string m= "Oh hi, I am Adeola Tijani, a software developer and an hair stylist. Hit me up on 07036949977 if you need my service(s). Gotcha!!!";
            QRCodeGenerator qrg = new QRCodeGenerator();
            QRCodeData qrd = qrg.CreateQrCode(m, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrd);
            System.DrawingCore.Bitmap qrCodeImage = qrCode.GetGraphic(20);
           
            ViewBag.QrCodeImg=BitmapToBytes(qrCodeImage);

            return View();
        }
        [AllowAnonymous]

        public static Byte[] BitmapToBytes(System.DrawingCore.Bitmap img)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                img.Save(stream, System.DrawingCore.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Error(string message)
        {
            var msg = generalClass.Decrypt(message);

            ViewData["Message"] = msg;
            return View();
        }
        [AllowAnonymous]

        public IActionResult Errorr(string message)
        {
            var msg = generalClass.Decrypt(message);

            ViewData["Message"] = msg;
            return View();
        }
    }
}
