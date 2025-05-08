using EPR.Calculator.API.Data;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.FSS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController(ApplicationDBContext context) : ControllerBase
    {
        private ApplicationDBContext _context => context;

        [HttpGet]
        public IActionResult Get()
        {            
            var somedata = _context.FinancialYears.ToList();

            return Ok(somedata);
        }
    }
}
