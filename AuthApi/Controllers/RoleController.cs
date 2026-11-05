using AuthApi.DTOs;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly AuthuserContext _context;

        public RoleController(AuthuserContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = roleDto.Name
            };

            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Szerepkör létrehozva", result = role });
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles
                .Select(role => new
                {
                    role.Id,
                    role.Name,
                    role.RegTime
                })
                .ToListAsync();

            return Ok(new { message = "Sikeres lekérdezés", result = roles });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRole(Guid id)
        {
            var role = await _context.Roles
                .Where(r => r.Id == id)
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.RegTime
                })
                .SingleOrDefaultAsync();

            if (role is null)
            {
                return NotFound(new { message = "A szerepkör nem található" });
            }

            return Ok(new { message = "Sikeres lekérdezés", result = role });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] RoleDto roleDto)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role is null)
            {
                return NotFound(new { message = "A szerepkör nem található" });
            }

            role.Name = roleDto.Name;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Szerepkör sikeresen módosítva", result = role });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role is null)
            {
                return NotFound(new { message = "A szerepkör nem található" });
            }

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Szerepkör sikeresen törölve", result = role });
        }
    }
}
