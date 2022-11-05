namespace ImprovedTransportManager.Data
{
    public sealed class StudentsTouristsReport : BasicReportData
    {
        public long Total { get; set; }
        public long Student { get; set; }
        public long Tourists { get; set; }

        public float PercentageStudents => Total == 0 ? 0 : Student / (float)Total;
        public float PercentageTourists => Total == 0 ? 0 : Tourists / (float)Total;
    }
}