using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text.Json;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Water_Filtration.Modals;
using Water_Filtration.Models.data;

using static System.Reflection.Metadata.BlobBuilder;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Alarm = Water_Filtration.Modals.Alarm;
namespace Water_Filtration.Controllers
{
    public class MasterController : Controller
    {


        
        private readonly DbPlcOnlineContext _context;
        private readonly string _connectionString;

        public MasterController(DbPlcOnlineContext context, IConfiguration configuration)
        {
            
            _context = context;
            _connectionString = configuration.GetConnectionString("Defaultconnection");
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get logged-in user
            var dbUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (dbUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Store Site Id
            if (dbUser.CompanyFk != null)
            {
                ViewBag.SiteId = dbUser.CompanyFk;

                // Get Site Name
                int siteFk = (int)dbUser.CompanyFk;

                var site = _context.Sites
    .Where(x => x.Id == siteFk)
    .Select(x => new
    {
        x.SiteName
    })
    .FirstOrDefault();

                string siteName = site?.SiteName ?? "Site";
                HttpContext.Session.SetString("SiteName", siteName);
                if (site != null)
                {
                    ViewBag.SiteName = site.SiteName;
                   
                }
            }

            return View();



        }




        [HttpPost]
        public IActionResult Venting()
        {
            return View();
        }


       



        private void AssignValuesDynamic(ReportMasterView data, int index, double? vValue)
        {
            string propertyName = $"r{index + 1}";

            var property = typeof(ReportMasterView).GetProperty(propertyName);
            if (property != null)
            {
                property.SetValue(data, Math.Round((double)vValue, 2));
            }
        }

        

        public IActionResult alarmreport()
        {
            return View();
        }

       
        [HttpGet]
        public JsonResult GetStepData()
        {
            // Fetch data from a database or API
            var data = new
            {
                RT = 40.6,  // Replace with actual DB value
                 
            };

            return Json(data);
        }



        [HttpGet]
        public IActionResult GetPieChartData(string dateRange)
        {
            try
            {
                // SESSION
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                int siteFk = (int)dbUser.CompanyFk;

                // DATE FILTER
                DateTime startDate = DateTime.Today;
                DateTime endDate = DateTime.Today.AddDays(1).AddSeconds(-1);

                if (!string.IsNullOrEmpty(dateRange))
                {
                    var dates = dateRange.Split("to");

                    if (dates.Length > 0)
                    {
                        startDate = Convert.ToDateTime(dates[0].Trim());

                        if (dates.Length > 1)
                        {
                            endDate = Convert.ToDateTime(dates[1].Trim())
                                .AddDays(1)
                                .AddSeconds(-1);
                        }
                        else
                        {
                            endDate = startDate.AddDays(1).AddSeconds(-1);
                        }
                    }
                }

                // DATABASE QUERY
                var rawData = _context.Waterusagesummaries
                    .Where(x =>
                        x.CreatedOn >= startDate &&
                        x.CreatedOn <= endDate &&
                        x.SiteFk == siteFk &&
                        x.IsDelete == 0)
                    .GroupBy(x => x.CreatedOn.Date)
                    .Select(g => new
                    {
                        Date = g.Key,

                        Fs101 = g.Sum(x => x.Fs101 ?? 0),

                        Fs102 = g.Sum(x => x.Fs102 ?? 0),

                        Fs301 = g.Sum(x => x.Fs301 ?? 0),

                        Fs302 = g.Sum(x => x.Fs302 ?? 0),

                        Fs601 = g.Sum(x => x.Fs601 ?? 0)
                    })
                    .OrderByDescending(x => x.Date)
                    .Take(1)
                    .ToList();

                // FORMAT AFTER ToList()
                var result = rawData.Select(x =>
                {
                    double total =
                        x.Fs101 +
                        x.Fs102 +
                        x.Fs302 +
                        x.Fs601;

                    return new
                    {
                        dataUsedStartDate = x.Date.ToString("yyyy-MM-dd"),

                        fs101 = Math.Round(x.Fs101 / 3600.0, 2),

                        fs102 = Math.Round(x.Fs102 / 3600.0, 2),

                        fs301 = Math.Round(x.Fs301 / 3600.0, 2),

                        fs302 = Math.Round(x.Fs302 / 3600.0, 2),

                        fs601 = Math.Round(x.Fs601 / 3600.0, 2),

                        fsper = total > 0
                            ? Math.Round((x.Fs302 / total) * 100, 2)
                            : 0
                    };
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        public IActionResult GetSiteTanks()
        {
            try
            {
                // Get UserId from session
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

                // Load tanks based on site access
                var tanks = (from sta in _context.Sitetankaccesses
                             join tm in _context.Tankmasters
                             on sta.FkTank equals tm.Id
                             where sta.FkSite == siteFk
                                   && sta.HasAccess == 1
                                   && (tm.IsDelete == 0 || tm.IsDelete == null)
                             select new
                             {
                                 id = tm.Id,
                                 name = tm.TankName,
                                 code = tm.TankName,
                                 isCwTank = sta.IsCwtank == 1
                             }).ToList();

                return Json(tanks);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public IActionResult Getsitetemp()
        {
            try
            {
                // Get UserId from session
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

                // Get SiteFk / CompanyFk
                int siteFk = (int)dbUser.CompanyFk;

                // Load temperature access data
                var temps = (from sta in _context.Sitetempaccesses
                             join tm in _context.Temperaturemasters
                             on sta.FkTemp equals tm.Tid
                             where sta.FkSite == siteFk
                                   && sta.HasAccess == true
                                   && (tm.IsDelete == 0 || tm.IsDelete == null)
                             select new
                             {
                                 id = tm.Tid,
                                 name = tm.Temperature,
                                 code = tm.Temperature
                             }).ToList();

                return Json(temps);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        
        public IActionResult GetSitePressures()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                int siteFk = (int)dbUser.CompanyFk;

                var pressures = (from sta in _context.Sitepressureaccesses
                                 join tm in _context.Pressuremasters
                                 on sta.FkPressure equals tm.Pid
                                 where sta.FkSite == siteFk
                                       && sta.HasAccess == true
                                       && (tm.IsDelete == 0 || tm.IsDelete == null)
                                 select new
                                 {
                                     id = tm.Pid,
                                     name = tm.PressureName,
                                     code = tm.PressureName
                                 }).ToList();

                return Json(pressures);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult GetSiteFlows()
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                int siteFk = (int)dbUser.CompanyFk;
                var flows = (from sta in _context.Siteflowaccesses
                             join tm in _context.Flowmasters
                             on sta.FkFlow equals tm.Fid
                             where sta.FkSite == siteFk
                                   && sta.HasAccess == true
                             select new
                             {
                                 id = tm.Fid,
                                 name = tm.FlowName
                             }).ToList();


                // fallback
                if (!flows.Any())
                {
                    flows = _context.Flowmasters
                        .Where(x => x.IsDelete == null)
                        .OrderBy(x => x.Fid)
                        .Select(x => new
                        {
                            id = x.Fid,
                            name = x.FlowName
                        })
                        .ToList();
                }


                var result = flows.Select(x => new
                {
                    id = x.id,
                    name = x.name,
                    code = ResolveFlowCode(x.id, x.name)
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private static string ResolveFlowCode(int id, string? flowName)
        {
            var normalized = (flowName ?? string.Empty)
                .Replace("_", string.Empty)
                .Replace("-", string.Empty)
                .Replace("/", string.Empty)
                .Replace(" ", string.Empty)
                .ToUpperInvariant();

            var nameMap = new Dictionary<string, string>
            {
                ["FS101"] = "FS_101",
                ["FS102"] = "FS_102",
                ["FS301"] = "FS_301",
                ["FS302"] = "FS_302",
                ["FS303"] = "FS_303",
                ["FS601"] = "FS_601",
                ["WASTEWATER"] = "FS_101",
                ["WASTE"] = "FS_101",
                ["FWBW"] = "FS_102",
                ["FW"] = "FS_102",
                ["PERMEATE"] = "FS_302",
                ["DRAIN"] = "FS_301",
                ["WO"] = "FS_601"
            };

            if (nameMap.TryGetValue(normalized, out var mappedCode))
            {
                return mappedCode;
            }

            // Fallback by known flow order used across reports.
            return id switch
            {
                1 => "FS_101",
                2 => "FS_102",
                3 => "FS_301",
                4 => "FS_302",
                5 => "FS_303",
                6 => "FS_601",
                _ => flowName ?? string.Empty
            };
        }

        public IActionResult alarmlogreport()
        {

            return View();
        }


        [HttpGet]
        public IActionResult plcAlarmReportJson(
    int pageNumber = 1,
    int pageSize = 10,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new List<object>());
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new List<object>());
                }

                int siteFk = (int)dbUser.CompanyFk;


                if (startDate == null)
                    startDate = DateTime.Today;

                if (endDate == null)
                    endDate = DateTime.Today;

                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date.AddDays(1);

                var query = from alarm in _context.Plcalarms
                            join tag in _context.Plctags
                            on alarm.TagFk equals tag.Id
                            where alarm.CreatedOn >= start
                                  && alarm.CreatedOn < end 
                                    && alarm.SiteFk == siteFk

                            orderby alarm.CreatedOn descending
                            select new
                            {
                                id = alarm.Id,
                                createdOn = alarm.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss"),
                                tagName = tag.TagName,
                                value = alarm.Value,
                                siteFk = alarm.SiteFk
                            };

                int totalRecords = query.Count();

                var data = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                return Json(new
                {
                    data,
                    pageNumber,
                    totalPages,
                    totalRecords
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    data = new List<object>(),
                    error = ex.Message
                });
            }
        }


        [HttpGet]
        public IActionResult ExportToExcelPlcAlarms(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int siteFk = (int)dbUser.CompanyFk;

                // Get Site Name
                var site = _context.Sites
     .Where(x => x.Id == siteFk)
     .Select(x => new
     {
         x.SiteName
     })
     .FirstOrDefault();

                string siteName = site?.SiteName ?? "Site";

                if (startDate == null)
                    startDate = DateTime.Today;

                if (endDate == null)
                    endDate = DateTime.Today;

                DateTime start = startDate.Value.Date;
                DateTime end = endDate.Value.Date.AddDays(1);

                // Get Alarm Data
                var data = (from alarm in _context.Plcalarms
                            join tag in _context.Plctags
                            on alarm.TagFk equals tag.Id
                            where alarm.CreatedOn >= start
                                  && alarm.CreatedOn < end
                                  && alarm.SiteFk == siteFk
                            orderby alarm.CreatedOn descending
                            select new
                            {
                                CreatedOn = alarm.CreatedOn,
                                TagName = tag.TagName
                            }).ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Alarm Report");

                    // Title
                    worksheet.Range("A1:C1").Merge();
                    worksheet.Cell(1, 1).Value = $"Alarm Report - {siteName}";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 18;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Date Range
                    worksheet.Range("A2:C2").Merge();
                    worksheet.Cell(2, 1).Value =
                        $"Date Range: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Header
                    worksheet.Cell(4, 1).Value = "Sr.No.";
                    worksheet.Cell(4, 2).Value = "Created On";
                    worksheet.Cell(4, 3).Value = "Tag Name";

                    var headerRange = worksheet.Range("A4:C4");

                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // Data
                    int row = 5;
                    int srNo = 1;

                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = srNo;
                        worksheet.Cell(row, 2).Value = item.CreatedOn.ToString("dd/MM/yyyy HH:mm");
                        worksheet.Cell(row, 3).Value = item.TagName;

                        worksheet.Range(row, 1, row, 3).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        worksheet.Range(row, 1, row, 3).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        row++;
                        srNo++;
                    }

                    // Auto Fit
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        var content = stream.ToArray();

                        string fileName = $"AlarmReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        

        public IActionResult FiltrationReport()
        {

            return View();
        }

        [HttpGet]
        public IActionResult filtrationreportJson(
        int pageNumber = 1,
        int pageSize = 10,
        DateTime? startDate = null,
        DateTime? endDate = null)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int siteFk = (int)dbUser.CompanyFk;



                DateTime start = startDate ?? DateTime.Today;
                DateTime end = endDate ?? DateTime.Today;

                end = end.Date.AddDays(1).AddSeconds(-1);

                // Filtration data
                var query = _context.Filtrationsummaries
                    .Where(x => x.CreatedOn >= start && x.CreatedOn <= end && x.SiteFk==siteFk)
                    .OrderByDescending(x => x.CreatedOn);

                int totalRecords = query.Count();

                var data = query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        createdOn =  x.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
                            ,

                        filtrationEvent = x.FiltrationEvent,

                        filtrationRunTime = x.FiltrationRunTime
                    })
                    .ToList();

                // ============================
                // CIP SUMMARY
                // ============================
                var cycleSummary = _context.Filtrationcyclesummaries
    .Where(x =>
        x.SummaryDate >= start &&
        x.SummaryDate <= end &&
        x.Cip > 0 &&
        x.SiteFk == siteFk)
    .GroupBy(x => x.SummaryDate.Value.Date)
    .Select(g => new
    {
        summaryDate = g.Key,
        cip = g.Sum(x => x.Cip)
    })
    .Where(x => x.cip > 0)
    .OrderByDescending(x => x.summaryDate)
    .ToList();

                return Json(new
                {
                    pageNumber = pageNumber,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    totalRecords = totalRecords,
                    data = data,
                    cycleSummary = cycleSummary
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ==================================
        // EXPORT TO EXCEL
        // ==================================
        [HttpGet]
        public IActionResult ExportToExcelFiltaration(
    DateTime? startDate,
    DateTime? endDate)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int siteFk = (int)dbUser.CompanyFk;

                var site = _context.Sites
    .Where(x => x.Id == siteFk)
    .Select(x => new
    {
        x.SiteName
    })
    .FirstOrDefault();

                string siteName = site?.SiteName ?? "Site";

                DateTime start = startDate ?? DateTime.Today;
                DateTime end = endDate ?? DateTime.Today;

                end = end.Date.AddDays(1).AddSeconds(-1);

                // =========================
                // FILTRATION DATA
                // =========================
                var filtrationData = _context.Filtrationsummaries
                    .Where(x =>
                        x.CreatedOn >= start &&
                        x.CreatedOn <= end &&
                        x.SiteFk == siteFk)
                    .OrderByDescending(x => x.CreatedOn)
                    .ToList();

                // =========================
                // CIP SUMMARY
                // =========================
                var cycleSummary = _context.Filtrationcyclesummaries
                    .Where(x =>
                        x.CreatedOn >= start &&
                        x.CreatedOn <= end &&
                        x.Cip > 0 &&
                        x.SiteFk == siteFk)
                    .GroupBy(x => x.CreatedOn.Value.Date)
                    .Select(g => new
                    {
                        summaryDate = g.Key,
                        cip = g.Sum(x => x.Cip)
                    })
                    .Where(x => x.cip > 0)
                    .OrderByDescending(x => x.summaryDate)
                    .ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Activity Report");

                    // =========================
                    // TITLE
                    // =========================
                    worksheet.Range("A1:F1").Merge();
                    worksheet.Cell("A1").Value = $"Activity Report - {siteName}";
                    worksheet.Cell("A1").Style.Font.Bold = true;
                    worksheet.Cell("A1").Style.Font.FontSize = 18;
                    worksheet.Cell("A1").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    // =========================
                    // DATE RANGE
                    // =========================
                    worksheet.Range("A2:F2").Merge();
                    worksheet.Cell("A2").Value =
                        $"Date Range: {start:dd/MM/yyyy} - {end:dd/MM/yyyy}";

                    worksheet.Cell("A2").Style.Font.Bold = true;
                    worksheet.Cell("A2").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    // =========================
                    // LEFT TABLE HEADER
                    // =========================
                    worksheet.Cell(4, 1).Value = "Sr.No.";
                    worksheet.Cell(4, 2).Value = "Created On";
                    worksheet.Cell(4, 3).Value = "Filtration Event";
                    worksheet.Cell(4, 4).Value = "Filtration RunTime";

                    // =========================
                    // RIGHT TABLE HEADER
                    // =========================
                    worksheet.Cell(4, 6).Value = "Date";
                    worksheet.Cell(4, 7).Value = "CIP (hrs)";

                    // Header Style
                    var headerRange = worksheet.Range("A4:G4");

                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    // =========================
                    // FILTRATION DATA
                    // =========================
                    int row = 5;
                    int srNo = 1;

                    foreach (var item in filtrationData)
                    {
                        worksheet.Cell(row, 1).Value = srNo;
                        worksheet.Cell(row, 2).Value =
                            item.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss");

                        worksheet.Cell(row, 3).Value = item.FiltrationEvent;
                        worksheet.Cell(row, 4).Value = item.FiltrationRunTime;

                        srNo++;
                        row++;
                    }

                    // =========================
                    // CIP DATA
                    // =========================
                    int cipRow = 5;

                    foreach (var item in cycleSummary)
                    {
                        worksheet.Cell(cipRow, 6).Value =
                            item.summaryDate.ToString("yyyy-MM-dd");

                        worksheet.Cell(cipRow, 7).Value =
                            Math.Round((double)(item.cip / 3600.0), 2);

                        cipRow++;
                    }

                    // =========================
                    // AUTO FIT
                    // =========================
                    worksheet.Columns().AdjustToContents();

                    // =========================
                    // BORDER
                    // =========================
                    var usedRange = worksheet.RangeUsed();

                    usedRange.Style.Border.OutsideBorder =
                        XLBorderStyleValues.Thin;

                    usedRange.Style.Border.InsideBorder =
                        XLBorderStyleValues.Thin;

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        var content = stream.ToArray();

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "ActivityReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }











        public IActionResult MonitorReport()
        {

            return View();
        }



        [HttpGet]
        public IActionResult Getmonitor_report(
    int pageNumber = 1,
    int pageSize = 10,
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                // =========================
                // SESSION
                // =========================
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int siteFk = (int)dbUser.CompanyFk;

                // =========================
                // DATE FILTER
                // =========================
                DateTime start = startDate ?? DateTime.Today;
                DateTime end = endDate ?? DateTime.Today;

                end = end.Date.AddDays(1).AddSeconds(-1);

                // =========================
                // QUERY
                // =========================
                var query = _context.Waterusagesummaries
                    .Where(x =>
                        x.CreatedOn >= start &&
                        x.CreatedOn <= end &&
                        x.SiteFk == siteFk &&
                        x.IsDelete == 0)
                    .GroupBy(x => x.CreatedOn.Date)
                    .Select(g => new
                    {
                        Date = g.Key,

                        Wastewater = g.Sum(x => x.Fs101 ?? 0),

                        FwBw = g.Sum(x => x.Fs102 ?? 0),

                        Permeate = g.Sum(x => x.Fs302 ?? 0),

                        Wo = g.Sum(x => x.Fs601 ?? 0)
                    })
                    .OrderByDescending(x => x.Date);

                int totalRecords = query.Count();

                var data = query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .Select(x => new
    {
        date = x.Date.ToString("yyyy-MM-dd"),

        fs101 = Math.Round(x.Wastewater / 3600.0, 2),

        fs102 = Math.Round(x.FwBw / 3600.0, 2),

        fs302 = Math.Round(x.Permeate / 3600.0, 2),

        fs601 = Math.Round(x.Wo / 3600.0, 2)
    })
    .ToList();

                return Json(new
                {
                    pageNumber = pageNumber,
                    totalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    totalRecords = totalRecords,
                    avgData = data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }





        [HttpGet]
        public IActionResult ExportTowastewater(
    DateTime? startDate,
    DateTime? endDate)
        {
            try
            {
                // =========================
                // SESSION
                // =========================
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int siteFk = (int)dbUser.CompanyFk;

                // =========================
                // DATE FILTER
                // =========================
                DateTime start = startDate ?? DateTime.Today;

                DateTime end = endDate ?? DateTime.Today;

                end = end.Date.AddDays(1).AddSeconds(-1);

                // =========================
                // DATA
                // =========================
                var data = _context.Waterusagesummaries
                    .Where(x =>
                        x.CreatedOn >= start &&
                        x.CreatedOn <= end &&
                        x.SiteFk == siteFk &&
                        x.IsDelete == 0)
                    .GroupBy(x => x.CreatedOn.Date)
                    .Select(g => new
                    {
                        Date = g.Key,

                        Wastewater = g.Sum(x => x.Fs101 ?? 0),

                        FwBw = g.Sum(x => x.Fs102 ?? 0),

                        Permeate = g.Sum(x => x.Fs302 ?? 0),

                        Wo = g.Sum(x => x.Fs601 ?? 0)
                    })
                    .OrderByDescending(x => x.Date)
                    .ToList();

                using (var workbook = new XLWorkbook())
                {
                    var worksheet =
                        workbook.Worksheets.Add("Waste Water Report");

                    // =========================
                    // TITLE
                    // =========================
                    worksheet.Range("A1:E1").Merge();

                    worksheet.Cell("A1").Value =
                        "Waste Water Data";

                    worksheet.Cell("A1").Style.Font.Bold = true;

                    worksheet.Cell("A1").Style.Font.FontSize = 20;

                    worksheet.Cell("A1").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("A1").Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    worksheet.Range("A1:E1").Style.Border.TopBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range("A1:E1").Style.Border.BottomBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range("A1:E1").Style.Border.LeftBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range("A1:E1").Style.Border.RightBorder =
                        XLBorderStyleValues.Thin;

                    // =========================
                    // DATE RANGE
                    // =========================
                    worksheet.Range("A2:E2").Merge();

                    worksheet.Cell("A2").Value =
                        $"Date Range: {start:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";

                    worksheet.Cell("A2").Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cell("A2").Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    worksheet.Range("A2:E2").Style.Border.BottomBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range("A2:E2").Style.Border.LeftBorder =
                        XLBorderStyleValues.Thin;

                    worksheet.Range("A2:E2").Style.Border.RightBorder =
                        XLBorderStyleValues.Thin;

                    // =========================
                    // HEADER
                    // =========================
                    worksheet.Cell(4, 1).Value = "Created On";
                    worksheet.Cell(4, 2).Value = "Wastewater";
                    worksheet.Cell(4, 3).Value = "FW/BW";
                    worksheet.Cell(4, 4).Value = "Permeate";
                    worksheet.Cell(4, 5).Value = "WO";

                    var headerRange = worksheet.Range("A4:E4");

                    headerRange.Style.Font.Bold = true;

                    headerRange.Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    headerRange.Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    headerRange.Style.Border.TopBorder =
                        XLBorderStyleValues.Thin;

                    headerRange.Style.Border.BottomBorder =
                        XLBorderStyleValues.Thin;

                    headerRange.Style.Border.LeftBorder =
                        XLBorderStyleValues.Thin;

                    headerRange.Style.Border.RightBorder =
                        XLBorderStyleValues.Thin;

                    // =========================
                    // DATA
                    // =========================
                    int row = 5;

                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value =
                            item.Date.ToString("yyyy-MM-dd");

                        worksheet.Cell(row, 2).Value =
                            Math.Round(item.Wastewater / 3600.0, 8);

                        worksheet.Cell(row, 3).Value =
                            Math.Round(item.FwBw / 3600.0, 8);

                        worksheet.Cell(row, 4).Value =
                            Math.Round(item.Permeate / 3600.0, 8);

                        worksheet.Cell(row, 5).Value =
                            Math.Round(item.Wo / 3600.0, 8);

                        // Border
                        worksheet.Range(row, 1, row, 5)
                            .Style.Border.OutsideBorder =
                                XLBorderStyleValues.Thin;

                        worksheet.Range(row, 1, row, 5)
                            .Style.Border.InsideBorder =
                                XLBorderStyleValues.Thin;

                        row++;
                    }

                    // =========================
                    // ALIGNMENT
                    // =========================
                    worksheet.Cells().Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    worksheet.Cells().Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    // =========================
                    // AUTO WIDTH
                    // =========================
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);

                        var content = stream.ToArray();

                        return File(
                            content,
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            "WasteWaterReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }


        


            public IActionResult wastewaterAnalytics()
        {

            return View();
        }


        [HttpGet]
        public IActionResult waterAnalytics(
    DateTime? startDate = null,
    DateTime? endDate = null)
        {
            try
            {
                // =========================
                // SESSION
                // =========================
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Session expired"
                    });
                }

                var dbUser = _context.Users
                    .FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "User not found"
                    });
                }

                int siteFk = (int)dbUser.CompanyFk;

                // =========================
                // DATE FILTER
                // =========================
                DateTime start = startDate ?? DateTime.Today;
                DateTime end = endDate ?? DateTime.Today;

                end = end.Date.AddDays(1).AddSeconds(-1);

                // =========================
                // QUERY
                // =========================
                var data = _context.Waterusagesummaries
                    .Where(x =>
                        x.CreatedOn >= start &&
                        x.CreatedOn <= end &&
                        x.SiteFk == siteFk &&
                        x.IsDelete == 0)
                    .AsEnumerable() // IMPORTANT for Date formatting
                    .GroupBy(x => x.CreatedOn.Date)
                    .Select(g => new
                    {
                        date = g.Key.ToString("yyyy-MM-dd"),

                        fs101 = Math.Round(g.Sum(x => x.Fs101 ?? 0) / 3600.0, 2),

                        fs102 = Math.Round(g.Sum(x => x.Fs102 ?? 0) / 3600.0, 2),

                        fs301 = Math.Round(g.Sum(x => x.Fs301 ?? 0) / 3600.0, 2),

                        fs302 = Math.Round(g.Sum(x => x.Fs302 ?? 0) / 3600.0, 2),

                        fs303 = Math.Round(g.Sum(x => x.Fs303 ?? 0) / 3600.0, 2),

                        fs601 = Math.Round(g.Sum(x => x.Fs601 ?? 0) / 3600.0, 2)
                    })
                    .OrderBy(x => x.date)
                    .ToList();

                return Json(new
                {
                    success = true,
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }



        //[HttpGet]
        //public IActionResult GetFiltrationNotifications()
        //{
        //    // SESSION
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Json(new { notifications = new List<object>() });
        //    }

        //    var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

        //    if (dbUser == null)
        //    {
        //        return Json(new { notifications = new List<object>() });
        //    }

        //    int siteFk = (int)dbUser.CompanyFk;

        //    // Latest 3 filtration records
        //    var notifications = _context.Filtrationsummaries
        //        .Where(x => x.SiteFk == siteFk)
        //        .OrderByDescending(x => x.CreatedOn)
        //        .Take(3)
        //        .Select(x => new
        //        {
        //            filtrationEvent = x.FiltrationEvent,
        //            createdOn = x.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
        //        })
        //        .ToList();

        //    return Json(new
        //    {
        //        notifications = notifications
        //    });
        //}

        //[HttpGet]
        //public IActionResult PlcAlarmNotifications()
        //{
        //    // SESSION
        //    int? userId = HttpContext.Session.GetInt32("UserId");

        //    if (userId == null)
        //    {
        //        return Json(new { alarms = new List<object>() });
        //    }

        //    var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

        //    if (dbUser == null)
        //    {
        //        return Json(new { alarms = new List<object>() });
        //    }

        //    int siteFk = (int)dbUser.CompanyFk;

        //    // Latest 3 alarms
        //    var alarms = (from alarm in _context.Plcalarms
        //                  join tag in _context.Plctags
        //                  on alarm.TagFk equals tag.Id
        //                  where alarm.SiteFk == siteFk
        //                  orderby alarm.CreatedOn descending
        //                  select new
        //                  {
        //                      tagName = tag.TagName,
        //                      createdOn = alarm.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
        //                  })
        //        .Take(3)
        //        .ToList();

        //    return Json(new
        //    {
        //        alarms = alarms
        //    });
        //}


        [HttpGet]
        public IActionResult GetFiltrationNotifications()
        {
            // SESSION
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { notifications = new List<object>() });
            }

            var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

            if (dbUser == null)
            {
                return Json(new { notifications = new List<object>() });
            }

            int siteFk = (int)dbUser.CompanyFk;

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            // Latest 3 today's filtration records
            var notifications = _context.Filtrationsummaries
                .Where(x => x.SiteFk == siteFk
                         && x.CreatedOn >= today
                         && x.CreatedOn < tomorrow)
                .OrderByDescending(x => x.CreatedOn)
                .Take(3)
                .Select(x => new
                {
                    filtrationEvent = x.FiltrationEvent,
                    createdOn = x.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            return Json(new
            {
                notifications = notifications
            });
        }

        [HttpGet]
        public IActionResult PlcAlarmNotifications()
        {
            // SESSION
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return Json(new { alarms = new List<object>() });
            }

            var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

            if (dbUser == null)
            {
                return Json(new { alarms = new List<object>() });
            }

            int siteFk = (int)dbUser.CompanyFk;

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            // Latest 3 today's alarms
            var alarms = (from alarm in _context.Plcalarms
                          join tag in _context.Plctags
                          on alarm.TagFk equals tag.Id
                          where alarm.SiteFk == siteFk
                                && alarm.CreatedOn >= today
                                && alarm.CreatedOn < tomorrow
                          orderby alarm.CreatedOn descending
                          select new
                          {
                              tagName = tag.TagName,
                              createdOn = alarm.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
                          })
                          .Take(3)
                          .ToList();

            return Json(new
            {
                alarms = alarms
            });
        }

    }
}
