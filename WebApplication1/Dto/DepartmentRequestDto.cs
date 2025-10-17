using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Dto
{
    public class DepartmentRequestDto
    {
        [Required(ErrorMessage = "اسم القسم مطلوب")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "اسم القسم يجب أن يكون بين 3 و 100 حرف")]
        public string NameDepart { get; set; }
    }
}
