﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewDepot.Models;
using Microsoft.AspNetCore.Authorization;
using NewDepot.Helpers;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;

namespace NewDepot.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly Depot_DBContext _context;
        GeneralClass generalClass = new GeneralClass();
        HelpersController helpers;
        IHttpContextAccessor _httpContextAccessor;
        IConfiguration _configuration;

        public UserRolesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;

            helpers = new HelpersController(_context, _configuration, _httpContextAccessor);
        }

        // GET: UserRoles
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public IActionResult Index()
        {
            return View(_context.UserRoles.ToList());
        }


        public JsonResult GetRoles()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = HttpContext.Request.Form["start"].FirstOrDefault();
            var length = HttpContext.Request.Form["length"].FirstOrDefault();
            var sortColumn = HttpContext.Request.Form["columns[" + HttpContext.Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnup = HttpContext.Request.Form["order[0][dir]"].FirstOrDefault();
            var txtSearch = HttpContext.Request.Form["search[value]"][0];

            int pageSize = length != null ? Convert.ToInt32(length) : 0;
            int skip = start != null ? Convert.ToInt32(start) : 0;
            int totalRecords = 0;

            var getRoles = from r in _context.UserRoles
                               where r.DeleteStatus == false
                               select new
                               {
                                   RoleID = r.Role_id,
                                   RoleName = r.RoleName,
                                   UpdatedAt = r.UpdatedAt.ToString(),
                                   CreatedAt = r.CreatedAt.ToString()
                               };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnup)))
            {
                if (sortColumnup == "desc")
                {
                    getRoles = sortColumn == "roleName" ? getRoles.OrderByDescending(c => c.RoleName) :
                               sortColumn == "updatedAt" ? getRoles.OrderByDescending(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getRoles.OrderByDescending(c => c.CreatedAt) :
                               getRoles.OrderByDescending(c => c.RoleID + " " + sortColumnup);
                }
                else
                {
                    getRoles = sortColumn == "roleName" ? getRoles.OrderBy(c => c.RoleName) :
                               sortColumn == "updatedAt" ? getRoles.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getRoles.OrderBy(c => c.CreatedAt) :
                               getRoles.OrderBy(c => c.RoleID);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getRoles = getRoles.Where(c => c.RoleName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getRoles.Count();
            var data = getRoles.Skip(skip).Take(pageSize).ToList();

            helpers.LogMessages("Displaying users roles...", generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            
            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }



       

        // POST: UserRoles/Create
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> CreateRoles(string RoleName)
        {
            string response = "";

            var role = from r in _context.UserRoles
                          where r.RoleName.ToUpper() == RoleName.ToUpper() && r.DeleteStatus == false
                          select r;

            if (role.Count() > 0)
            {
                response = "Role already exits, please enter another role.";
            }
            else
            {
                UserRoles con = new UserRoles()
                {
                    RoleName = RoleName,
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.UserRoles.Add(con);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "Role Created";
                }
                else
                {
                    response = "Something went wrong trying to create this role. Please try again.";
                }

            }

            helpers.LogMessages("Creating new User Role. Status : " + response + " Role Name : " + RoleName, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        

        // POST: UserRoles/Edit/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> EditRole(string RoleName, int RoleID)
        {
            string response = "";
            var getRole = from c in _context.UserRoles where c.Role_id == RoleID select c;

            getRole.FirstOrDefault().RoleName = RoleName;
            getRole.FirstOrDefault().UpdatedAt = DateTime.Now;
            getRole.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Role Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            helpers.LogMessages("Updating User Role. Status : " + response + " Role ID : " + RoleID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));

            return Json(response);
        }

        

        // POST: UserRoles/Delete/5
        //[Authorize(Roles = "SUPPORT, IT ADMIN, SUPER ADMIN, HEAD OFFICE ADMIN")]
        public async Task<IActionResult> DeleteRole(int RoleID)
        {
            string response = "";

            var getRoles = from c in _context.UserRoles where c.Role_id == RoleID select c;

            getRoles.FirstOrDefault().DeletedAt = DateTime.Now;
            getRoles.FirstOrDefault().UpdatedAt = DateTime.Now;
            getRoles.FirstOrDefault().DeleteStatus = true;
            getRoles.FirstOrDefault().DeletedBy = Convert.ToInt32(generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionUserID")));

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "Role Deleted";
            }
            else
            {
                response = "Role not deleted. Something went wrong trying to delete this role.";
            }
            helpers.LogMessages("Deleting User Role. Status : " + response + " Role ID : " + RoleID, generalClass.Decrypt(_httpContextAccessor.HttpContext.Session.GetString("_sessionEmail")));
            
            return Json(response);
        }

        private bool UserRolesExists(int id)
        {
            return _context.UserRoles.Any(e => e.Role_id == id);
        }
    }
}
