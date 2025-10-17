namespace WebApplication1.Dto
{
    public class PrescriptionResponseDto
    {
        public int Id { get; set; }
        public DateTime DateIssued { get; set; }

        public string MedicationName { get; set; }
        public string Dosage { get; set; }
        public string Instructions { get; set; }

        public int AppointmentId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
    }
}
