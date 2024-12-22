namespace StudentRegidtration.DTO
{
    public class AttendanceSummaryDto
    {
        public int StudentId { get; set; }
        public int TotalPresentDays { get; set; }
        public int TotalAbsentDays { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }

}
