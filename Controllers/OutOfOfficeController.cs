using System;
using System.Collections.Generic;
using System.Linq;
using NewDepot.Controllers.Configurations;
using NewDepot.Helpers;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace NewDepot.Controllers.UsersManagement
{
    //[Authorize]
    public class OutOfOfficeController : Controller
    {

        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();
       

        public OutOfOfficeController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult OutOfOffice()
        {
            return View();
        }



        /*
         * Creating out of office module
         */
        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult CreateOutOfOffice(OutOfOffice outOfOffice)
        {
            string result = "";

            var staffID = _helpersController.getSessionUserID();

            var office = _context.OutOfOffice.Where(x => x.StaffID == staffID && x.Status != GeneralClass._FINISHED && x.DeletedStatus == false);

            if (office.Count() > 0)
            {
                result = "Sorry, you already have an active out of office schedule.";
            }
            else
            {
                OutOfOffice outOf = new OutOfOffice
                {
                    StaffID = staffID,
                    ReliverID = outOfOffice.ReliverID,
                    Comment = outOfOffice.Comment,
                    DateFrom = outOfOffice.DateFrom,
                    DateTo = outOfOffice.DateTo,
                    CreatedAt = DateTime.Now,
                    //Status = GeneralClass._WAITING,
                    Status = GeneralClass._STARTED,
                    DeletedStatus = false
                };

                _context.OutOfOffice.Add(outOf);

                if (_context.SaveChanges() > 0)
                {
                    result = "Out Created";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to create your out of office schedule.";
                }

            }

            _helpersController.LogMessages("Creating an out of office schedule see output => " + result, _helpersController.getSessionEmail());

            return Json(result);
        }




        /*
        * Creating out of office module
        */
        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult EditOutOfOffice(OutOfOffice outOfOffice)
        {
            string result = "";

            var StaffID = _helpersController.getSessionUserID();

            var office = _context.OutOfOffice.Where(x => x.OutID == outOfOffice.OutID && x.DeletedStatus == false);

            if (office.Count() <= 0)
            {
                result = "Sorry, no out of office schedule was found.";
            }
            else
            {
                office.FirstOrDefault().ReliverID = outOfOffice.ReliverID;
                office.FirstOrDefault().DateFrom = outOfOffice.DateFrom;
                office.FirstOrDefault().DateTo = outOfOffice.DateTo;
                office.FirstOrDefault().Comment = outOfOffice.Comment;
                office.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Out Edited";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to edit your out of office schedule.";
                }

            }

            _helpersController.LogMessages("Editing an out of office schedule see output => " + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * Get specific out of office for a staff
         */
        //[Authorize(Policy = "AllStaffRoles")]
        public JsonResult GetStaffOutOfOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join s in _context.Staff on o.ReliverID equals s.StaffID
                         where o.StaffID == _helpersController.getSessionUserID() && o.DeletedStatus == false
                         select new
                         {
                             OutID = o.OutID,
                             Me = "ME",
                             Reliever = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                             RelieverID = s.StaffID,
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment
                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Reliever.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying my out of office records", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult ApproveOOF(int OutID, string Comment)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutID == OutID);

            if (get.Count() > 0)
            {

                get.FirstOrDefault().ApproverComment = Comment;
                get.FirstOrDefault().Approved = true;
                get.FirstOrDefault().ApprovedBy = userEmail;
                get.FirstOrDefault().ApproverRole = userRole;
                get.FirstOrDefault().ApprovedDate = DateTime.Now;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Approve";

                    //send mail to staff
                    string subj = "Approval For Out Of Office";
                    string cont = "Your 'out of office' application has been approved.";

                    var getStaff = _context.Staff.Where(a => a.StaffID == get.FirstOrDefault().StaffID).FirstOrDefault();

                    var sendEmail2 = _helpersController.SendEmailMessageSBJAsync(getStaff.StaffEmail, getStaff.FirstName, subj, cont, "Staff");

                }
                else
                {
                    result = "Sorry! Something went wrong while trying to approve this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Approving an out of office by "+userRole+". see output => " + result, _helpersController.getSessionEmail());


            return Json(result);
        }
         //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult RejectOOF(int OutID, string Comment)
        {
            var userRole = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionRoleName));
            var userName = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionUserName));
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutID == OutID);

            if (get.Count() > 0)
            {
                get.FirstOrDefault().ApproverComment = Comment;
                get.FirstOrDefault().Approved = false;
                get.FirstOrDefault().ApprovedBy = userEmail;
                get.FirstOrDefault().ApproverRole = userRole;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Reject";
                    //send mail to staff
                    string subj = "Approval For Out Of Office";
                    string cont = "Your 'out of office' application has been declined.";

                    var getStaff = _context.Staff.Where(a => a.StaffID == get.FirstOrDefault().StaffID).FirstOrDefault();

                    var sendEmail2 = _helpersController.SendEmailMessageSBJAsync(getStaff.StaffEmail, getStaff.FirstName, subj, cont, "Staff");

                }
                else
                {
                    result = "Sorry! Something went wrong while trying to decline this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Declining an out of office by "+userRole+". see output => " + result, _helpersController.getSessionEmail());


            return Json(result);
        }

            //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult AllOutOfOffice()
        {
            var staffOfficeID = _helpersController.getSessionOfficeID();

            var getOut = from o in _context.OutOfOffice
                         join s in _context.Staff on o.ReliverID equals s.StaffID
                         join rr in _context.UserRoles.AsEnumerable() on s.RoleID equals rr.Role_id 
                         join ss in _context.Staff on o.StaffID equals ss.StaffID
                         join sr in _context.UserRoles.AsEnumerable() on ss.StaffID equals sr.Role_id 
                         join lc in _context.Location on ss.LocationID equals lc.LocationID
                         join fd in _context.FieldOffices on ss.FieldOfficeID equals fd.FieldOffice_id
                         where o.DeletedStatus == false && fd.FieldOffice_id== staffOfficeID
                         select new OutOfOfficeModel
                         {
                             OutID = o.OutID,
                             Staff = ss.LastName + " " + ss.FirstName + " (" + ss.StaffEmail + ")",
                             StaffRole= sr.RoleName,
                             StaffLocation=lc.LocationName,
                             Reliever = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                             RelieverRole= rr.RoleName,
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment,
                             ApproverComment=o.ApproverComment,
                             Approved=o.Approved,
                             ApprovedBy=o.ApprovedBy,
                             ApproverRole=o.ApproverRole,
                             ApprovedDate=o.ApprovedDate.ToString(),
                             DeskCount = _context.MyDesk.Where(x => x.StaffID == ss.StaffID && x.HasWork != true).Count(),
                         };


            _helpersController.LogMessages("Displaying all out of office staff", _helpersController.getSessionEmail());

            return View(getOut.ToList());

        }




        //[Authorize(Policy = "AllStaffRoles")]
        public IActionResult RelieveStaff()
        {
            return View();
        }




        /*
        * Get specific out of office for a staff
        */
        //[Authorize(Policy = "AllStaffRoles")]
        public JsonResult GetAllStaffOutOfOffice()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join s in _context.Staff on o.ReliverID equals s.StaffID
                         join ss in _context.Staff on o.StaffID equals ss.StaffID
                         where o.DeletedStatus == false
                         select new
                         {
                             OutID = o.OutID,
                             Staff = ss.LastName + " " + ss.FirstName + " (" + ss.StaffEmail + ")",
                             Reliever = s.LastName + " " + s.FirstName + " (" + s.StaffEmail + ")",
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment
                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Reliever.Contains(txtSearch.ToUpper()) || c.Staff.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all out of office staff", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }


        //[Authorize(Policy = "AllStaffRoles")]
        public JsonResult GetRelieveStaff()
        {

            var relieveStaff = _helpersController.getSessionUserID();
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));

            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDir = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getOut = from o in _context.OutOfOffice
                         join ss in _context.Staff on o.StaffID equals ss.StaffID
                         where o.DeletedStatus == false && o.ReliverID == relieveStaff && o.Status == GeneralClass._STARTED
                         select new
                         {
                             OutID = o.OutID,
                             Staff = ss.LastName + " " + ss.FirstName + " (" + ss.StaffEmail + ")",
                             DeskCount = _context.MyDesk.Where(c => c.StaffID == ss.StaffID && c.HasWork == false).Count(),
                             DateFrom = o.DateFrom.ToString(),
                             DateTo = o.DateTo.ToString(),
                             CreatedAt = o.CreatedAt.ToString(),
                             UpdatedAt = o.UpdatedAt.ToString(),
                             Status = o.Status,
                             Comment = o.Comment,
                             StaffEmail = ss.StaffEmail,
                             MyEmail = userEmail

                         };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderByDescending(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderByDescending(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderByDescending(c => c.DateTo) :
                               getOut.OrderByDescending(c => c.OutID + " " + sortColumnDir);
                }
                else
                {
                    getOut = sortColumn == "dateFrom" ? getOut.OrderBy(c => c.DateFrom) :
                               sortColumn == "updatedAt" ? getOut.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getOut.OrderBy(c => c.CreatedAt) :
                               sortColumn == "dateTo" ? getOut.OrderBy(c => c.DateTo) :
                               getOut.OrderBy(c => c.OutID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getOut = getOut.Where(c => c.Staff.Contains(txtSearch.ToUpper()) || c.Staff.Contains(txtSearch.ToUpper()) || c.DateFrom.Contains(txtSearch) || c.DateTo.Contains(txtSearch) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getOut.Count();
            var data = getOut.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all out of office staff to relieve", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }




        /*
         * Ending an out of office for a staff by support
         */
        //[Authorize(Policy = "AllStaffRoles")]
        public JsonResult FinishedOut(int OutID)
        {
            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutID == OutID && x.DeletedStatus == false);

            if (get.Count() > 0)
            {
                get.FirstOrDefault().Status = GeneralClass._FINISHED;
                get.FirstOrDefault().UpdatedAt = DateTime.Now;

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to end this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Ending an out of office for a staff. see output => " + result, _helpersController.getSessionEmail());

            return Json(result);
        }



        /*
         * deleting an out of office by staff
         */
        
        public JsonResult DeleteOut(int OutID)
        {
            var result = "";

            var get = _context.OutOfOffice.Where(x => x.OutID == OutID);

            if (get.Count() > 0)
            {
                get.FirstOrDefault().DeletedStatus = true;
                get.FirstOrDefault().DeletedAt = DateTime.Now;
                get.FirstOrDefault().DeletedBy = _helpersController.getSessionUserID();

                if (_context.SaveChanges() > 0)
                {
                    result = "Done";
                }
                else
                {
                    result = "Sorry! Something went wrong trying to delete this out of office schedule";
                }
            }
            else
            {
                result = "Sorry! cannot find the selected out of office schedule.";
            }

            _helpersController.LogMessages("Deleteing an out of office by a staff. see output => " + result, _helpersController.getSessionEmail());


            return Json(result);
        }



        [AllowAnonymous]
        public JsonResult TriggerOutOfOfficeStart()
        {
            var result = 0;

            var get = from o in _context.OutOfOffice.AsEnumerable()
                      where o.Status == GeneralClass._WAITING && o.DeletedStatus == false
                      select o;


            foreach (var a in get.ToList())
            {
                if (a.DateFrom < DateTime.Now)
                {
                    var update = _context.OutOfOffice.Where(x => x.OutID == a.OutID);
                    update.FirstOrDefault().Status = GeneralClass._STARTED;
                    update.FirstOrDefault().UpdatedAt = DateTime.Now;
                    result += _context.SaveChanges();
                }
            }
            return Json(result);
        }
        /*
         * Trigger end for out of office
         */
        [AllowAnonymous]
        public JsonResult TriggerOutOfOfficeEnd()
        {
            var result = 0;

            var get = from o in _context.OutOfOffice.AsEnumerable()
                      where o.Status == GeneralClass._STARTED && o.DeletedStatus == false
                      select o;

            foreach (var a in get.ToList())
            {
                if (a.DateTo < DateTime.Now)
                {
                    var update = _context.OutOfOffice.Where(x => x.OutID == a.OutID);
                    update.FirstOrDefault().Status = GeneralClass._FINISHED;
                    update.FirstOrDefault().UpdatedAt = DateTime.Now;
                    result += _context.SaveChanges();
                }
            }
            return Json(result);
        }




        [AllowAnonymous]
        public JsonResult CountRelieveStaff()
        {
            var relieveStaff = _helpersController.getSessionUserID();
            var countRelieve = _context.OutOfOffice.Where(x => x.ReliverID == relieveStaff && x.DeletedStatus == false && x.Status == GeneralClass._STARTED).AsEnumerable().Count();
            return Json(countRelieve);
        }





        
        public JsonResult SwitchAccount(string email)
        {
            var userEmail = generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString(Authentications.AuthController.sessionEmail));
            var result = "Done|"+ userEmail;
            _helpersController.LogMessages("Switching account for out of office for " + email,_helpersController.getSessionEmail());
            return Json(result);
        }




    }
}