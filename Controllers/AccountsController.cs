using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManagement.Data; // Thay bằng namespace của bạn
using System.Threading.Tasks;
using System;
using InternshipManagement.Models;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AppDbContext _context; // Thay bằng DbContext của bạn

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/accounts
        [HttpGet]
        [Authorize(Roles = "admin")] // Chỉ admin mới có quyền truy cập
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            try
            {
                var accounts = await _context.Accounts.ToListAsync();
                return Ok(accounts);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching accounts: {ex.Message}");
            }
        }

        // PUT: api/accounts/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")] // Chỉ admin mới có quyền truy cập
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountModel model)
        {
            try
            {
                // Tìm tài khoản theo ID
                var account = await _context.Accounts.FindAsync(id);
                if (account == null)
                {
                    return NotFound($"Account with ID {id} not found");
                }

                // Cập nhật các trường
                if (!string.IsNullOrEmpty(model.Username))
                {
                    account.Username = model.Username;
                }
                if (!string.IsNullOrEmpty(model.Password))
                {
                    // Mã hóa mật khẩu trước khi lưu (nếu cần)
                    // Ví dụ: account.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
                    account.Password = model.Password; // Để plaintext cho demo
                }
                if (!string.IsNullOrEmpty(model.Role))
                {
                    account.Role = model.Role;
                }
                if (!string.IsNullOrEmpty(model.Email))
                {
                    account.Email = model.Email;
                }

                // Lưu thay đổi vào database
                _context.Accounts.Update(account);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"Account with ID {id} updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating account: {ex.Message}");
            }
        }

        // DELETE: api/accounts/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")] // Chỉ admin mới có quyền truy cập
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                // Tìm tài khoản theo ID
                var account = await _context.Accounts.FindAsync(id);
                if (account == null)
                {
                    return NotFound($"Account with ID {id} not found");
                }

                // Xóa tài khoản
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"Account with ID {id} deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting account: {ex.Message}");
            }
        }
    }

    // Model để nhận dữ liệu cập nhật
    public class UpdateAccountModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}