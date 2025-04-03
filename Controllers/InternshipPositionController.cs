using InternshipManagement.Models;
using InternshipManagement.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InternshipManagement.Controllers
{
    [Route("api/internship-positions")]
    [ApiController]
    public class InternshipPositionController : ControllerBase
    {
        private readonly IRepository<InternshipPosition> _positionRepository;
        private readonly IRepository<Company> _companyRepository;

        public InternshipPositionController(
            IRepository<InternshipPosition> positionRepository,
            IRepository<Company> companyRepository)
        {
            _positionRepository = positionRepository;
            _companyRepository = companyRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var positions = await _positionRepository.GetAllAsync();
            return Ok(positions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return NotFound();
            return Ok(position);
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Create([FromBody] InternshipPosition position)
        {
            if (position == null || position.CompanyId <= 0)
                return BadRequest("Invalid position data or CompanyId");

            // Thêm repository cho Companies
            var companyRepository = HttpContext.RequestServices.GetService<IRepository<Company>>();
            var company = await companyRepository.GetByIdAsync(position.CompanyId);
            if (company == null)
            {
                Console.WriteLine($"Company with ID {position.CompanyId} not found in database.");
                return BadRequest("Company not found");
            }

            await _positionRepository.CreateAsync(position);
            return CreatedAtAction(nameof(GetById), new { id = position.Id }, position);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, [FromBody] InternshipPosition position)
        {
            var existingPosition = await _positionRepository.GetByIdAsync(id);
            if (existingPosition == null) return NotFound();

            var companyRepository = HttpContext.RequestServices.GetService<IRepository<Company>>();
            if (companyRepository == null)
                return StatusCode(500, "Internal server error: Company repository not available");

            var company = await companyRepository.GetByIdAsync(position.CompanyId);
            if (company == null)
            {
                Console.WriteLine($"Company with ID {position.CompanyId} not found in database.");
                return BadRequest("Company not found");
            }

            // Cập nhật dữ liệu từ position vào existingPosition
            existingPosition.CompanyId = position.CompanyId;
            existingPosition.Title = position.Title;
            existingPosition.Description = position.Description;
            existingPosition.Slots = position.Slots;
            existingPosition.Status = position.Status;

            await _positionRepository.UpdateAsync(existingPosition); // Cập nhật entity đã theo dõi
            return Ok(new { Message = "Position updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            Console.WriteLine($"User roles: {string.Join(", ", User.FindAll(ClaimTypes.Role).Select(r => r.Value))}");
            var position = await _positionRepository.GetByIdAsync(id);
            if (position == null) return NotFound();

            await _positionRepository.DeleteAsync(id);
            return Ok(new { Message = "Position deleted successfully" });
        }
    }
}