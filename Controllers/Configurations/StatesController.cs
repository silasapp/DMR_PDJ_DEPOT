using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewDepot.Helpers;

using Microsoft.AspNetCore.Authorization;
using NewDepot.Models;

namespace NewDepot.Controllers.Configurations
{

    ////[Authorize(Policy = "ConfigurationRoles")]
    public class StatesController : Controller
    {
        private readonly Depot_DBContext _context;
        IHttpContextAccessor _httpContextAccessor;
        public IConfiguration _configuration;
        HelpersController _helpersController;
        GeneralClass generalClass = new GeneralClass();

        public StatesController(Depot_DBContext context, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _helpersController = new HelpersController(_context, _configuration, _httpContextAccessor);
        }


        #region state configuration

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetStates()
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

            var getStates = from s in _context.States_UT
                            join c in _context.countries on s.Country_id equals c.id
                            where s.DeleteStatus == false 
                            select new
                            {
                                CountryId = c.id,
                                StateId = s.State_id,
                                CountryName = c.name,
                                StateName = s.StateName,
                                UpdatedAt = s.UpdatedAt.ToString(),
                                CreatedAt = s.CreatedAt.ToString()
                            };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getStates = sortColumn == "countryName" ? getStates.OrderByDescending(s => s.CountryName) :
                               sortColumn == "stateName" ? getStates.OrderByDescending(s => s.StateName) :
                               sortColumn == "updatedAt" ? getStates.OrderByDescending(s => s.UpdatedAt) :
                               sortColumn == "createdAt" ? getStates.OrderByDescending(s => s.CreatedAt) :
                               getStates.OrderByDescending(s => s.StateId + " " + sortColumnDir);
                }
                else
                {
                    getStates = sortColumn == "countryName" ? getStates.OrderBy(c => c.CountryName) :
                                sortColumn == "stateName" ? getStates.OrderBy(s => s.StateName) :
                               sortColumn == "updatedAt" ? getStates.OrderBy(c => c.UpdatedAt) :
                               sortColumn == "createdAt" ? getStates.OrderBy(c => c.CreatedAt) :
                               getStates.OrderBy(c => c.StateId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getStates = getStates.Where(c => c.CountryName.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) || c.CreatedAt.Contains(txtSearch) || c.UpdatedAt.Contains(txtSearch));
            }

            totalRecords = getStates.Count();
            var data = getStates.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all States", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }

        public async Task<IActionResult> CreateState(int CountryID, string StateName)
        {
            string response = "";

            var country = from s in _context.States_UT
                          where s.Country_id == CountryID && s.StateName == StateName.ToUpper() && s.DeleteStatus == false
                          select s;

            if (country.Count() > 0)
            {
                response = "State already exits, please enter another state.";
            }
            else
            {
                States_UT states = new States_UT()
                {
                    Country_id = CountryID,
                    StateName = StateName.ToUpper(),
                    CreatedAt = DateTime.Now,
                    DeleteStatus = false
                };

                _context.States_UT.Add(states);
                int StateCreated = await _context.SaveChangesAsync();

                if (StateCreated > 0)
                {
                    response = "State Created";
                }
                else
                {
                    response = "Something went wrong trying to create state. Please try again.";
                }
            }
            _helpersController.LogMessages("Creating new State. Status : " + response + " Country ID : " + CountryID + "State Name : " + StateName, _helpersController.getSessionEmail());

            return Json(response);
        }

        public async Task<IActionResult> EditState(string StateName, int StateID, int CountryID)
        {
            string response = "";
            var getState = from x in _context.States_UT where x.State_id == StateID select x;

            getState.FirstOrDefault().StateName = StateName.ToUpper();
            getState.FirstOrDefault().Country_id = CountryID;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = false;

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "State Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating State. Status : " + response + " State ID : " + StateID + " Country ID : " + CountryID, _helpersController.getSessionEmail());

            return Json(response);
        }        
        public async Task<IActionResult> DeleteState(int StateID)
        {
            string response = "";

            var getState = from c in _context.States_UT where c.State_id == StateID select c;

            getState.FirstOrDefault().DeletedAt = DateTime.Now;
            getState.FirstOrDefault().UpdatedAt = DateTime.Now;
            getState.FirstOrDefault().DeleteStatus = true;
            getState.FirstOrDefault().DeletedBy =  _helpersController.getSessionUserID();

            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "State Deleted";
            }
            else
            {
                response = "State not deleted. Something went wrong trying to delete this state.";
            }

            _helpersController.LogMessages("Deleting State. Status : " + response + " State ID : " + StateID, _helpersController.getSessionEmail());

            return Json(response);
        }
        #endregion

        #region LGA configuration
        // GET: States

        public IActionResult LGA()
        {
            return View();
        }



        public JsonResult GetLGAs()
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

            var getLGAs = from l in _context.Lgas
                            join s in _context.States_UT on l.StateId equals s.State_id
                            select new
                            {
                                LGAId = l.Id,
                                StateId = l.StateId,
                                LGAName = l.Name,
                                LGACode = l.LGA_Code,
                                StateName = s.StateName,
                            };

            if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
            {
                if (sortColumnDir == "desc")
                {
                    getLGAs = sortColumn == "lGAName" ? getLGAs.OrderByDescending(s => s.LGAName) :
                               sortColumn == "stateName" ? getLGAs.OrderByDescending(s => s.StateName) :
                               sortColumn == "lGACode" ? getLGAs.OrderByDescending(s => s.StateName) :
                               getLGAs.OrderByDescending(s => s.StateId + " " + sortColumnDir);
                }
                else
                {
                    getLGAs = sortColumn == "lGAName" ? getLGAs.OrderBy(s => s.LGAName) :
                               sortColumn == "stateName" ? getLGAs.OrderBy(s => s.StateName) :
                               sortColumn == "lGACode" ? getLGAs.OrderBy(s => s.StateName) :
                               getLGAs.OrderBy(c => c.StateId);
                }

            }

            if (!string.IsNullOrWhiteSpace(txtSearch))
            {
                getLGAs = getLGAs.Where(c => c.LGAName.Contains(txtSearch.ToUpper()) || c.LGACode.Contains(txtSearch.ToUpper()) || c.StateName.Contains(txtSearch.ToUpper()) );
            }

            totalRecords = getLGAs.Count();
            var data = getLGAs.Skip(skip).Take(pageSize).ToList();

            _helpersController.LogMessages("Displaying all LGAs", _helpersController.getSessionEmail());

            return Json(new { draw = draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data = data });

        }


        public async Task<IActionResult> CreateLGA(string Name, string Code, int StateID)
        {
            string response = "";

            var LGA = from s in _context.Lgas
                          where s.StateId == StateID && s.Name.ToUpper() == Name.ToUpper() 
                          select s;

            if (LGA.Count() > 0)
            {
                response = "State already exits, please enter another state.";
            }
            else
            {
                Lgas lga = new Lgas()
                {
                    LGA_Code= Code.Trim().ToUpper(),
                    StateId = StateID,
                    Name = Name.ToUpper()
                };

                _context.Lgas.Add(lga);
                int Created = await _context.SaveChangesAsync();

                if (Created > 0)
                {
                    response = "LGA Created";
                }
                else
                {
                    response = "Something went wrong trying to create LGA. Please try again.";
                }
            }
            _helpersController.LogMessages("Creating new LGA. Status : " + response + " State ID : " + StateID + "LGA Name : " + Name, _helpersController.getSessionEmail());

            return Json(response);
        }


        /*
        * edit state and country
        */

        public async Task<IActionResult> EditLGA(int ID, string Name, string Code, int StateID)
        {
            string response = "";
            var getLGA = from x in _context.Lgas where x.Id == ID select x;

            getLGA.FirstOrDefault().Name = Name.ToUpper();
            getLGA.FirstOrDefault().LGA_Code = Code.Trim().ToUpper();
            getLGA.FirstOrDefault().StateId = StateID;
            
            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "LGA Updated";
            }
            else
            {
                response = "Nothing was updated.";
            }

            _helpersController.LogMessages("Updating LGA. Status : " + response + " State ID : " + StateID + " LGA Name : " + Name, _helpersController.getSessionEmail());

            return Json(response);
        }


        // Removing a state

        public async Task<IActionResult> DeleteLGA(int ID)
        {
            string response = "";

            var getLGA = (from c in _context.Lgas where c.Id == ID select c).FirstOrDefault();
            string Name = getLGA.Name;
            _context.Lgas.Remove(getLGA);
            int updated = await _context.SaveChangesAsync();

            if (updated > 0)
            {
                response = "LGA Deleted";
            }
            else
            {
                response = "LGA not deleted. Something went wrong trying to delete this state.";
            }

            _helpersController.LogMessages("Deleting LGA. Status : " + response + " LGA Name : " + Name, _helpersController.getSessionEmail());

            return Json(response);
        }


        #endregion


    }
}
