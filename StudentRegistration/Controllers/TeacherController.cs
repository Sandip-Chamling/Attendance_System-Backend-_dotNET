using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Data;
using StudentRegistration.Models;
using System.Threading.Tasks;
using BCrypt.Net;
using StudentRegidtration.Models;
using System.Net.Mail;
using System.Net;
using StudentRegistration.DTO;
using StudentRegidtration.DTO;

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeacherController : ControllerBase
    {
        private readonly AppDBContext context;

        public TeacherController(AppDBContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeacherDto>>> GetTeachers()
        {
            var Teacher = await context.Teachers
                .Include(t => t.Faculty)
                .Where(t => t.IsDeleted == false || t.IsDeleted == null)
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .ToListAsync();


            var teacherDtos = Teacher
                .Select(t => new TeacherDto
                {
                    Id = t.Id,
                    FirstName = t.FirstName,
                    LastName = t.LastName,
                    Address = t.Address,
                    DateOfBirth = t.DateOfBirth.ToString("yyyy-MM-dd"),
                    Gender = t.Gender,
                    Email = t.Email,
                    Contact = t.Contact,
                    FacultyId = t.FacultyId,
                    FacultyName = t.Faculty.FacultyName,
                    Semester = t.Semester,
                })
                  .ToList();

            return Ok(teacherDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<TeacherDto>>> GetTeachers(int id)
        {
            var teachers = await context.Teachers
                .Include(t => t.Faculty)
                .Where(t => t.IsDeleted == false || t.IsDeleted == null)
                .FirstOrDefaultAsync(t => t.Id == id);

            var teacherDtos = new TeacherDto
            {
                Id = teachers.Id,
                FirstName = teachers.FirstName,
                LastName = teachers.LastName,
                Address = teachers.Address,
                DateOfBirth = teachers.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = teachers.Gender,
                Email = teachers.Email,
                Contact = teachers.Contact,
                FacultyId = teachers.FacultyId,
                FacultyName = teachers.Faculty.FacultyName,
                Semester = teachers.Semester,
            };


            return Ok(teacherDtos);
        }


        [HttpPost]
        public async Task<IActionResult> PostTeacher([FromBody] Teacher teacher)
        {
            if (teacher == null)
            {
                return BadRequest("Teacher data is null.");
            }


            await context.Teachers.AddAsync(teacher);
            await context.SaveChangesAsync();
            var generatedPassword = GeneratePassword(5);



            var user = new User
            {
                UserName = teacher.Contact,
                Password = HashPassword(generatedPassword),
                Role = "teacher",
                TeacherId = teacher.Id
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await SendPasswordToUser(teacher.Email, user.UserName, generatedPassword, teacher.FirstName, teacher.LastName);


            return Ok(generatedPassword);
        }

        private string GeneratePassword(int length)
        {
            const string validChars = "abcdefghjkmnpqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            var res = new char[length];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[length];
                rng.GetBytes(randomBytes);
                for (int i = 0; i < length; i++)
                {
                    res[i] = validChars[randomBytes[i] % validChars.Length];
                }
            }
            return new string(res);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private async Task SendPasswordToUser(string email, string userName, string password, string fname, string lname)
        {
            var mailMessage = new MailMessage("sandipchamling6286@gmail.com", email)
            {
                Subject = "Your Password",
                Body = $"Dear {fname} {lname},<br/><br/>" +
               $"Your Username: {userName}<br/>" +
               $"Your Password: {password}<br/><br/>" +
               "Please keep these credentials safe and do not share them with anyone.",
                IsBodyHtml = true,
            };

            using (var smtpClient = new SmtpClient("smtp.gmail.com", 587)) // Gmail SMTP server
            {
                smtpClient.Credentials = new NetworkCredential("sandipchamling6286@gmail.com", "mjksvfqukkqimvjb");
                smtpClient.EnableSsl = true;

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (SmtpException ex)
                {

                    Console.WriteLine($"SMTP Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending email: {ex.Message}");
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult<Teacher>> PutStudent(int id, Teacher updatedTeacher)
        {
            var existingTeacher = await context.Teachers
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTeacher == null) return NotFound();

            context.Entry(existingTeacher).CurrentValues.SetValues(updatedTeacher);
            await context.SaveChangesAsync();
            return Ok(existingTeacher);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Teacher>> DeleteTeacher(int id)
        {
            var teacher = await context.Teachers
              .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }


            teacher.IsDeleted = true;

            await context.SaveChangesAsync();

            return Ok(teacher);

        }

    }
}
