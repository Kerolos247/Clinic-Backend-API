using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        public DateTime DateIssued { get; set; } = DateTime.Now;

        [Required]
        public string MedicationName { get; set; }

        public string Dosage { get; set; }

        public string Instructions { get; set; }

        
        [ForeignKey("Appointment")]
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
    }
}
