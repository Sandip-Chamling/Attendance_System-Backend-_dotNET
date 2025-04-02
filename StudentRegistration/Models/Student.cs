using StudentRegidtration.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
#nullable enable
namespace StudentRegistration.Models
{
    public class Student
    {
        internal readonly string? Name;

        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Contact { get; set; }
        public string Batch { get; set; }
        public string Semester { get; set; }
        public int FacultyId { get; set; }
        [JsonIgnore]
        public bool? IsDeleted { get; set;} = false;
        [JsonIgnore]
        public Faculty? Faculty { get; set; }
        [JsonIgnore]
        public List<AttendanceDetail>? AttendanceDetails { get; set; } = new List<AttendanceDetail>();
        [JsonIgnore]
        public User? User { get; set; }

    }
}
