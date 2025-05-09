using EPR.Calculator.API.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.Controllers
{
    [ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    [ApiController]
    public class ExampleController(ApplicationDBContext context) : ControllerBase
    {
        private ApplicationDBContext Context => context;

        [HttpGet]
        public IActionResult Get()
        {
            var somedata = this.Context.FinancialYears.ToList();

            return this.Ok(somedata);
        }
    }
}