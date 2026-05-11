namespace Water_Filtration.Modals
{
    public class ReportMasterView
    {
        
            public ReportMasterView()
            {
            ReportViews = new List<ReportMasterView>(); // Initialize the list
            }

            public int batchid { get; set; }
            public double? r1 { get; set; }
            public double? r2 { get; set; }
            public double? r3 { get; set; }
            public double? r4 { get; set; }
            public double? r5 { get; set; }
            public double? r6 { get; set; }
            public DateTime date { get; set; }
            public string date1 { get; set; }
            public DateTime time { get; set; }


            public List<ReportMasterView> ReportViews { get; set; }
        }
    
}
