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
    }
}
