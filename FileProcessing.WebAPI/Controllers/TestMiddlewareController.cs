using System.Diagnostics.Eventing.Reader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessing.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestMiddlewareController : ControllerBase
    {
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            if (Request.Query["token"].ToString() == "")
                return StatusCode(403);
            else
                return StatusCode(200);


        }

        [HttpGet("notoken")]
        public async Task<IActionResult> GetNoToken()
        {
            return StatusCode(200);
        }
    }
}
