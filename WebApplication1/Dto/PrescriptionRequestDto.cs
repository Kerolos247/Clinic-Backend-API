using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class PrescriptionRequestDto
    {
        [Required(ErrorMessage = "Medication name is required.")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Medication name must be between 1 and 200 characters.")]
        public string MedicationName { get; set; }

        [StringLength(100, ErrorMessage = "Dosage cannot exceed 100 characters.")]
        public string Dosage { get; set; }

        [StringLength(500, ErrorMessage = "Instructions cannot exceed 500 characters.")]
        public string Instructions { get; set; }

        [Required(ErrorMessage = "AppointmentId is required.")]
        public int AppointmentId { get; set; }

        [Required(ErrorMessage = "DoctorId is required.")]
        public int DoctorId { get; set; }

        [Required(ErrorMessage = "PatientId is required.")]
        public int PatientId { get; set; }
    }
}
