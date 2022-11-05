namespace ImprovedTransportManager.Data
{
    public sealed class WealthPassengerReport : BasicReportData
    {
        public long Low { get; set; }
        public long Medium { get; set; }
        public long High { get; set; }
        public long Total => Low + Medium + High;
    }
}