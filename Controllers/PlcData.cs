using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Water_Filtration.Models.data;

namespace Water_Filtration.Controllers
{
    public class PlcDataController : Controller
    {

        private readonly DbPlcOnlineContext _context;
        private readonly string _connectionString;

        public PlcDataController(DbPlcOnlineContext context, IConfiguration configuration)
        {

            _context = context;
            _connectionString = configuration.GetConnectionString("Defaultconnection");
        }
        public IActionResult Export()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ExportToExcelPLCdata(DateTime? startDate)
        {
            try
            {
                int? userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    return Unauthorized();
                }

                var dbUser = _context.Users.FirstOrDefault(x => x.Id == userId);

                if (dbUser == null)
                {
                    return Unauthorized();
                }

                int siteFk = (int)dbUser.CompanyFk;

                DateTime start = startDate?.Date ?? DateTime.Today;
                DateTime end = start.Date.AddDays(1).AddSeconds(-1);

                // Site Name
                var site = _context.Sites
                    .Where(x => x.Id == siteFk)
                    .Select(x => new
                    {
                        x.SiteName
                    })
                    .FirstOrDefault();

                string siteName = site?.SiteName ?? "Site";

                // DataTable
                DataTable dt = new DataTable();

                string connString = _context.Database.GetConnectionString();

                using (MySqlConnection conn = new MySqlConnection(connString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("GetDistinctPlcvaluesForExport", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@p_StartDate", start);
                        cmd.Parameters.AddWithValue("@p_EndDate", end);
                        cmd.Parameters.AddWithValue("@p_SiteFk", siteFk);

                        conn.Open();

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }
                }

                using (XLWorkbook wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("PLC Report");

                    // Title
                    ws.Range(1, 1, 1, dt.Columns.Count).Merge();
                    ws.Cell(1, 1).Value = $"PLC Report - {siteName}";
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 18;
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Date
                    ws.Range(2, 1, 2, dt.Columns.Count).Merge();
                    ws.Cell(2, 1).Value = $"Date: {start:dd/MM/yyyy}";
                    ws.Cell(2, 1).Style.Font.Bold = true;
                    ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int headerRow = 4;

                    // Column Headers
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        ws.Cell(headerRow, i + 1).Value = dt.Columns[i].ColumnName;

                        ws.Cell(headerRow, i + 1).Style.Font.Bold = true;
                        ws.Cell(headerRow, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        ws.Cell(headerRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    // Data
                    int dataRow = 5;

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ws.Cell(dataRow + i, j + 1).Value =
                                dt.Rows[i][j]?.ToString();

                            ws.Cell(dataRow + i, j + 1)
                                .Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }
                    }

                    ws.Columns().AdjustToContents();

                    using (MemoryStream stream = new MemoryStream())
                    {
                        wb.SaveAs(stream);

                        string fileName =
                            $"PLC_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                        return File(
                            stream.ToArray(),
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
    }
}
