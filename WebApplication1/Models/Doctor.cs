using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public decimal ConsultationFee { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        // 🔹 علاقة مع ApplicationUser
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // 🔹 Navigation Properties
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<MedicalRecord> MedicalRecords { get; set; }
    }
}
