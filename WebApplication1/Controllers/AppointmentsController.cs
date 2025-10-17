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
    public class AppointmentsController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(Clininc_DBCONTEXT context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User);

           
            if (currentUserRole == "Doctor")
            {
                query = query.Where(a => a.Doctor.UserId == currentUserId);
            }
            
            else if (currentUserRole == "Patient")
            {
                query = query.Where(a => a.Patient.UserId == currentUserId);
            }

            var appointments = await query
                .Select(a => new AppointmentResponseDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status.ToString(),
                    Notes = a.Notes,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.FullName,
                    PatientId = a.PatientId,
                    PatientName = a.Patient.FullName
                })
                .ToListAsync();

            return Ok(appointments);
        }

        // 🔹 GET: api/appointments/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<AppointmentResponseDto>> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

           
            if (currentUserRole == "Doctor" && appointment.Doctor.UserId != currentUserId)
                return Forbid();

            if (currentUserRole == "Patient" && appointment.Patient.UserId != currentUserId)
                return Forbid();

            return Ok(new AppointmentResponseDto
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status.ToString(),
                Notes = appointment.Notes,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor.FullName,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient.FullName
            });
        }

        // 🔹 POST: api/appointments
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            var patient = await _context.Patients.FindAsync(dto.PatientId);

            if (doctor == null || patient == null)
                return NotFound(new { Message = "Doctor or Patient not found." });

           
            bool exists = await _context.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId && a.AppointmentDate == dto.AppointmentDate);

            if (exists)
                return BadRequest(new { Message = "This doctor already has an appointment at this time." });

            var appointment = new Appointment
            {
                AppointmentDate = dto.AppointmentDate,
                Status = dto.Status,
                Notes = dto.Notes,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAppointment), new { id = appointment.Id }, new
            {
                appointment.Id,
                appointment.AppointmentDate,
                Status = appointment.Status.ToString(),
                appointment.Notes,
                appointment.DoctorId,
                DoctorName = doctor.FullName,
                appointment.PatientId,
                PatientName = patient.FullName
            });
        }

        // 🔹 PUT: api/appointments/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole == "Doctor" && appointment.Doctor.UserId != currentUserId)
                return Forbid();

           
            appointment.AppointmentDate = dto.AppointmentDate;
            appointment.Status = dto.Status;
            appointment.Notes = dto.Notes ?? appointment.Notes;
            appointment.DoctorId = dto.DoctorId;
            appointment.PatientId = dto.PatientId;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment updated successfully." });
        }

        // 🔹 DELETE: api/appointments/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment deleted successfully." });
        }


    }
}
