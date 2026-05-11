using Microsoft.AspNetCore.Mvc;
using Water_Filtration.Models.data;

namespace Water_Filtration.Controllers
{
    public class UserController : Controller
    {
        private readonly DbPlcOnlineContext _context;
        private readonly string _connectionString;

        public UserController(DbPlcOnlineContext context, IConfiguration configuration)
        {

            _context = context;
            _connectionString = configuration.GetConnectionString("Defaultconnection");
        }
        public IActionResult Index()
        {

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new List<object>());
            }

            // Get logged in user
            var dbUser = _context.Users
                .FirstOrDefault(x => x.Id == userId);

            if (dbUser == null)
            {
                return Json(new List<object>());
            }

            // CompanyFk / SiteFk from user table
            int siteFk = (int)dbUser.CompanyFk;

            var users = _context.Users
                .Where(x => x.IsDelete != 1 && x.CompanyFk == siteFk)
                .ToList();

            return View(users);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedOn = DateTime.Now;
                model.IsDelete = 0;

                _context.Users.Add(model);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }




        
        public IActionResult accessSetting()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveSettings([FromBody] SaveSettingsDto model)
        {
            try
            {


                if (model == null)
                {
                    return BadRequest("Invalid data");
                }

                // Example:
                // You can also get from session if needed
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                // Get logged in user
                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                // CompanyFk / SiteFk from user table
                int siteId = (int)dbUser.CompanyFk;

                

                //----------------------------------------
                // TANK ACCESS
                //----------------------------------------
                foreach (var tank in model.Tanks)
                {
                    var existingTank = _context.Sitetankaccesses
                        .FirstOrDefault(x =>
                            x.FkSite == siteId &&
                            x.FkTank == tank.Id);

                    if (existingTank != null)
                    {
                        existingTank.HasAccess = tank.IsChecked ? (sbyte)1 : (sbyte)0;
                        existingTank.IsCwtank =
                            model.CwTankId == tank.Id ? 1 : 0;

                        existingTank.CreatedOn = DateTime.Now;
                    }
                    else
                    {
                        _context.Sitetankaccesses.Add(new Sitetankaccess
                        {
                            FkSite = siteId,
                            FkTank = tank.Id,
                            HasAccess = tank.IsChecked ? (sbyte)1 : (sbyte)0,
                            IsCwtank = model.CwTankId == tank.Id ? 1 : 0,
                            CreatedOn = DateTime.Now
                        });
                    }
                }

                //----------------------------------------
                // FLOW ACCESS
                //----------------------------------------
                foreach (var flow in model.FlowMeters)
                {
                    var existingFlow = _context.Siteflowaccesses
                        .FirstOrDefault(x =>
                            x.FkSite == siteId &&
                            x.FkFlow == flow.Id);

                    if (existingFlow != null)
                    {
                        existingFlow.HasAccess = flow.IsChecked;
                        existingFlow.CreatedOn = DateTime.Now;
                    }
                    else
                    {
                        _context.Siteflowaccesses.Add(new Siteflowaccess
                        {
                            FkSite = siteId,
                            FkFlow = flow.Id,
                            HasAccess = flow.IsChecked,
                            CreatedOn = DateTime.Now
                        });
                    }
                }

                //----------------------------------------
                // TEMP ACCESS
                //----------------------------------------
                foreach (var temp in model.Temps)
                {
                    var existingTemp = _context.Sitetempaccesses
                        .FirstOrDefault(x =>
                            x.FkSite == siteId &&
                            x.FkTemp == temp.Id);

                    if (existingTemp != null)
                    {
                        existingTemp.HasAccess = temp.IsChecked;
                        existingTemp.CreatedOn = DateTime.Now;
                    }
                    else
                    {
                        _context.Sitetempaccesses.Add(new Sitetempaccess
                        {
                            FkSite = siteId,
                            FkTemp = temp.Id,
                            HasAccess = temp.IsChecked,
                            CreatedOn = DateTime.Now
                        });
                    }
                }

                //----------------------------------------
                // PRESSURE ACCESS
                //----------------------------------------
                foreach (var pressure in model.Pressures)
                {
                    var existingPressure = _context.Sitepressureaccesses
                        .FirstOrDefault(x =>
                            x.FkSite == siteId &&
                            x.FkPressure == pressure.Id);

                    if (existingPressure != null)
                    {
                        existingPressure.HasAccess = pressure.IsChecked;
                    }
                    else
                    {
                        _context.Sitepressureaccesses.Add(new Sitepressureaccess
                        {
                            FkSite = siteId,
                            FkPressure = pressure.Id,
                            HasAccess = pressure.IsChecked
                        });
                    }
                }

                _context.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Settings saved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        public class SaveSettingsDto
        {
            public int SiteId { get; set; }

            public int? CwTankId { get; set; }

            public List<AccessItemDto> Tanks { get; set; }

            public List<AccessItemDto> FlowMeters { get; set; }

            public List<AccessItemDto> Temps { get; set; }

            public List<AccessItemDto> Pressures { get; set; }
        }

        public class AccessItemDto
        {
            public int Id { get; set; }

            public bool IsChecked { get; set; }
        }
        [HttpGet]
        public IActionResult GetTanks()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                // Get logged in user
                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                // CompanyFk / SiteFk from user table
                int siteId = (int)dbUser.CompanyFk;


                var data = (
                    from tank in _context.Tankmasters
                    join access in _context.Sitetankaccesses
                        .Where(x => x.FkSite == siteId)
                    on tank.Id equals access.FkTank into gj
                    from access in gj.DefaultIfEmpty()

                    select new
                    {
                        id = tank.Id,
                        name = tank.TankName,

                        isChecked = access != null &&
                                    access.HasAccess == 1,

                        isCwTank = access != null &&
                                   access.IsCwtank == 1
                    }
                ).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetFlowMeters()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                // Get logged in user
                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                // CompanyFk / SiteFk from user table
                int siteId = (int)dbUser.CompanyFk;

                var data = (
                    from flow in _context.Flowmasters
                    join access in _context.Siteflowaccesses
                        .Where(x => x.FkSite == siteId)
                    on flow.Fid equals access.FkFlow into gj
                    from access in gj.DefaultIfEmpty()

                    select new
                    {
                        id = flow.Fid,
                        name = flow.FlowName,

                        isChecked = access != null &&
                                    access.HasAccess == true
                    }
                ).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        public IActionResult GetTemps()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                // Get logged in user
                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                // CompanyFk / SiteFk from user table
                int siteId = (int)dbUser.CompanyFk;

                var data = (
                    from temp in _context.Temperaturemasters
                    join access in _context.Sitetempaccesses
                        .Where(x => x.FkSite == siteId)
                    on temp.Tid equals access.FkTemp into gj
                    from access in gj.DefaultIfEmpty()

                    select new
                    {
                        id = temp.Tid,
                        name = temp.Temperature,

                        isChecked = access != null &&
                                    access.HasAccess == true
                    }
                ).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetPressures()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                // Get logged in user
                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                // CompanyFk / SiteFk from user table
                int siteId = (int)dbUser.CompanyFk;

                var data = (
                    from pressure in _context.Pressuremasters
                    join access in _context.Sitepressureaccesses
                        .Where(x => x.FkSite == siteId)
                    on pressure.Pid equals access.FkPressure into gj
                    from access in gj.DefaultIfEmpty()

                    select new
                    {
                        id = pressure.Pid,
                        name = pressure.PressureName,

                        isChecked = access != null &&
                                    access.HasAccess == true
                    }
                ).ToList();

                return Json(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
} 



