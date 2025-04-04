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
        private readonly IRepository<Student> _studentRepository; // Thêm repository cho Student

        public ApplicationController(
            IRepository<Application> applicationRepository,
            IRepository<InternshipPosition> positionRepository,
            IRepository<Company> companyRepository,
            IRepository<Student> studentRepository) // Thêm Student repository vào constructor
        {
            _applicationRepository = applicationRepository;
            _positionRepository = positionRepository;
            _companyRepository = companyRepository;
            _studentRepository = studentRepository;
        }

        // GET: api/applications
        [HttpGet]
        [Authorize(Roles = "student,company")]
        public async Task<IActionResult> GetAll()
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized("AccountId not found in token.");
            if (!int.TryParse(accountIdValue, out int accountId))
                return BadRequest("Invalid AccountId format.");

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(role)) return Unauthorized("Role not found in token.");

            try
            {
                if (role == "student")
                {
                    var applications = (await _applicationRepository.GetAllAsync())
                        .Where(a => a.StudentId == accountId)
                        .Select(a => new
                        {
                            a.Id,
                            a.StudentId,
                            a.CompanyId,
                            a.PositionId,
                            a.Status,
                            a.InterviewDate,
                            a.InterviewTime,
                            a.InterviewLocation
                        });
                    return Ok(applications);
                }
                else // company
                {
                    var company = (await _companyRepository.GetAllAsync())
                        .FirstOrDefault(c => c.AccountId == accountId);
                    if (company == null) return NotFound("Company not found");

                    var applications = (await _applicationRepository.GetAllAsync())
                        .Where(a => a.CompanyId == company.Id)
                        .Select(a => new
                        {
                            a.Id,
                            a.StudentId,
                            a.CompanyId,
                            a.PositionId,
                            a.Status,
                            a.InterviewDate,
                            a.InterviewTime,
                            a.InterviewLocation
                        });
                    return Ok(applications);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching applications: {ex.Message}");
            }
        }

        // POST: api/applications
        [HttpPost]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto)
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized("AccountId not found in token.");
            if (!int.TryParse(accountIdValue, out int accountId))
                return BadRequest("Invalid AccountId format.");

            try
            {
                var position = await _positionRepository.GetByIdAsync(dto.PositionId);
                if (position == null || position.Status != "open")
                    return BadRequest("Invalid or closed position");

                // Kiểm tra xem sinh viên đã ứng tuyển vị trí này chưa
                var existingApplication = (await _applicationRepository.GetAllAsync())
                    .FirstOrDefault(a => a.StudentId == accountId && a.PositionId == dto.PositionId);
                if (existingApplication != null)
                    return BadRequest("You have already applied for this position.");

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
                return CreatedAtAction(nameof(GetAll), new { id = application.Id }, application);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating application: {ex.Message}");
            }
        }

        // DELETE: api/applications/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "student")]
        public async Task<IActionResult> Delete(int id)
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized("AccountId not found in token.");
            if (!int.TryParse(accountIdValue, out int accountId))
                return BadRequest("Invalid AccountId format.");

            try
            {
                var application = await _applicationRepository.GetByIdAsync(id);
                if (application == null) return NotFound("Application not found");

                // Kiểm tra xem ứng tuyển có thuộc về sinh viên này không
                if (application.StudentId != accountId) return Forbid("You can only cancel your own applications.");

                // Chỉ cho phép hủy khi trạng thái là "pending"
                if (application.Status != "pending")
                    return BadRequest("You can only cancel applications that are still pending.");

                await _applicationRepository.DeleteAsync(id);
                return Ok(new { Message = "Application cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error cancelling application: {ex.Message}");
            }
        }

        // GET: api/applications/interviews
        [HttpGet("interviews")]
        [Authorize(Roles = "company")]
        public async Task<IActionResult> GetInterviews()
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized("AccountId not found in token.");
            if (!int.TryParse(accountIdValue, out int accountId))
                return BadRequest("Invalid AccountId format.");

            try
            {
                var company = (await _companyRepository.GetAllAsync())
                    .FirstOrDefault(c => c.AccountId == accountId);
                if (company == null) return NotFound("Company not found");

                var interviews = (await _applicationRepository.GetAllAsync())
                    .Where(a => a.CompanyId == company.Id && (a.Status == "interviewing" || a.Status == "accepted"))
                    .Select(a => new
                    {
                        a.Id,
                        a.StudentId,
                        a.PositionId,
                        a.InterviewDate,
                        a.InterviewTime,
                        a.InterviewLocation,
                        a.Status
                    });

                return Ok(interviews);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching interviews: {ex.Message}");
            }
        }

        // PUT: api/applications/{id}/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "company")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusModel model)
        {
            var accountIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(accountIdValue)) return Unauthorized("AccountId not found in token.");
            if (!int.TryParse(accountIdValue, out int accountId))
                return BadRequest("Invalid AccountId format.");

            try
            {
                var application = await _applicationRepository.GetByIdAsync(id);
                if (application == null) return NotFound("Application not found");

                var company = (await _companyRepository.GetAllAsync())
                    .FirstOrDefault(c => c.AccountId == accountId);
                if (company == null || application.CompanyId != company.Id)
                    return Forbid("You can only update applications for your company.");

                // Kiểm tra trạng thái hợp lệ
                var validStatuses = new[] { "pending", "interviewing", "accepted", "rejected" };
                if (!validStatuses.Contains(model.Status.ToLower()))
                    return BadRequest("Invalid status. Valid statuses are: pending, interviewing, accepted, rejected.");

                // Cập nhật trạng thái
                application.Status = model.Status;

                if (model.Status.ToLower() == "interviewing")
                {
                    if (model.InterviewDate == null || string.IsNullOrEmpty(model.InterviewTime) || string.IsNullOrEmpty(model.InterviewLocation))
                        return BadRequest("Interview details (date, time, location) are required for 'interviewing' status.");
                    application.InterviewDate = model.InterviewDate;
                    application.InterviewTime = model.InterviewTime;
                    application.InterviewLocation = model.InterviewLocation;
                }
                else if (model.Status.ToLower() == "accepted")
                {
                    // Giữ nguyên thông tin phỏng vấn
                    if (application.InterviewDate == null || string.IsNullOrEmpty(application.InterviewTime) || string.IsNullOrEmpty(application.InterviewLocation))
                        return BadRequest("Interview details must be set before accepting an application.");
                }
                else if (model.Status.ToLower() == "rejected")
                {
                    // Xóa thông tin phỏng vấn khi từ chối
                    application.InterviewDate = null;
                    application.InterviewTime = string.Empty;
                    application.InterviewLocation = string.Empty;
                }

                await _applicationRepository.UpdateAsync(application);
                return Ok(new { Message = "Application status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating application status: {ex.Message}");
            }
        }

        // GET: api/applications/internship-status
        [HttpGet("internship-status")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetInternshipStatus()
        {
            try
            {
                var applications = await _applicationRepository.GetAllAsync();
                var companies = await _companyRepository.GetAllAsync();
                var positions = await _positionRepository.GetAllAsync();
                var students = await _studentRepository.GetAllAsync();

                var internshipStatus = applications
                    .Where(a => a.Status.ToLower() == "accepted")
                    .Select(a => new
                    {
                        a.Id,
                        a.StudentId,
                        a.CompanyId,
                        a.PositionId,
                        a.Status,
                        a.InterviewDate,
                        a.InterviewTime,
                        a.InterviewLocation,
                        Student = students.FirstOrDefault(s => s.Id == a.StudentId),
                        Company = companies.FirstOrDefault(c => c.Id == a.CompanyId),
                        Position = positions.FirstOrDefault(p => p.Id == a.PositionId)
                    });

                return Ok(internshipStatus);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching internship status: {ex.Message}");
            }
        }
    }

    public class CreateApplicationDto
    {
        public int PositionId { get; set; }
    }

    public class UpdateStatusModel
    {
        public string Status { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string InterviewTime { get; set; }
        public string InterviewLocation { get; set; }
    }
}