using Microsoft.AspNetCore.Mvc;
using S7.Net;
using Water_Filtration.Modals;

namespace Water_Filtration.Controllers
{
    public class PlcController : Controller
    {
        private readonly string plcIpAddress = "192.168.0.10"; // Replace with your PLC's IP address
        private readonly int rack = 0; // Replace with your PLC's rack
        private readonly int slot = 0; // Replace with your PLC's slot
        private static Random random = new Random();
        //public IActionResult Index()
        //{
        //    var plcData = new PlcData();
        //    using (var plc = new Plc(CpuType.S71200, plcIpAddress, (short)rack, (short)slot))
        //    {
        //        plc.Open();
        //        Console.WriteLine("PLC Connected");
        //        // Corrected way to read a bit (bool)
        //        plcData.ButtonState = (bool)plc.Read("DB50.DBX97.0"); // Read from DB1, Byte 0, Bit 0

        //        // Corrected way to read a word (int)
        //        plcData.DataValue = (bool)plc.Read("DB50.DBX97.0"); // Read from DB1, Word 2

        //        Console.WriteLine("PLC close");

        //        plc.Close();
        //    }
        //    return View(plcData);
        //}

        //[HttpPost]
        //public IActionResult ChangeButtonState(bool newState)
        //{
        //    using (var plc = new Plc(CpuType.S71200, plcIpAddress, (short)rack, (short)slot))
        //    {
        //        plc.Open();

        //        var ButtonState = (bool)plc.Read("DB50.DBX97.0");

        //        if (ButtonState == true)
        //        {
        //            plc.Write("DB50.DBX97.0", false);
        //        }
        //        else
        //        {
        //            plc.Write("DB50.DBX97.0", true);
        //        }

             
        //        Console.WriteLine("PLC Connected for ChangeButtonState value updated");

        //        plc.Close();
        //    }
        //    Console.WriteLine("PLC Not-Connected for ChangeButtonState");
        //    return RedirectToAction("Index");
        //}
    }
}
