using StudentRegistration.Data;

namespace StudentRegistration.Controllers
{
    internal class ApplicationDbContext
    {
        public object AttendanceDetails { get; internal set; }

        public static implicit operator ApplicationDbContext(AppDBContext v)
        {
            throw new NotImplementedException();
        }
    }
}