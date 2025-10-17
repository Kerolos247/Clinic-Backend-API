using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class MedicalRecordRequestDto
    {
        [Required(ErrorMessage = "Diagnosis is required")]
        [StringLength(500, ErrorMessage = "Diagnosis cannot exceed 500 characters")]
        public string Diagnosis { get; set; }

        [Required(ErrorMessage = "Prescription is required")]
        [StringLength(500, ErrorMessage = "Prescription cannot exceed 500 characters")]
        public string Prescription { get; set; }

        [Required(ErrorMessage = "Record date is required")]
        public DateTime RecordDate { get; set; }

        [Required(ErrorMessage = "PatientId is required")]
        public int PatientId { get; set; }

        [Required(ErrorMessage = "DoctorId is required")]
        public int DoctorId { get; set; }
    }
}
