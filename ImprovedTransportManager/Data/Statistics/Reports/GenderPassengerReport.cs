namespace ImprovedTransportManager.Data
{
    public sealed class GenderPassengerReport : BasicReportData
    {
        public long Male { get; set; }
        public long Female { get; set; }
        public long Total => Male + Female;
    }
}