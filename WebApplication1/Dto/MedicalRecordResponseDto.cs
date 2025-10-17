namespace WebApplication1.Dto
{
    public class MedicalRecordResponseDto
    {
        public int Id { get; set; }
        public string Diagnosis { get; set; }
        public string Prescription { get; set; }
        public DateTime RecordDate { get; set; }

        public int PatientId { get; set; }
        public string PatientName { get; set; }

        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
    }
}
