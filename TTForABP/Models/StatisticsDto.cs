namespace TTForABP.Models
{
    public class StatisticsDto
    {
        public string Key { get; set; }
        public int TotalDevices { get; set; }
        public Dictionary<string, string> OptionsDistribution { get; set; }
    }
}
