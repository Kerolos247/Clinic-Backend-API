using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _Context;
        private readonly UserManager<ApplicationUser> _userManager;
        public DoctorsController(Clininc_DBCONTEXT dBCONTEXT,
           UserManager<ApplicationUser> userManager)
        {
            _Context = dBCONTEXT;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<DoctorResponseDto>>> GetDoctors()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            var query = _Context.Doctors
                .Include(d => d.Department)
                .Include(d => d.User)
                .AsQueryable();

            if (currentUserRole == "Doctor")
                query = query.Where(d => d.UserId == currentUserId);

            var doctors = await query.Select(d => new DoctorResponseDto
            {
                Id = d.Id,
                FullName = d.FullName,
                ConsultationFee = d.ConsultationFee,
                DepartmentId = d.DepartmentId,
                DepartmentName = d.Department.Name,
                UserId = d.UserId,
                Username = d.User.UserName,
                Email = d.User.Email
            }).ToListAsync();

            return Ok(doctors);
        }
        // 🔹 GET: api/doctors/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<DoctorResponseDto>> GetDoctor(int id)
        {
            var doctor = await _Context.Doctors
                .Include(d => d.Department)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole == "Doctor" && doctor.UserId != currentUserId)
                return Forbid();

            return Ok(new DoctorResponseDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                ConsultationFee = doctor.ConsultationFee,
                DepartmentId = doctor.DepartmentId,
                DepartmentName = doctor.Department?.Name,
                UserId = doctor.UserId,
                Username = doctor.User?.UserName,
                Email = doctor.User?.Email
            });
        }
        // 🔹 POST: api/doctors
        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<ActionResult> CreateDoctor([FromBody] DoctorRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            
            var department = await _Context.Departments.FindAsync(dto.DepartmentId);
            if (department == null)
                return NotFound(new { Message = "Department not found." });

            
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { Message = "User not found." });

           
            bool doctorExists = await _Context.Doctors.AnyAsync(d => d.UserId == dto.UserId);
            if (doctorExists)
                return BadRequest(new { Message = "This user is already assigned as a doctor." });

            var doctor = new Doctor
            {
                FullName = dto.FullName,
                ConsultationFee = dto.ConsultationFee,
                DepartmentId = dto.DepartmentId,
                UserId = dto.UserId
            };

            _Context.Doctors.Add(doctor);
            await _Context.SaveChangesAsync();

          
            return CreatedAtAction(nameof(GetDoctor), new { id = doctor.Id }, new
            {
                doctor.Id,
                doctor.FullName,
                doctor.ConsultationFee,
                doctor.DepartmentId,
                DepartmentName = department.Name,
                doctor.UserId
            });
        }
        //🔹 PUT: api/doctors/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _Context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { Message = "Doctor not found." });

           
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole != "Admin" && doctor.UserId != currentUserId)
                return Forbid(); 

            
            var departmentExists = await _Context.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
            if (!departmentExists)
                return BadRequest(new { Message = "Invalid DepartmentId." });

            
            doctor.FullName = dto.FullName ?? doctor.FullName;
            if (dto.ConsultationFee > 0)
                doctor.ConsultationFee = dto.ConsultationFee;
            doctor.DepartmentId = dto.DepartmentId;

            
            if (currentUserRole == "Admin" && !string.IsNullOrEmpty(dto.UserId))
                doctor.UserId = dto.UserId;

            await _Context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Doctor updated successfully.",
                doctor.Id,
                doctor.FullName,
                doctor.ConsultationFee,
                doctor.DepartmentId,
                doctor.UserId
            });
        }
        // 🔹 DELETE: api/doctors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _Context.Doctors
                .Include(d => d.Appointments)
                .Include(d => d.MedicalRecords)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(new { Message = "Doctor not found." });

           
            if (doctor.Appointments.Any() || doctor.MedicalRecords.Any())
                return BadRequest(new { Message = "Cannot delete doctor with existing appointments or medical records." });

            _Context.Doctors.Remove(doctor);
            await _Context.SaveChangesAsync();

            return Ok(new { Message = "Doctor deleted successfully." });
        }




    }
}
