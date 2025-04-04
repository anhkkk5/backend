using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using InternshipManagement.Data;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách tài khoản
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var accounts = await _context.Accounts
            .Select(a => new
            {
                a.Id,
                a.Username,
                a.Role
            })
            .ToListAsync();
        return Ok(accounts);
    }

    // Cập nhật thông tin tài khoản
    [HttpPut("accounts/{id}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto dto)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound("Account not found");
        }

        account.Username = dto.Username ?? account.Username;
        if (!string.IsNullOrEmpty(dto.Password))
        {
            account.Password = dto.Password; // Nên mã hóa mật khẩu trong thực tế
        }
        account.Role = dto.Role ?? account.Role;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Account updated successfully" });
    }

    // Xóa tài khoản
    [HttpDelete("accounts/{id}")]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var account = await _context.Accounts.FindAsync(id);
        if (account == null)
        {
            return NotFound("Account not found");
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Account deleted successfully" });
    }

    // Lấy danh sách công ty
    [HttpGet("companies")]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _context.Companies
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.Address,
                c.Contact,
                c.Description,
                c.AccountId
            })
            .ToListAsync();
        return Ok(companies);
    }

    // Xóa công ty
    [HttpDelete("companies/{id}")]
    public async Task<IActionResult> DeleteCompany(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        _context.Companies.Remove(company);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Company deleted successfully" });
    }

    // Lấy danh sách sinh viên
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students
            .Select(s => new
            {
                s.Id,
                s.AccountId,
                s.Phone,
                s.Address,
                s.Skills,
                s.CvUrl
            })
            .ToListAsync();
        return Ok(students);
    }

    // Xóa sinh viên
    [HttpDelete("students/{id}")]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        var student = await _context.Students.FindAsync(id);
        if (student == null)
        {
            return NotFound("Student not found");
        }

        _context.Students.Remove(student);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Student deleted successfully" });
    }
}

public class UpdateAccountDto
{
    public string Username { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}