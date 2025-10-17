namespace WebApplication1.Dto
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public decimal ConsultationFee { get; set; }

        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }

        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
