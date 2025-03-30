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

        public StudentController(IRepository<Student> studentRepository)
        {
            _studentRepository = studentRepository;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized();
            var accountId = int.Parse(accountIdValue);
            var students = await _studentRepository.GetAllAsync();
            Console.WriteLine($"Total students: {students.Count}, AccountId: {accountId}");
            var profile = students.FirstOrDefault(s => s.AccountId == accountId);
            if (profile == null)
            {
                Console.WriteLine($"Profile not found for AccountId: {accountId}");
                return NotFound();
            }
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] Student profile)
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            profile.AccountId = accountId;
            await _studentRepository.UpdateAsync(profile);
            return Ok(new { Message = "Profile updated successfully" });
        }
    }
}