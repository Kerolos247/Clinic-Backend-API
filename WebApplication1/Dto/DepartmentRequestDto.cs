using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class DepartmentRequestDto
    {
        [Required(ErrorMessage = "Department name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Section name must be between 3 and 100 characters.")]
        public string NameDepart { get; set; }
    }
}
