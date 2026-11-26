using AuthApi.Models;
using AuthApi.Models.DTOs;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers
{
    [Route("users")]
    [ApiController]
    
    public class UsersController : ControllerBase
    {
        private readonly AuthuserContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly GenerateToken _generateToken;

        public UsersController(AuthuserContext context, PasswordHasher passwordHasher, GenerateToken generateToken)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _generateToken = generateToken;
        }

        [HttpPost("newUser")]
        public async Task<IActionResult> NewUser([FromBody]NewUserDto newUserDto )
        {
            var (hash, salt) = _passwordHasher.HashPassword(newUserDto.Password!);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = newUserDto.Name,
                Email = newUserDto.Email,
                Password = hash,
                Salt = salt
            };

            if (user is null)
            {
                return StatusCode(400, new { message = "Hiba"});
            }

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return StatusCode(201, new { message = "Felvéve" , result = user});
        }

        [Authorize(Roles = "student")]
        [HttpGet("allUser")]
        public async Task<IActionResult> AllUser()
        {
            var users = await _context.Users.ToListAsync();

            if (users is null)
            {
                return StatusCode(400, new { message = "Hiba" });
            }

            return StatusCode(200, new { message = "Lekérdezve", result = users });
        }

        [HttpGet("byId")]
        public async Task<IActionResult> UserById([FromQuery]Guid id)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return StatusCode(404, new { message = "Nincs találat" });
            }
            else if (user is not null)
            {
                return StatusCode(200, new { message = "Lekérdezve" , result = user});
            }

            return StatusCode(400, new { message = "Hiba" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateUser([FromQuery] Guid id , [FromBody]UpdateUserDto updateUserDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (user is null)
            {
                return StatusCode(404, new { message = "Nincs találat" });
            }
            else if (user is not null)
            {
                user.Name = updateUserDto.Name;
                user.Email = updateUserDto.Email;
                user.Password = updateUserDto.Password;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return StatusCode(200, new { message = "Frissítve", result = user });
            }

            return StatusCode(400, new { message = "Hiba" });
        }

        [HttpDelete("deleteUser")]
        public async Task<IActionResult> DeleteUser([FromQuery] Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user is null)
            {
                return StatusCode(404, new { message = "Nincs találat" });
            }
            else if (user is not null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return StatusCode(200, new { message = "Törölve", result = user });
            }

            return StatusCode(400, new { message = "Hiba" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Roles)
                    .SingleOrDefaultAsync(u => u.Email == loginUserDto.Email);

                if (user is null)
                {
                    return StatusCode(404, new {message = "Nincs ilyen email"});
                }

                var isValid = _passwordHasher.Verify(loginUserDto.Password!,user.Password!,user.Salt!);

                if (isValid)
                {
                    var newToken = _generateToken.Token(user.Id, user.Email!, user.Roles);

                    return StatusCode(200, new { message = "Sikeres belépés", token = newToken});
                }

                return StatusCode(404, new { message = "Hibás jelszó" });
            }
            catch (Exception exception)
            {
                return StatusCode(400, new { message = exception.Message });
            }
        }
    }
}
