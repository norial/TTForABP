namespace TTForABP.Models
{
        public class Experiment : DeviceExperiment
        {
            public int Id { get; set; }
            public string DeviceToken { get; set; }
            public string Key { get; set; }
            public string Options { get; set; }
        public string Distribution { get; set; }
        }

    public class DeviceExperiment
    {
        public int Id { get; set; }
        public string DeviceToken { get; set; }
        public int ExperimentId { get; set; }
        public Experiment Experiment { get; set; }
    }
}
