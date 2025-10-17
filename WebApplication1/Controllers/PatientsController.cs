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
    public class PatientsController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(Clininc_DBCONTEXT context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<PatientResponseDto>>> GetPatients()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            IQueryable<Patient> query = _context.Patients.Include(p => p.User);

            
            if (currentUserRole == "Doctor")
            {
                query = query
                    .Where(p => _context.Appointments
                        .Any(a => a.PatientId == p.Id && a.Doctor.UserId == currentUserId));
            }

            var patients = await query
                .Select(p => new PatientResponseDto
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    Address = p.Address,
                    PhoneNumber = p.PhoneNumber,
                    UserId = p.UserId,
                    Username = p.User.UserName,
                    Email = p.User.Email
                })
                .ToListAsync();

            return Ok(patients);
        }

        // 🔹 GET: api/patients/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<PatientResponseDto>> GetPatient(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new { Message = "Patient not found." });

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

          
            if (currentUserRole == "Patient" && patient.UserId != currentUserId)
                return Forbid();

           
            if (currentUserRole == "Doctor")
            {
                bool hasAppointment = await _context.Appointments
                    .Include(a => a.Doctor)
                    .AnyAsync(a => a.PatientId == id && a.Doctor.UserId == currentUserId);

                if (!hasAppointment)
                    return Forbid("You are not authorized to view this patient's data.");
            }

           
            return Ok(new PatientResponseDto
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Address = patient.Address,
                PhoneNumber = patient.PhoneNumber,
                UserId = patient.UserId,
                Username = patient.User?.UserName,
                Email = patient.User?.Email
            });
        }
        // 🔹 POST: api/patients
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePatient([FromBody] PatientRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            bool exists = await _context.Patients.AnyAsync(p => p.UserId == dto.UserId);
            if (exists)
                return BadRequest(new { Message = "This user is already assigned as a patient." });

            var patient = new Patient
            {
                FullName = dto.FullName.Trim(),
                Address = dto.Address.Trim(),
                PhoneNumber = dto.PhoneNumber.Trim(),
                UserId = dto.UserId
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, new
            {
                patient.Id,
                patient.FullName,
                patient.Address,
                patient.PhoneNumber,
                patient.UserId
            });
        }

        // 🔹 PUT: api/patients/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound(new { Message = "Patient not found." });

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

           
            if (currentUserRole != "Admin" && patient.UserId != currentUserId)
                return Forbid();

            patient.FullName = dto.FullName ?? patient.FullName;
            patient.Address = dto.Address ?? patient.Address;
            patient.PhoneNumber = dto.PhoneNumber ?? patient.PhoneNumber;

            if (currentUserRole == "Admin" && !string.IsNullOrEmpty(dto.UserId))
                patient.UserId = dto.UserId;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Patient updated successfully.",
                patient.Id,
                patient.FullName,
                patient.Address,
                patient.PhoneNumber,
                patient.UserId
            });
        }

        // 🔹 DELETE: api/patients/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.Appointments)
                .Include(p => p.MedicalRecords)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new { Message = "Patient not found." });

            if (patient.Appointments.Any() || patient.MedicalRecords.Any())
                return BadRequest(new { Message = "Cannot delete patient with existing appointments or medical records." });

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Patient deleted successfully." });
        }
    }
}
