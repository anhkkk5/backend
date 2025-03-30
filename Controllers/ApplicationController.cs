using InternshipManagement.Models;
using InternshipManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    [Route("api/applications")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IRepository<Application> _applicationRepository;
        private readonly IRepository<InternshipPosition> _positionRepository;
        private readonly IRepository<Company> _companyRepository;

        public ApplicationController(
            IRepository<Application> applicationRepository,
            IRepository<InternshipPosition> positionRepository,
            IRepository<Company> companyRepository)
        {
            _applicationRepository = applicationRepository;
            _positionRepository = positionRepository;
            _companyRepository = companyRepository;
        }

        [HttpGet]
        [Authorize(Roles = "student,company")]
        public async Task<IActionResult> GetAll()
        {
            var accountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "student")
            {
                var applications = (await _applicationRepository.GetAllAsync())
                    .Where(a => a.StudentId == accountId);
                return Ok(applications);
            }
            else // company
            {
                var company = (await _companyRepository.GetAllAsync()).FirstOrDefault(c => c.AccountId == accountId);
                if (company == null) return NotFound("Company not found");
                var applications = (await _applicationRepository.GetAllAsync())
                    .Where(a => a.CompanyId == company.Id);
                return Ok(applications);
            }
        }

        [HttpPost]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto)
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized();
            var accountId = int.Parse(accountIdValue);

            var position = await _positionRepository.GetByIdAsync(dto.PositionId);
            if (position == null || position.Status != "open")
                return BadRequest("Invalid or closed position");

            var application = new Application
            {
                StudentId = accountId,
                CompanyId = position.CompanyId,
                PositionId = dto.PositionId,
                Status = "pending",
                InterviewDate = null,
                InterviewTime = string.Empty,
                InterviewLocation = string.Empty
            };

            await _applicationRepository.CreateAsync(application);
            return CreatedAtAction(nameof(GetAll), application);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "company")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusModel model)
        {
            var application = await _applicationRepository.GetByIdAsync(id);
            if (application == null) return NotFound();

            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized();
            var accountId = int.Parse(accountIdValue);
            var company = (await _companyRepository.GetAllAsync()).FirstOrDefault(c => c.AccountId == accountId);
            if (company == null || application.CompanyId != company.Id) return Forbid();

            application.Status = model.Status;
            if (model.Status == "interviewing")
            {
                if (model.InterviewDate == null || string.IsNullOrEmpty(model.InterviewTime) || string.IsNullOrEmpty(model.InterviewLocation))
                    return BadRequest("Interview details required for 'interviewing' status");
                application.InterviewDate = model.InterviewDate;
                application.InterviewTime = model.InterviewTime;
                application.InterviewLocation = model.InterviewLocation;
            }
            else
            {
                application.InterviewDate = null;
                application.InterviewTime = string.Empty;
                application.InterviewLocation = string.Empty;
            }

            await _applicationRepository.UpdateAsync(application);
            return Ok(new { Message = "Application status updated" });
        }
    }

    public class UpdateStatusModel
    {
        public string Status { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewTime { get; set; }
        public string InterviewLocation { get; set; }
    }
}