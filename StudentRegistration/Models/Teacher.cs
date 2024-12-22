using StudentRegistration.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable enable

namespace StudentRegidtration.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public int FacultyId { get; set; }
        public string Semester { get; set; }
        [JsonIgnore]
        public bool? IsDeleted { get; set; } = false;

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Faculty? Faculty { get; set; }
    }
}
