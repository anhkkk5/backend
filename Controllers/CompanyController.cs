using InternshipManagement.Data;
using InternshipManagement.Models;
using InternshipManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    [Route("api/companies")]
    [ApiController]
    [Authorize(Roles = "company")]
    public class CompanyController : ControllerBase
    {
        private readonly IRepository<Company> _companyRepository;
        private readonly AppDbContext _context;

        public CompanyController(IRepository<Company> companyRepository, AppDbContext context)
        {
            _companyRepository = companyRepository;
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

            var profile = _context.Companies.FirstOrDefault(c => c.AccountId == accountId);
            if (profile == null)
            {
                Console.WriteLine($"Profile not found for AccountId: {accountId}");
                return NotFound("Company profile not found.");
            }

            Console.WriteLine($"Profile found for AccountId: {accountId}, CompanyId: {profile.Id}");
            return Ok(profile);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] Company company)
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
            if (string.IsNullOrEmpty(company.Name) || string.IsNullOrEmpty(company.Address) ||
                string.IsNullOrEmpty(company.Contact) || string.IsNullOrEmpty(company.Description))
            {
                Console.WriteLine($"Invalid company data for AccountId: {accountId}");
                return BadRequest("All company fields (Name, Address, Contact, Description) are required.");
            }

            // Tìm hồ sơ hiện có
            var existingCompany = _context.Companies.FirstOrDefault(c => c.AccountId == accountId);
            if (existingCompany == null)
            {
                // Tạo mới nếu chưa có
                var newCompany = new Company
                {
                    AccountId = accountId,
                    Name = company.Name,
                    Address = company.Address,
                    Contact = company.Contact,
                    Description = company.Description
                };
                await _companyRepository.CreateAsync(newCompany);
                Console.WriteLine($"Company profile created for AccountId: {accountId}, CompanyId: {newCompany.Id}");
            }
            else
            {
                // Cập nhật nếu đã có
                existingCompany.Name = company.Name;
                existingCompany.Address = company.Address;
                existingCompany.Contact = company.Contact;
                existingCompany.Description = company.Description;
                await _companyRepository.UpdateAsync(existingCompany);
                Console.WriteLine($"Company profile updated for AccountId: {accountId}, CompanyId: {existingCompany.Id}");
            }

            return Ok(new { Message = "Company profile updated successfully" });
        }
    }
}