namespace WebApplication1.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Navigation Property (كل قسم فيه أكتر من دكتور)
        public ICollection<Doctor> Doctors { get; set; }
    }
}
