using AuthApi.DTOs;
using AuthApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace AuthApi.Controllers
{
    [Route("users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AuthuserContext _context;

        public UserController(AuthuserContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> NewUser([FromBody] NewUserDto newUserDto)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = newUserDto.Name,
                Email = newUserDto.Email,
                Password = newUserDto.Password
            };

            if (user is null)
            {
                return StatusCode(400, new { message = "Hiba" });
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new {message = "Felvéve" , result = user});
        }
        [HttpGet("allUser")]
        public async Task<IActionResult> AllUsers()
        {
            var users = await _context.Users.ToListAsync();
            if (users is null)
            {
                return StatusCode(400, new { message = "Hiba" });
            }

            return StatusCode(200, new { message = "Sikeres lekérdezés", result = users });
        }

        [HttpGet("byId")]
        public async Task<IActionResult> UserById([FromQuery] Guid id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return StatusCode(404, new { message = "A felhasználó nem található" });
            }

            return StatusCode(200, new { message = "Sikeres lekérdezés", result = user });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromQuery] Guid id, [FromBody] NewUserDto newUserDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return StatusCode(404, new { message = "A felhasználó nem található" });
            }

            user.Name = newUserDto.Name;
            user.Email = newUserDto.Email;
            user.Password = newUserDto.Password;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return StatusCode(200, new { message = "Sikeresen módosítva", result = user });
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return StatusCode(404, new { message = "Nincs találat" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return StatusCode(200, new { message = "Frissítve", result = user });
        }
    }
}