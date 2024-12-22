namespace StudentRegistration.DTO
{
    public class LoginDto
    {
        //public int UserId { get; set; }
       // public string UserName { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }  
        public string Semester { get; set; }
        public string Faculty { get; set; }
        public int? FacultyId { get; set; } 
        public int? StudentId { get; set; }  
        public int? TeacherId { get; set; }  
    }
}
