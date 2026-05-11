using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Water_Filtration.Models.data;
using MySqlConnector;
using System.Text;
using Microsoft.EntityFrameworkCore;
using static Water_Filtration.EmailService;
using Microsoft.Graph.Models;

namespace Water_Filtration.Controllers
{
    public class scheduledtaskController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly DbPlcOnlineContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly EmailService _emailService;





        public scheduledtaskController(
            DbPlcOnlineContext dbcontext,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment, EmailService emailService)
        {
            _context = dbcontext;
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _emailService = emailService;
        }

        class DataPoint
        {
            public int id { get; set; }
            public double FS_101 { get; set; }
            public double FS_102 { get; set; }
            public double FS_301 { get; set; }
            public double FS_302 { get; set; }
            public double FS_303 { get; set; }
            public double FS_601 { get; set; }
            public DateTime CreatedOn { get; set; }

            public override string ToString()
                => $"CreatedOn: {CreatedOn:yyyy-MM-dd HH:mm:ss}";
        }

        [HttpGet]
        //public async Task<IActionResult> Run()
        //{
        //    await LogToFile("Run() invoked at " + GetLondonTime() + "");

        //    // 1. Determine time window
        //    DateTime startTime;
        //    var lastRecord = _context.Piechartdata
        //                             .OrderByDescending(p => p.Id)
        //                             .FirstOrDefault();

        //    if (lastRecord != null && lastRecord.cronjobsendtime.HasValue)
        //    {
        //        startTime = lastRecord.cronjobsendtime.Value;
        //        await LogToFile($"Using last record EndTime as startTime: {startTime:O}");
        //    }
        //    else
        //    {
        //        // fallback default
        //        startTime = GetLondonTime().AddMinutes(-10);
        //        await LogToFile($"No previous EndTime found; defaulting startTime to {startTime:O}");
        //    }

        //    DateTime endTime = GetLondonTime();
        //    await LogToFile($"Computed endTime: {endTime:O}");



        //    if (startTime.Date != endTime.Date)
        //    {
        //        var midnight = startTime.Date.AddDays(1);
        //        await LogToFile($"Day crossed: clamping endTime from {endTime:O} to midnight {midnight:O}");
        //        endTime = midnight;
        //    }


        //    // 2. Read connection string
        //    string connectionString = _configuration.GetConnectionString("Defaultconnection");
        //    await LogToFile($"Connection string loaded: {(connectionString != null ? "OK" : "MISSING")}");

        //    try
        //    {
        //        // 3. Fetch and process data
        //        var rawPoints = ReadDataFromDatabase(connectionString, startTime, endTime);
        //        await LogToFile($"Read {rawPoints.Count} raw points between {startTime:O} and {endTime:O}");

        //        if (!rawPoints.Any())
        //        {
        //            await LogToFile("No data found; exiting Run()");
        //            return Content("No data found");
        //        }

        //        rawPoints = rawPoints.OrderBy(p => p.CreatedOn).ToList();
        //        var filledPoints = FillMissingSeconds(rawPoints);
        //        await LogToFile($"After filling missing seconds: {filledPoints.Count} total points");

        //        var newCount = filledPoints.Except(rawPoints, new DataPointComparer()).Count();
        //        await LogToFile($"Interpolated points count: {newCount}");

        //        // 4. Save aggregated sums + start/end times
        //        await SaveAggregatedSumsAsync(filledPoints, startTime, endTime);
        //        await LogToFile("Aggregated sums saved successfully");

        //        return Content($"Ok – {rawPoints.Count} original, {newCount} interpolated");
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogToFile($"ERROR in Run(): {ex.Message}");
        //        return Content($"Error: {ex.Message}");
        //    }
        //}

        private static List<DataPoint> ReadDataFromDatabase(string connectionString, DateTime start, DateTime end)
        {
            var dataPoints = new List<DataPoint>();

            const string sql = @"
                SELECT 
                  MIN(id) AS id,
                  AVG(FS_101) AS FS_101,
                  AVG(FS_102) AS FS_102,
                  AVG(FS_301) AS FS_301,
                  AVG(FS_302) AS FS_302,
                  AVG(FS_303) AS FS_303,
                  AVG(FS_601) AS FS_601,
                  created_on
                FROM plcdata_main
                WHERE created_on BETWEEN @Start AND @End
                GROUP BY created_on
                ORDER BY created_on;";

            using var conn = new MySqlConnection(connectionString);
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Start", start);
            cmd.Parameters.AddWithValue("@End", end);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                dataPoints.Add(new DataPoint
                {
                    id = reader.GetInt32(0),
                    FS_101 = reader.GetDouble(1),
                    FS_102 = reader.GetDouble(2),
                    FS_301 = reader.GetDouble(3),
                    FS_302 = reader.GetDouble(4),
                    FS_303 = reader.GetDouble(5),
                    FS_601 = reader.GetDouble(6),
                    CreatedOn = reader.GetDateTime(7)
                });
            }

            return dataPoints;
        }

        private static List<DataPoint> FillMissingSeconds(List<DataPoint> originalData)
        {
            if (originalData.Count < 2)
                return originalData;

            var filled = new List<DataPoint> { originalData[0] };
            const int batchSize = 1000;
            int processed = 1;

            for (int i = 0; i < originalData.Count - 1; i++)
            {
                var curr = originalData[i];
                var next = originalData[i + 1];
                int diff = (int)(next.CreatedOn - curr.CreatedOn).TotalSeconds;

                if (diff > 1)
                {
                    for (int s = 1; s < diff; s++)
                    {
                        double frac = (double)s / diff;
                        filled.Add(new DataPoint
                        {
                            id = -1,
                            CreatedOn = curr.CreatedOn.AddSeconds(s),
                            FS_101 = curr.FS_101 + (next.FS_101 - curr.FS_101) * frac,
                            FS_102 = curr.FS_102 + (next.FS_102 - curr.FS_102) * frac,
                            FS_301 = curr.FS_301 + (next.FS_301 - curr.FS_301) * frac,
                            FS_302 = curr.FS_302 + (next.FS_302 - curr.FS_302) * frac,
                            FS_303 = curr.FS_303 + (next.FS_303 - curr.FS_303) * frac,
                            FS_601 = curr.FS_601 + (next.FS_601 - curr.FS_601) * frac
                        });
                    }
                }

                filled.Add(next);
                processed++;
            }

            return filled.OrderBy(p => p.CreatedOn).ToList();
        }

        //private async Task SaveAggregatedSumsAsync(
        //    List<DataPoint> points,
        //    DateTime startTime,
        //    DateTime endTime)
        //{
        //    double sum101 = points.Where(p => p.FS_101 >= 1).Sum(p => p.FS_101);
        //    double sum102 = points.Where(p => p.FS_102 >= 1).Sum(p => p.FS_102);
        //    double sum301 = points.Where(p => p.FS_301 >= 1).Sum(p => p.FS_301);
        //    double sum302 = points.Where(p => p.FS_302 >= 1).Sum(p => p.FS_302);
        //    double sum303 = points.Where(p => p.FS_303 >= 1).Sum(p => p.FS_303);
        //    double sum601 = points.Where(p => p.FS_601 >= 1).Sum(p => p.FS_601);

        //    //double sum101 = points.Sum(p => p.FS_101);
        //    //double sum102 = points.Sum(p => p.FS_102);
        //    //double sum301 = points.Sum(p => p.FS_301);
        //    //double sum302 = points.Sum(p => p.FS_302);
        //    //double sum303 = points.Sum(p => p.FS_303);
        //    //double sum601 = points.Sum(p => p.FS_601);

        //    var record = new Piechartdatum
        //    {
        //        BatchId = 1,               // set as needed
        //        Fs101 = sum101,
        //        Fs102 = sum102,
        //        Fs301 = sum301,
        //        Fs302 = sum302,
        //        Fs303 = sum303,
        //        Fs601 = sum601,
        //        Efficiency = 0,
        //        CreatedOn = endTime,
        //        cronjobstarttime = startTime,
        //        cronjobsendtime = endTime
        //    };

        //    _context.Piechartdata.Add(record);
        //    await _context.SaveChangesAsync();
        //    await LogToFile($"Inserted Piechartdatum ID={record.Id}");
        //}

        class DataPointComparer : IEqualityComparer<DataPoint>
        {
            public bool Equals(DataPoint x, DataPoint y)
                => x.CreatedOn == y.CreatedOn;
            public int GetHashCode(DataPoint obj)
                => obj.CreatedOn.GetHashCode();
        }

        private async Task LogToFile(string message)
        {
            try
            {
                string logsDir = Path.Combine(_webHostEnvironment.WebRootPath, "logs");
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);

                string filePath = Path.Combine(logsDir, $"log_{GetLondonTime():yyyy-MM-dd}.txt");
                string line = $"[{GetLondonTime():yyyy-MM-dd HH:mm:ss}] {message}";
                await System.IO.File.AppendAllTextAsync(filePath, line + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // If logging fails, silently ignore
            }
        }

        public static DateTime GetLondonTime()
        {
            TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
        }
        public static DateTime GetLondonYesterdayDate()
        {
            TimeZoneInfo londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            DateTime londonNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
            return londonNow.Date.AddDays(-1); 
        }

        //public async Task<IActionResult> Run1()
        //{
        //    try
        //    {


        //        DateTime londonYesterday = GetLondonYesterdayDate();

        //        DateTime startDate = londonYesterday;
        //        DateTime endDate = londonYesterday;

        //        //DateTime startDate = new DateTime(2025, 04, 9);
        //        //DateTime endDate = new DateTime(2025, 04, 9);

        //        var query = _context.Piechartdata.AsQueryable();

        //        if (startDate != null && endDate != null)
        //        {
        //            var start = startDate.Date;
        //            var end = endDate.Date.AddDays(1).AddTicks(-1);
        //            query = query.Where(x => x.CreatedOn >= start && x.CreatedOn <= end);
        //        }

        //        var dailyAvgData = await query
        //            .Where(x => x.CreatedOn != null)
        //            .Select(x => new
        //            {
        //                Date = x.CreatedOn.Value.Date,
        //                x.Fs101,
        //                x.Fs102,
        //                x.Fs301,
        //                x.Fs302,
        //                x.Fs303,
        //                x.Fs601
        //            })
        //            .GroupBy(x => x.Date)
        //            .Select(g => new
        //            {
        //                date = g.Key,
        //                fs101 = g.Sum(x => (double?)x.Fs101) / 3600 ?? 0,
        //                fs102 = g.Sum(x => (double?)x.Fs102) / 3600 ?? 0,
        //                fs301 = g.Sum(x => (double?)x.Fs301) / 3600 ?? 0,
        //                fs302 = g.Sum(x => (double?)x.Fs302) / 3600 ?? 0,
        //                fs303 = g.Sum(x => (double?)x.Fs303) / 3600 ?? 0,
        //                fs601 = g.Sum(x => (double?)x.Fs601) / 3600 ?? 0
        //            })
        //            .OrderBy(x => x.date)
        //            .ToListAsync();

        //        var formattedData = dailyAvgData.Select(d => new
        //        {
        //            date = d.date.ToString("yyyy-MM-dd"),
        //            d.fs101,
        //            d.fs102,
        //            d.fs301,
        //            d.fs302,
        //            d.fs303,
        //            d.fs601
        //        }).ToList();

        //        // Convert to report items
        //        var reportItems = formattedData.Select(d => new WastewaterReportItem
        //        {
        //            Date = DateTime.Parse(d.date),
        //            WastewaterVolume = d.fs101,
        //            FwBwVolume = d.fs102,
        //            Permit = d.fs302, // Add your logic here
        //            Drain = d.fs101 - d.fs302, // Add your logic here
        //            WO = d.fs601, // Replace with actual WorkOrder if needed
        //            RecoveryPercentage = (d.fs101 != 0) ? (d.fs302 / d.fs101) * 100 : 0
        //        }).ToList();

        //        // Generate email body
        //        // string emailBody = GenerateEmailBody(reportItems);

        //        // Send email
        //        await _emailService.SendEmailAsync(reportItems);

        //        return Ok("Email Sent!");
        //    }
        //    catch (Exception ex)
        //    {
        //        await LogToFile($"ERROR in Run1(): {ex.Message}");
        //        return Content($"Error: {ex.Message}");
        //    }
        //}



    }
}