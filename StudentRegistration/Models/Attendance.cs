using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable enable

namespace StudentRegistration.Models
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        [JsonIgnore]
        public List<AttendanceDetail>? AttendanceDetails { get; set; } = new List<AttendanceDetail>();

    }
}
