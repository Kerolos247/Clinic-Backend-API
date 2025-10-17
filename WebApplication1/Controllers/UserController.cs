using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Dto;
using Microsoft.AspNetCore.Identity;
using WebApplication1.Models;
using System.Security.Claims;
using WebApplication1.Configuration;
using Microsoft.AspNetCore.Authorization;
namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Clininc_DBCONTEXT _context;
        private readonly TokenService _tokenService; 


        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
            Clininc_DBCONTEXT dBCONTEXT, TokenService tokenService)
        {
            
            _userManager = userManager;
            _roleManager = roleManager;
            _context = dBCONTEXT;
            _tokenService = tokenService;
        }
        //login
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserLoginDto user)
        {
            var emailUser = await _userManager.FindByEmailAsync(user.Email);
            if (emailUser == null)
            {
                return BadRequest("Invalid email or password");
            }
            
            var passwordValid = await _userManager.CheckPasswordAsync(emailUser, user.Password);
            if (!passwordValid)
            {
                return BadRequest("Invalid email or password");
            }
 
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, emailUser.Id),
                new Claim(ClaimTypes.Email, emailUser.Email),
                new Claim(ClaimTypes.Name, emailUser.UserName)
            };

            var roles = await _userManager.GetRolesAsync(emailUser);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            
            var token = _tokenService.GenerateToken(claims);

            return Ok(new { Token = token });
        }
        // Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return BadRequest("Email already exists");

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var roleExists = await _roleManager.RoleExistsAsync(dto.Role);
            if (!roleExists)
                await _roleManager.CreateAsync(new IdentityRole(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            // نجهز الـ response DTO
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName= dto.FullName,
                Username = user.UserName,
                Email = user.Email,
                Role = dto.Role
            };

          
            return CreatedAtAction(
                nameof(GetUser),       
                new { id = user.Id }, 
                response             
            );
        }
        // Get all users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var response = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                response.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault()
                });
            }

            return Ok(response);
        }

        // Get single user by id
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(string id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole != "Admin" && currentUserId != id)
                return Forbid(); 


            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            var response = new UserResponseDto
            {
                Id = user.Id,
                FullName=user.FullName,
                Username = user.UserName,
                Email = user.Email,
                Role = roles.FirstOrDefault()
            };

            return Ok(response);
        }

        // Update user
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UserUpdateDto dto)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            if (currentUserRole != "Admin" && currentUserId != id)
                return Forbid(); // 403 Forbidden

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.UserName = dto.Username ?? user.UserName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            if (!string.IsNullOrEmpty(dto.Role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (!await _roleManager.RoleExistsAsync(dto.Role))
                    await _roleManager.CreateAsync(new IdentityRole(dto.Role));

                await _userManager.AddToRoleAsync(user, dto.Role);
            }

            var rolesUpdated = await _userManager.GetRolesAsync(user);

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName,
                Email = user.Email,
                Role = rolesUpdated.FirstOrDefault()
            });
        }
        // Delete user
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return NoContent();
        }

        // Delete all users
        [HttpDelete("delete-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(new { message = $"Failed to delete user {user.UserName}", errors = result.Errors });
                }
            }

            return Ok(new { message = "All users have been deleted successfully." });
        }






    }
}
