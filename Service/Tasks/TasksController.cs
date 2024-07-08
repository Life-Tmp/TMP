using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace TMP.Service.Tasks
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TasksController : ControllerBase
    {
        
        public TasksController()
        {
           
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetUsers()
        {
            
            return Ok("This is a working endpoint");
        }
    }
}