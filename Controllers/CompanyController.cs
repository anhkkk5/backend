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

        public CompanyController(IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var company = (await _companyRepository.GetAllAsync()).FirstOrDefault(c => c.AccountId == accountId);
            if (company == null) return NotFound();
            return Ok(company);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] Company company)
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            company.AccountId = accountId;
            await _companyRepository.UpdateAsync(company);
            return Ok(new { Message = "Company profile updated successfully" });
        }
    }
}