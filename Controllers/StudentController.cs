using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    [Route("api/students")]
    [ApiController]
    [Authorize(Roles = "student")]
    public class StudentController : ControllerBase
    {
        private readonly IRepository<Student> _studentRepository;
        private readonly AppDbContext _context;

        public StudentController(IRepository<Student> studentRepository, AppDbContext context)
        {
            _studentRepository = studentRepository;
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue))
            {
                Console.WriteLine("AccountId not found in token.");
                return Unauthorized("AccountId not found in token.");
            }

            if (!int.TryParse(accountIdValue, out int accountId))
            {
                Console.WriteLine($"Invalid AccountId format: {accountIdValue}");
                return BadRequest("Invalid AccountId format.");
            }

            var profile = _context.Students.FirstOrDefault(s => s.AccountId == accountId);
            if (profile == null)
            {
                Console.WriteLine($"Profile not found for AccountId: {accountId}");
                return NotFound("Student profile not found.");
            }

            Console.WriteLine($"Profile found for AccountId: {accountId}, StudentId: {profile.Id}");
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] Student profile)
        {
            // Kiểm tra AccountId từ token
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue))
            {
                Console.WriteLine("AccountId not found in token.");
                return Unauthorized("AccountId not found in token.");
            }

            if (!int.TryParse(accountIdValue, out int accountId))
            {
                Console.WriteLine($"Invalid AccountId format: {accountIdValue}");
                return BadRequest("Invalid AccountId format.");
            }

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrEmpty(profile.Phone) || string.IsNullOrEmpty(profile.Address) ||
                string.IsNullOrEmpty(profile.Skills) || string.IsNullOrEmpty(profile.CvUrl))
            {
                Console.WriteLine($"Invalid profile data for AccountId: {accountId}");
                return BadRequest("All profile fields (Phone, Address, Skills, CvUrl) are required.");
            }

            // Tìm hồ sơ hiện có
            var existingProfile = _context.Students.FirstOrDefault(s => s.AccountId == accountId);
            if (existingProfile == null)
            {
                // Tạo mới nếu chưa có
                var newProfile = new Student
                {
                    AccountId = accountId,
                    Phone = profile.Phone,
                    Address = profile.Address,
                    Skills = profile.Skills,
                    CvUrl = profile.CvUrl
                };
                await _studentRepository.CreateAsync(newProfile);
                Console.WriteLine($"Profile created for AccountId: {accountId}, StudentId: {newProfile.Id}");
            }
            else
            {
                // Cập nhật nếu đã có
                existingProfile.Phone = profile.Phone;
                existingProfile.Address = profile.Address;
                existingProfile.Skills = profile.Skills;
                existingProfile.CvUrl = profile.CvUrl;
                await _studentRepository.UpdateAsync(existingProfile);
                Console.WriteLine($"Profile updated for AccountId: {accountId}, StudentId: {existingProfile.Id}");
            }

            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}