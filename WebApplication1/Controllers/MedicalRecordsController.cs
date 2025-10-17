using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
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
    public class MedicalRecordsController : ControllerBase
    {
        private readonly Clininc_DBCONTEXT _context;

        public MedicalRecordsController(Clininc_DBCONTEXT context)
        {
            _context = context;
        }
        // 🔹 GET: api/MedicalRecords
        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<ActionResult<IEnumerable<MedicalRecordResponseDto>>> GetMedicalRecords()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            IQueryable<MedicalRecord> query = _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor);

            // لو الدكتور فقط، يظهر السجلات الخاصة به فقط
            if (currentUserRole == "Doctor")
            {
                query = query.Where(m => m.Doctor.UserId == currentUserId);
            }

            var records = await query
                .Select(m => new MedicalRecordResponseDto
                {
                    Id = m.Id,
                    Diagnosis = m.Diagnosis,
                    Prescription = m.Prescription,
                    RecordDate = m.RecordDate,
                    PatientId = m.PatientId,
                    PatientName = m.Patient.FullName,
                    DoctorId = m.DoctorId,
                    DoctorName = m.Doctor.FullName
                })
                .ToListAsync();

            return Ok(records);
        }

        // 🔹 GET: api/MedicalRecords/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<ActionResult<MedicalRecordResponseDto>> GetMedicalRecord(int id)
        {
            var record = await _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (record == null)
                return NotFound("Medical record not found");

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // 🧩 المريض يرى سجله فقط
            if (currentUserRole == "Patient" && record.Patient.UserId != currentUserId)
                return Forbid();

            // 🧩 الدكتور يرى سجله فقط إذا هو المسؤول عنه
            if (currentUserRole == "Doctor" && record.Doctor.UserId != currentUserId)
                return Forbid();

            return Ok(new MedicalRecordResponseDto
            {
                Id = record.Id,
                Diagnosis = record.Diagnosis,
                Prescription = record.Prescription,
                RecordDate = record.RecordDate,
                PatientId = record.PatientId,
                PatientName = record.Patient.FullName,
                DoctorId = record.DoctorId,
                DoctorName = record.Doctor.FullName
            });
        }

        // 🔹 POST: api/MedicalRecords
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] MedicalRecordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var patient = await _context.Patients.FindAsync(dto.PatientId);
            if (patient == null)
                return NotFound("Patient not found");

            var doctor = await _context.Doctors.FindAsync(dto.DoctorId);
            if (doctor == null)
                return NotFound("Doctor not found");

            // لو الدكتور يحاول إضافة لسجل لمريض غيره
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserRole == "Doctor" && doctor.UserId != currentUserId)
                return Forbid("You are not authorized to create record for this doctor");

            var record = new MedicalRecord
            {
                Diagnosis = dto.Diagnosis.Trim(),
                Prescription = dto.Prescription.Trim(),
                RecordDate = dto.RecordDate,
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId
            };

            _context.MedicalRecords.Add(record);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMedicalRecord), new { id = record.Id }, new
            {
                record.Id,
                record.Diagnosis,
                record.Prescription,
                record.RecordDate,
                record.PatientId,
                PatientName = patient.FullName,
                record.DoctorId,
                DoctorName = doctor.FullName
            });
        }

        // 🔹 PUT: api/MedicalRecords/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateMedicalRecord(int id, [FromBody] MedicalRecordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound("Medical record not found");

            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // فقط Admin أو Doctor المسؤول عنه
            if (currentUserRole == "Doctor" && record.Doctor.UserId != currentUserId)
                return Forbid();

            // تحقق من وجود المريض والدكتور
            var patientExists = await _context.Patients.AnyAsync(p => p.Id == dto.PatientId);
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == dto.DoctorId);

            if (!patientExists || !doctorExists)
                return BadRequest("Invalid PatientId or DoctorId");

            record.Diagnosis = dto.Diagnosis ?? record.Diagnosis;
            record.Prescription = dto.Prescription ?? record.Prescription;
            record.RecordDate = dto.RecordDate;
            record.PatientId = dto.PatientId;
            record.DoctorId = dto.DoctorId;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Medical record updated successfully" });
        }

        // 🔹 DELETE: api/MedicalRecords/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedicalRecord(int id)
        {
            var record = await _context.MedicalRecords.FindAsync(id);
            if (record == null)
                return NotFound("Medical record not found");

            _context.MedicalRecords.Remove(record);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Medical record deleted successfully" });
        }

    }
}
