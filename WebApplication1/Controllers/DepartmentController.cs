using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _context;
        public DepartmentController(Clininc_DBCONTEXT dBCONTEXT)
        {
            _context = dBCONTEXT;
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> CreateDepartment([FromBody] DepartmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool exists = await _context.Departments.AnyAsync(d => d.Name.ToLower() == dto.NameDepart.ToLower());
            if (exists)
                return Conflict(new { Message = $"Department '{dto.NameDepart}' already exists." });

            var department = new Department
            {
                Name = dto.NameDepart.Trim()
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, new
            {
                department.Id,
                department.Name
            });
        }
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDepartmentById(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
                return NotFound(new { Message = "Department not found." });

            return Ok(new
            {
                department.Id,
                department.Name
            });
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetAllDepartments()
        {
            var departs = await _context.Departments
                .Select(d => new DepartmentResponseDto
                {
                    Id = d.Id,
                    Name = d.Name
                })
                .ToListAsync();

            return Ok(departs);
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
           
            var department = await _context.Departments
                .Include(d => d.Doctors)
                .FirstOrDefaultAsync(d => d.Id == id);

           
            if (department == null)
                return NotFound(new { Message = "Department not found." });

            
            if (department.Doctors != null && department.Doctors.Any())
                return BadRequest(new { Message = "Cannot delete department that has assigned doctors." });

           
            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

           
            return Ok(new { Message = $"Department '{department.Name}' deleted successfully." });
        }







    }
}
