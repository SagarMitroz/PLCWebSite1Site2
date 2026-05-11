namespace Water_Filtration.Modals
{
    public class Alarm
    {
        public Alarm()
        {
            Alarms = new List<Alarm>(); // Initialize the list
        }

        public int Id { get; set; }
        public string TankName { get; set; }
        public double HH { get; set; }
        public double H { get; set; }
        public double L { get; set; }
        public double LL { get; set; }
        public List<Alarm> Alarms { get; set; }
      
    }
}
