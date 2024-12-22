using StudentRegistration.Models;

namespace StudentRegistration.DTO
{
    public class AttendanceDto
    {
        public int id { get; set; }
        public string Date { get; set; }
        public List<AttendanceDetailDto> AttendanceDetails { get; set; } 

    }

    public class AttendanceDetailDto
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; }
        public int FacultyId { get; set; }
        public string Semester { get; set; }
        public string Status { get; set; }  
    }
}
