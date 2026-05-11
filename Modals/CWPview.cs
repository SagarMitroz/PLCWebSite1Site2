namespace Water_Filtration.Modals
{
    public class CWPview
    {
        public CWPview()
        {
            CWPviews = new List<CWPview>(); // Initialize the list
        }

        public int batchid { get; set; }
        public double? c1 { get; set; }
        public double? c2 { get; set; }
        public double? c3 { get; set; }
        public double? c4 { get; set; }
       
        public DateTime date { get; set; }
        public DateTime time { get; set; }


        public List<CWPview> CWPviews { get; set; }
    }
}
