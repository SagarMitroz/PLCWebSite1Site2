using Microsoft.VisualBasic;
using Water_Filtration.Models.data;

namespace Water_Filtration.Modals
{
    public class Tankview
    {

        public Tankview()
        {
            Tankviews = new List<Tankview>(); // Initialize the list
        }

        public int batchid { get; set; }
      public double? t1 { get; set; }
      public double? t6{ get; set; }
      public double? t3 { get; set; }
      public double? t4 { get; set; }
      public double? t5 { get; set; }
      public DateTime date { get; set; }
      public DateTime time { get; set; }


        public List<Tankview> Tankviews { get; set; }
    }
}
