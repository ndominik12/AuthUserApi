using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("userroles")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly AuthuserContext _context;

        public UserRoleController(AuthuserContext context)
        {
            _context = context;
        }

        [HttpGet("{userId:guid}")]
        public async Task<IActionResult> GetUserRoles(Guid userId)
        {
            var userExists = await _context.Users.AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                return NotFound(new { message = "A felhasználó nem található" });
            }

            var roles = await _context.Users
                .Where(user => user.Id == userId)
                .SelectMany(user => user.Roles)
                .Select(role => new
                {
                    role.Id,
                    role.Name,
                    role.RegTime
                })
                .ToListAsync();

            return Ok(new { message = "Sikeres lekérdezés", result = roles });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole(
            [FromQuery] Guid userId,
            [FromQuery] Guid roleId)
        {
            var user = await _context.Users
                .Include(currentUser => currentUser.Roles)
                .SingleOrDefaultAsync(currentUser => currentUser.Id == userId);

            if (user is null)
            {
                return NotFound(new { message = "A felhasználó nem található" });
            }

            var role = await _context.Roles.FindAsync(roleId);

            if (role is null)
            {
                return NotFound(new { message = "A szerepkör nem található" });
            }

            if (user.Roles.Any(existingRole => existingRole.Id == roleId))
            {
                return Conflict(new { message = "A szerepkör már hozzá van rendelve a felhasználóhoz" });
            }

            user.Roles.Add(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Szerepkör sikeresen hozzárendelve", result = new { userId, roleId } });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveRole(
            [FromQuery] Guid userId,
            [FromQuery] Guid roleId)
        {
            var user = await _context.Users
                .Include(currentUser => currentUser.Roles)
                .SingleOrDefaultAsync(currentUser => currentUser.Id == userId);

            if (user is null)
            {
                return NotFound(new { message = "A felhasználó nem található" });
            }

            var role = user.Roles.SingleOrDefault(existingRole => existingRole.Id == roleId);

            if (role is null)
            {
                return NotFound(new { message = "A szerepkör nincs hozzárendelve a felhasználóhoz" });
            }

            user.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Szerepkör sikeresen levéve", result = new { userId, roleId } });
        }
    }
}
