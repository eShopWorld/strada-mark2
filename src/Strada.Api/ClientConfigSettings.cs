namespace Strada.Api
{
    public class ClientConfigSettings
    {
        public string ProjectId { get; set; }
        public string TopicId { get; set; }
        public long ElementCountThreshold { get; set; } = 1000;
        public long RequestByteThreshold { get; set; } = 5242880; // MAX bytes 10485760 / 2
        public int DelayThreshold { get; set; } = 3;
        public int MaxThreadCount { get; set; } = 10;
        public int ExecutionTimeInterval { get; set; } = 30;
        public int MaxQueueLength { get; set; } = 175000;
    }
}