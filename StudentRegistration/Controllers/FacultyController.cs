using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentRegistration.Data;
using StudentRegistration.DTO;
using StudentRegistration.Models;

namespace StudentRegistration.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultyController : ControllerBase
    {
        private readonly AppDBContext context;

        public FacultyController(AppDBContext context)
        {
            this.context = context;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Faculty>>> GetFacultys()
        {
            var Faculty = await context.Faculty.ToListAsync();

            return Ok(Faculty);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Faculty>> GetFacultyById(int id)
        {
            var Faculty = await context.Faculty
                .FirstOrDefaultAsync(p => p.Id == id);

            return Ok(Faculty);
        }

        [HttpPost]
        public async Task<ActionResult<Faculty>> PostFacultys(Faculty data)
        {
            context.Faculty.Add(data);
            await context.SaveChangesAsync();
            return Ok(data);   
        }

    }
}
