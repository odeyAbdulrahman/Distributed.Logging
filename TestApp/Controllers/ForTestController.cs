using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForTestController : ControllerBase
    {
        [HttpPost]
        public ActionResult Post([FromBody] int id)
        {
            Error();
            return Ok(id);
        }

        private void Error()
        {
            throw new ArgumentNullException();

        }
    }
}
