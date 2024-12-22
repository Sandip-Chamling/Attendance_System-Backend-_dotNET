using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegidtration.DTO;
using StudentRegistration.Data;
using StudentRegistration.DTO;
using StudentRegistration.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly AppDBContext context;

        public AttendanceController(AppDBContext context)
        {
            this.context = context;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendanceDate()
        {
            var attendanceRecords = await context.Attendances
                .Include(a => a.AttendanceDetails)
                .ThenInclude(ad => ad.Student)
                .Where(a => a.AttendanceDetails.Any(ad => ad.Student.IsDeleted == false || ad.Student.IsDeleted == null))
                .ToListAsync();


            var attendanceDtos = attendanceRecords.Select(a => new AttendanceDto
            {
                id = a.Id,
                Date = a.Date.ToString("yyyy-MM-dd"),
                AttendanceDetails = a.AttendanceDetails.Select(ad => new AttendanceDetailDto
                {
                    StudentId = ad.Student.Id,
                    StudentName = $"{ad.Student.FirstName} {ad.Student.LastName}",
                    FacultyId = ad.Student.FacultyId,
                    Semester = ad.Student.Semester,
                    Status = ad.Status
                }).ToList()
            }).ToList();


            return Ok(attendanceDtos);
        }


        [HttpGet("Date={date},facultyId={facultyId},semester={semester}")]
        public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendanceDateById(string date, int facultyId, string semester)
        {
         
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest("Invalid date format.");
            }

            var attendanceRecords = await context.Attendances
                .Include(a => a.AttendanceDetails)
                .ThenInclude(ad => ad.Student)
                .Where(a => a.Date.Date == parsedDate.Date
                    && a.AttendanceDetails.Any(ad => ad.Student.IsDeleted == false
                        && ad.Student.FacultyId == facultyId
                        && ad.Student.Semester == semester))
                .Select(a => new
                {
                    Attendance = a,
                    SortedAttendanceDetails = a.AttendanceDetails
                        .Where(ad => ad.Student.IsDeleted == false
                            && ad.Student.FacultyId == facultyId
                            && ad.Student.Semester == semester)
                        .OrderBy(ad => ad.Student.FirstName)
                        .ThenBy(ad => ad.Student.LastName)
                        .ToList()
                })
                .ToListAsync();

            var attendanceDtos = attendanceRecords.Select(record => new AttendanceDto
            {
                id = record.Attendance.Id,
                Date = record.Attendance.Date.ToString("yyyy-MM-dd"),
                AttendanceDetails = record.SortedAttendanceDetails.Select(ad => new AttendanceDetailDto
                {
                    StudentId = ad.Student.Id,
                    StudentName = $"{ad.Student.FirstName} {ad.Student.LastName}",
                    FacultyId = ad.Student.FacultyId,
                    Semester = ad.Student.Semester,
                    Status = ad.Status
                }).ToList()
            }).ToList();

            return Ok(attendanceDtos);
        }

        [HttpGet("studentId={studentId},facultyId={facultyId},semester={semester},fromDate={fromDate},toDate={toDate}")]
        public async Task<ActionResult<AttendanceSummaryDto>> GetAttendanceSummaryByStudentAndDateRange(
                 int studentId, int facultyId, string semester, string fromDate, string toDate)
        {
           
            if (!DateTime.TryParse(fromDate, out var parsedFromDate) || !DateTime.TryParse(toDate, out var parsedToDate))
            {
                return BadRequest("Invalid date format for fromDate or toDate.");
            }

            
            if (parsedFromDate > parsedToDate)
            {
                return BadRequest("fromDate cannot be later than toDate.");
            }

           
            var attendanceRecords = await context.Attendances
                .Include(a => a.AttendanceDetails)
                .ThenInclude(ad => ad.Student)
                .Where(a => a.Date.Date >= parsedFromDate.Date && a.Date.Date <= parsedToDate.Date
                    && a.AttendanceDetails.Any(ad => ad.Student.IsDeleted == false
                        && ad.Student.Id == studentId
                        && ad.Student.FacultyId == facultyId
                        && ad.Student.Semester == semester))
                .ToListAsync();

            var totalPresentDays = 0;
            var totalAbsentDays = 0;

            foreach (var attendance in attendanceRecords)
            {
                var studentAttendance = attendance.AttendanceDetails
                    .FirstOrDefault(ad => ad.Student.Id == studentId);

                if (studentAttendance != null)
                {
                    if (studentAttendance.Status == "present")
                    {
                        totalPresentDays++;
                    }
                    else if (studentAttendance.Status == "absent")
                    {
                        totalAbsentDays++;
                    }
                }
            }

            var attendanceSummary = new AttendanceSummaryDto
            {
                StudentId = studentId,
                TotalPresentDays = totalPresentDays,
                TotalAbsentDays = totalAbsentDays,
                FromDate = parsedFromDate.ToString("yyyy-MM-dd"),
                ToDate = parsedToDate.ToString("yyyy-MM-dd")
            };

            return Ok(attendanceSummary);
        }


        [HttpPost("record")]
        public async Task<IActionResult> RecordAttendance([FromBody] AttendanceInputDto attendanceInput)
        {

            if (!DateTime.TryParse(attendanceInput.Date, out DateTime attendanceDate))
            {
                return BadRequest("Invalid date format.");
            }

            var attendanceRecord = new Attendance
            {
                Date = attendanceDate
            };

            context.Attendances.Add(attendanceRecord);
            await context.SaveChangesAsync();

            var attendanceDetails = new List<AttendanceDetail>();
            foreach (var detail in attendanceInput.AttendanceDetails)
            {
                var student = await context.Students.FindAsync(detail.StudentId);

                var attendanceDetail = new AttendanceDetail
                {
                    AttendanceId = attendanceRecord.Id,
                    StudentId = detail.StudentId,
                    Status = detail.Status
                };

                attendanceDetails.Add(attendanceDetail);
            }

            context.AttendanceDetails.AddRange(attendanceDetails);
            await context.SaveChangesAsync();

            return Ok("Attendance recorded successfully.");
        }



       
        [HttpPut("{attendanceId}")]
        public async Task<IActionResult> UpdateAttendanceDetails(int attendanceId, [FromBody] List<AttendanceUpdateDto> updatedDetails)
        {
            var attendanceDetails = await context.AttendanceDetails
                                                 .Where(a => a.AttendanceId == attendanceId)
                                                 .ToListAsync();

            context.AttendanceDetails.RemoveRange(attendanceDetails);

            foreach (var update in updatedDetails)
            {
                var newRecord = new AttendanceDetail
                {
                    AttendanceId = attendanceId,  
                    StudentId = update.StudentId,
                    Status = update.Status
                };

                await context.AttendanceDetails.AddAsync(newRecord);
            }          
           
                await context.SaveChangesAsync();

            return Ok("Updated");
        }

    }
}
