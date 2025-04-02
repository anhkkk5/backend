using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace InternshipManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            var user = _context.Accounts.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user == null)
            {
                return Unauthorized("Invalid username or password");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }

            var user = _context.Accounts.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user == null)
            {
                return NotFound("User not found in database");
            }

            return Ok(new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                role = role
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Register([FromBody] CreateAccountDto model)
        {
            // Kiểm tra username hoặc email đã tồn tại
            if (_context.Accounts.Any(u => u.Username == model.Username))
            {
                return BadRequest("Username already exists");
            }
            if (_context.Accounts.Any(u => u.Email == model.Email))
            {
                return BadRequest("Email already exists");
            }

            // Kiểm tra role hợp lệ
            var validRoles = new[] { "admin", "company", "student" };
            if (!validRoles.Contains(model.Role.ToLower()))
            {
                return BadRequest("Invalid role. Role must be 'admin', 'company', or 'student'.");
            }

            // Tạo tài khoản mới
            var account = new Account
            {
                Username = model.Username,
                Password = model.Password, // Lưu ý: Nên mã hóa mật khẩu trong thực tế
                Email = model.Email,
                Role = model.Role.ToLower()
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Account created successfully", accountId = account.Id });
        }

        private string GenerateJwtToken(Account user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var secret = _configuration["JwtSettings:Secret"];
            if (string.IsNullOrEmpty(secret)) throw new InvalidOperationException("JWT Secret is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}