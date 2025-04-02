namespace StudentRegistration.Models
{
    public class RecommendationDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string FacultyName { get; set; } 
        public string Semester { get; set; }
        public double AttendanceRate { get; set; }
        public string RecommendationMessage { get; set; }
    }
}
