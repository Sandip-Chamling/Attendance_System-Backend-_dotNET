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

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly AppDBContext context;

        public StudentController(AppDBContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents()
        {
            var students = await context.Students
                .Include(s => s.Faculty)
                .Where(s => s.IsDeleted == false || s.IsDeleted == null)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();

            var studentDtos = students
                .Select(s => new StudentDto
                {
                    Id = s.Id,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Address = s.Address,
                    DateOfBirth = s.DateOfBirth.ToString("yyyy-MM-dd"),
                    Gender = s.Gender,
                    Email = s.Email,
                    Contact = s.Contact,
                    FacultyId = s.FacultyId,
                    FacultyName = s.Faculty.FacultyName,
                    Batch = s.Batch,
                    Semester = s.Semester,
                   })
                  .ToList();

            return Ok(studentDtos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudents(int id)
        {
            var students = await context.Students
                .Include(s => s.Faculty)
                .Where(s => s.IsDeleted == false || s.IsDeleted == null)
                .FirstOrDefaultAsync(s => s.Id == id);

            var studentDtos = new StudentDto
            {
                Id = students.Id,
                FirstName = students.FirstName,
                LastName = students.LastName,
                Address = students.Address,
                DateOfBirth = students.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = students.Gender,
                Email = students.Email,
                Contact = students.Contact,
                FacultyId = students.FacultyId,
                FacultyName = students.Faculty.FacultyName,
                Batch = students.Batch,
                Semester = students.Semester,
            };
                  

            return Ok(studentDtos);
        }

        [HttpGet("facultyId={facultyId},semester={semester}")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByFaculty(int facultyId, string semester)
        {
            var students = await context.Students
                .Include(s => s.Faculty)
                .Where(s => (s.IsDeleted == false || s.IsDeleted == null) &&
                             s.FacultyId == facultyId &&
                             s.Semester == semester)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();

            var studentDtos = students.Select(student => new StudentDto
            {
                Id = student.Id,
                FirstName = student.FirstName,
                LastName = student.LastName,
                Address = student.Address,
                DateOfBirth = student.DateOfBirth.ToString("yyyy-MM-dd"),
                Gender = student.Gender,
                Email = student.Email,
                Contact = student.Contact,
                FacultyId = student.FacultyId,
                FacultyName = student.Faculty.FacultyName,
                Batch = student.Batch,
                Semester = student.Semester,
            }).ToList();

            return Ok(studentDtos);
        }



        [HttpPost]
        public async Task<IActionResult> PostStudent([FromBody] Student student)
        {
            if (student == null)
            {
                return BadRequest("Student data is null.");
            }

           
            await context.Students.AddAsync(student);
            await context.SaveChangesAsync();
            var generatedPassword = GeneratePassword(5);



            var user = new User
            {
                UserName = student.Contact,
                Password = HashPassword(generatedPassword), 
                Role = "student",
                StudentId = student.Id
            };

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            await SendPasswordToUser(student.Email,user.UserName, generatedPassword, student.FirstName, student.LastName);


            return Ok(generatedPassword); 
        }

        private string GeneratePassword(int length)
        {
            const string validChars = "abcdefghjkmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
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

        private async Task SendPasswordToUser(string email, string userName,string password,string fname, string lname)
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
        public async Task<ActionResult<Student>> PutStudent(int id, Student updatedStudent)
        {
            var existingStudent = await context.Students
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null) return NotFound();

            context.Entry(existingStudent).CurrentValues.SetValues(updatedStudent);
            await context.SaveChangesAsync();
            return Ok(existingStudent);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Student>> DeleteStudent(int id)
        {
            var student = await context.Students
              .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
            {
                return NotFound();
            }


            student.IsDeleted = true;

            await context.SaveChangesAsync();

            return Ok(student);

        }

    }
}


