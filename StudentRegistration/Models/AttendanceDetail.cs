using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable enable
namespace StudentRegistration.Models
{
    public class AttendanceDetail
    {
        [Key]
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string Status { get; set; }
        [JsonIgnore]
        public Attendance? Attendance { get; set; }
        [JsonIgnore]
        public Student? Student { get; set; } 

    }
}
