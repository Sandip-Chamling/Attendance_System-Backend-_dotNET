using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Data;
using StudentRegistration.Models;
using StudentRegistration.DTO;
using System.Threading.Tasks;
using BCrypt.Net;

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDBContext context;

        public AuthenticationController(AppDBContext context)
        {
            this.context = context;
        }

        [HttpGet("userName={userName},password={password}")]
        public async Task<ActionResult<LoginDto>> GetPassword(string userName, string password)
        {
            var user = await context.Users
                .Include(u => u.Student)
                .ThenInclude(s => s.Faculty) 
                .Include(u => u.Teacher)
                .ThenInclude(t => t.Faculty) 
                .FirstOrDefaultAsync(u => u.UserName == userName);

            
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return Unauthorized("Invalid username or password.");
            }

            
            var loginDto = new LoginDto
            {
                Role = user.Role
            };

            if (user.Role == "student" && user.Student != null)
            {
                loginDto.Name = $"{user.Student.FirstName} {user.Student.LastName}";
                loginDto.Semester = user.Student.Semester;
                loginDto.Faculty = user.Student.Faculty?.FacultyName;
                loginDto.FacultyId = user.Student.Faculty?.Id; // Set FacultyId for student
                loginDto.StudentId = user.Student.Id;
            }
            else if (user.Role == "teacher" && user.Teacher != null)
            {
                loginDto.Name = $"{user.Teacher.FirstName} {user.Teacher.LastName}";
                loginDto.Semester = user.Teacher.Semester;
                loginDto.Faculty = user.Teacher.Faculty?.FacultyName;
                loginDto.FacultyId = user.Teacher.Faculty?.Id; // Set FacultyId for teacher
                loginDto.TeacherId = user.Teacher.Id;
            }
            else if (user.Role == "admin")
            {
                loginDto.Name = "Admin User";
                loginDto.Semester = null;
                loginDto.Faculty = null;
                loginDto.FacultyId = null; // Admin does not have a FacultyId
            }

            return Ok(loginDto);
        }
    }
}
