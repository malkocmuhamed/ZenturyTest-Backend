using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Users_app.Context;
using Users_app.Models;

[Route("api/[controller]")]
[ApiController]
//[Authorize]
public class UsersController : ControllerBase
{
    private readonly UsersDBContext _context;

    public UsersController(UsersDBContext context)
    {
        _context = context;
    }

    // GET: api/Users
  
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
    {
        return await _context.Users.ToListAsync();
    }

    [HttpGet("paging")]
    public async Task<ActionResult<PaginatedResponse<User>>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = await _context.Users.CountAsync();
        var users = await _context.Users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paginatedResponse = new PaginatedResponse<User>
        {
            Data = users,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return paginatedResponse;
    }

    // GET: api/Users/5
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        var newUser = new User
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            Email = user.Email,
            PasswordHash = user.PasswordHash // Hash the password here
        };
        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }
    private string HashPassword(string password)
    {
        // Use a strong and secure password hashing algorithm, such as bcrypt
        // You'll need to install a package that supports the chosen hashing algorithm
        // Here's an example using BCrypt.Net library
        // Install-Package BCrypt.Net-Next
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // PUT: api/Users/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.Id)
        {
            return BadRequest();
        }

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Users/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserExists(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
