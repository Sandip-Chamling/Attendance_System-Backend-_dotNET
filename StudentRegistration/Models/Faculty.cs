using StudentRegidtration.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#nullable enable

namespace StudentRegistration.Models
{

    public class Faculty
    {
        [Key]
        public int Id { get; set; }
        public string FacultyName { get; set; }
        [JsonIgnore]
        public List<Student>? Students { get; set; }
        [JsonIgnore]
        public List<Teacher>? Teachers { get; set; }




    }
}
