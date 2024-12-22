using StudentRegistration.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable enable

namespace StudentRegidtration.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string UserName { get; set; }    
        public string Password { get; set; }
        public int? TeacherId  { get; set; }
        public int? StudentId  { get; set; }
        public string Role { get; set; }

        [JsonIgnore]
        public Teacher? Teacher { get; set;}
        [JsonIgnore]
        public Student? Student { get; set; }
    }
}
