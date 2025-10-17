using WebApplication1.Models;

namespace WebApplication1.Dto
{
    public class UserResponseDto
    {
        public string Id { get; set; }
        public string FullName {  get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}
