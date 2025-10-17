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
    public class PrescriptionsController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _context;

        public PrescriptionsController(Clininc_DBCONTEXT context)
        {
            _context = context;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<PrescriptionResponseDto>>> GetPrescriptions()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            IQueryable<Prescription> query = _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient);

            if (currentUserRole == "Doctor")
            {
                query = query.Where(p => p.Doctor.UserId == currentUserId);
            }

            var prescriptions = await query
                .Select(p => new PrescriptionResponseDto
                {
                    Id = p.Id,
                    DateIssued = p.DateIssued,
                    MedicationName = p.MedicationName,
                    Dosage = p.Dosage,
                    Instructions = p.Instructions,
                    AppointmentId = p.AppointmentId,
                    DoctorId = p.DoctorId,
                    DoctorName = p.Doctor.FullName,
                    PatientId = p.PatientId,
                    PatientName = p.Patient.FullName
                })
                .ToListAsync();

            return Ok(prescriptions);
        }

        // 🔹 GET: api/prescriptions/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<PrescriptionResponseDto>> GetPrescription(int id)
        {
            var prescription = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
                return NotFound("Prescription not found.");

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Patient sees only his prescriptions
            if (currentUserRole == "Patient" && prescription.Patient.UserId != currentUserId)
                return Forbid();

            // Doctor sees only his prescriptions
            if (currentUserRole == "Doctor" && prescription.Doctor.UserId != currentUserId)
                return Forbid();

            return Ok(new PrescriptionResponseDto
            {
                Id = prescription.Id,
                DateIssued = prescription.DateIssued,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Instructions = prescription.Instructions,
                AppointmentId = prescription.AppointmentId,
                DoctorId = prescription.DoctorId,
                DoctorName = prescription.Doctor.FullName,
                PatientId = prescription.PatientId,
                PatientName = prescription.Patient.FullName
            });
        }

        // 🔹 POST: api/prescriptions
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreatePrescription([FromBody] PrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // التأكد من وجود المراجع
            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null)
                return NotFound("Doctor not found.");

            var patient = await _context.Patients.FindAsync(dto.PatientId);
            if (patient == null)
                return NotFound("Patient not found.");

            var appointment = await _context.Appointments.FindAsync(dto.AppointmentId);
            if (appointment == null)
                return NotFound("Appointment not found.");

            var prescription = new Prescription
            {
                MedicationName = dto.MedicationName.Trim(),
                Dosage = dto.Dosage?.Trim(),
                Instructions = dto.Instructions?.Trim(),
                AppointmentId = dto.AppointmentId,
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId
            };

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPrescription), new { id = prescription.Id }, new PrescriptionResponseDto
            {
                Id = prescription.Id,
                DateIssued = prescription.DateIssued,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Instructions = prescription.Instructions,
                AppointmentId = prescription.AppointmentId,
                DoctorId = prescription.DoctorId,
                DoctorName = doctor.FullName,
                PatientId = prescription.PatientId,
                PatientName = patient.FullName
            });
        }

        // 🔹 PUT: api/prescriptions/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] PrescriptionRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
                return NotFound("Prescription not found.");

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole == "Doctor" && prescription.Doctor.UserId != currentUserId)
                return Forbid();

            prescription.MedicationName = dto.MedicationName ?? prescription.MedicationName;
            prescription.Dosage = dto.Dosage ?? prescription.Dosage;
            prescription.Instructions = dto.Instructions ?? prescription.Instructions;
            prescription.AppointmentId = dto.AppointmentId;
            prescription.DoctorId = dto.DoctorId;
            prescription.PatientId = dto.PatientId;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Prescription updated successfully." });
        }

        // 🔹 DELETE: api/prescriptions/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
                return NotFound("Prescription not found.");

            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            if (currentUserRole == "Doctor" && prescription.Doctor.UserId != currentUserId)
                return Forbid();

            _context.Prescriptions.Remove(prescription);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Prescription deleted successfully." });
        }


    }
}
