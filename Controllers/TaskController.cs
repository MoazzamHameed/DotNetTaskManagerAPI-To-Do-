using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManagerAPI.DTOs;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Authorize]                 // all actions require token unless AllowAnonymous
    [Route("api/tasks")]
    public class TasksController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Health check (no token)
        [AllowAnonymous]
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { message = "Ping OK" });

        // GET /api/tasks -> 200 [] if none; 401 if no token
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TaskItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTasks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var tasks = await _context.TaskItems
                                      .Where(t => t.AppUserId == userId)
                                      .AsNoTracking()
                                      .ToListAsync();

            return Ok(tasks);
        }

        // GET /api/tasks/{id} -> only your own item
        [HttpGet("{id:int}")]
        //[ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var task = await _context.TaskItems
                                     .AsNoTracking()
                                     .SingleOrDefaultAsync(t => t.Id == id && t.AppUserId == userId);

            if (task == null) return NotFound();
            return Ok(task);
        }

        // POST /api/tasks
        [HttpPost]
        //[Consumes("application/json")]
        //[ProducesResponseType(typeof(TaskItem), StatusCodes.Status201Created)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] TaskDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                DueDate = dto.DueDate,
                AppUserId = userId
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // 201 + Location header
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }

        // PUT /api/tasks/{id}
        [HttpPut("{id:int}")]
        //[Consumes("application/json")]
        //[ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(int id, [FromBody] TaskDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var task = await _context.TaskItems
                                     .SingleOrDefaultAsync(t => t.Id == id && t.AppUserId == userId);
            if (task == null) return NotFound();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.Status = dto.Status;
            task.DueDate = dto.DueDate;

            await _context.SaveChangesAsync();
            return Ok(task);
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id:int}")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(StatusCodes.Status404NotFound)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var task = await _context.TaskItems
                                     .SingleOrDefaultAsync(t => t.Id == id && t.AppUserId == userId);
            if (task == null) return NotFound();

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
