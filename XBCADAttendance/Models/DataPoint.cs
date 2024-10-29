namespace XBCADAttendance.Models
{
    public class DataPoint
    {
        public string Name { get; set; }
        public int Y { get; set; }

        public DataPoint(string name, int y)
        {
            Name = name;
            Y = y;
        }
    }
}
