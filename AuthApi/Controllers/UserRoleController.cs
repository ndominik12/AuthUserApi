using AuthApi.Models;
using AuthApi.Models.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("userrole")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly AuthuserContext _context;

        public UserRoleController(AuthuserContext context)
        {
            _context = context;
        }

        [HttpPost("assingRole")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto assignRoleDto)
        {
            try
            {
                var user = await _context.Users
                               .Include(u => u.Roles)
                               .SingleOrDefaultAsync(u => u.Id == assignRoleDto.UserId);

                var role = await _context.Roles.FindAsync(assignRoleDto.RoleId);

                if (user is null || role is null)
                {
                    return StatusCode(404, new { message = "Hibás user vagy role ID" });
                }

                var isValid = user.Roles.Any(r => r.Id == assignRoleDto.RoleId);

                if (isValid == false)
                {
                    user.Roles.Add(role);
                    await _context.SaveChangesAsync(); //Bekerül a kapcsolo táblába a 2 Guid

                    return Ok(new { message = "Sikeres összerendelés" });
                }

                return Conflict(new { message = "Ilyen szerepköre már van a USER-nek" });
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpDelete("deleteRole")]
        public async Task<IActionResult> DeleteRole([FromBody] DeleteRoleDto deleteRoleDto)
        {
            try
            {
                var user = await _context.Users
                               .Include(u => u.Roles)
                               .SingleOrDefaultAsync(u => u.Id == deleteRoleDto.UserId);

                var role = await _context.Roles.FindAsync(deleteRoleDto.RoleId);

                if (user is null || role is null)
                {
                    return StatusCode(404, new { message = "Hibás user vagy role ID" });
                }

                var isValid = user.Roles.Any(r => r.Id == deleteRoleDto.RoleId);

                if (isValid)
                {
                    user.Roles.Remove(role);
                    await _context.SaveChangesAsync(); //Bekerül a kapcsolo táblába a 2 Guid

                    return Ok(new { message = "Sikeres törlés" });
                }

                return Conflict(new { message = "Ilyen szerepköre nincs a USER-nek" });
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        //Az összes user összes szerpköre
        [HttpGet("allUsersAllRoles")]
        public async Task<IActionResult> AllUsersAllRoles()
        {
            try
            {
                var users = await _context.Users.Include(u => u.Roles)
                    .Select(u => 
                    new 
                    {
                        Felhasználó = u,
                        Szerepkörök = u.Roles
                    }).ToListAsync();
                    

                if (users is null)
                {
                    return NotFound(new {message = "Nincs találat"});
                }

                return Ok(users);
            }
            catch (Exception exception)
            {
                return BadRequest(new {message = exception.Message});
            }
        }

        [HttpGet("userAndRoles")]
        public async Task<IActionResult> UserAndRoles([FromQuery] Guid id)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Roles)
                           .Select(u =>new
                           {
                               Id = u.Id,
                               Roles = u.Roles.ToList()
                           })
                           .FirstOrDefaultAsync(u => u.Id == id);

                if (user is null)
                {
                    return NotFound(new { message = "Nincs találat" });
                }

                return Ok(user);
            }
            catch (Exception exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
