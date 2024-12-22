namespace StudentRegistration.DTO
{
    public class AttendanceInputDto
    {
        public string Date { get; set; } 
        public List<AttendanceDetailsDto> AttendanceDetails { get; set; }
    }

    public class AttendanceDetailsDto
    {
        public int StudentId { get; set; }
        public string Status { get; set; }
    }
}
