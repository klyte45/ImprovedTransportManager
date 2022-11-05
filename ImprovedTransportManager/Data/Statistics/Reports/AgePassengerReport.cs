namespace ImprovedTransportManager.Data
{
    public sealed class AgePassengerReport : BasicReportData
    {
        public long Child { get; set; }
        public long Teen { get; set; }
        public long Young { get; set; }
        public long Adult { get; set; }
        public long Elder { get; set; }
        public long Total => Child + Teen + Young + Adult + Elder;
    }
}