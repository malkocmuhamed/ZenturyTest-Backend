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
public class UserLoginsController : ControllerBase
{
    private readonly UsersDBContext _context;

    public UserLoginsController(UsersDBContext context)
    {
        _context = context;
    }

    // GET: api/UserLogins
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserLogin>>> GetAllUserLogins()
    {
        return await _context.UserLogins.ToListAsync();
    }

    // GET: api/UserLogins paging
    [HttpGet("paging")]
    public async Task<ActionResult<PaginatedResponse<UserLogin>>> GetUserLogins([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var totalCount = await _context.UserLogins.CountAsync();
        var userLogins = await _context.UserLogins
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var paginatedResponse = new PaginatedResponse<UserLogin>
        {
            Data = userLogins,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return paginatedResponse;
    }

    // GET: api/UserLogins/5
    [HttpGet("{id}")]
    public async Task<ActionResult<UserLogin>> GetUserLogin(int id)
    {
        var userLogin = await _context.UserLogins.FindAsync(id);

        if (userLogin == null)
        {
            return NotFound();
        }

        return userLogin;
    }

    // DELETE: api/UserLogins/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUserLogin(int id)
    {
        var userLogin = await _context.UserLogins.FindAsync(id);
        if (userLogin == null)
        {
            return NotFound();
        }

        _context.UserLogins.Remove(userLogin);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UserLoginExists(int id)
    {
        return _context.UserLogins.Any(e => e.Id == id);
    }
}
