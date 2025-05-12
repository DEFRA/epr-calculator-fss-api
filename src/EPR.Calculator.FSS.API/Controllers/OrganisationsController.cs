namespace EPR.Calculator.FSS.API.Controllers
{
    using BackendAccountService.Core.Services;
    using EPR.Calculator.FSS.API.Common.Models;
    using EPR.Calculator.FSS.API.Common.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics.CodeAnalysis;

    [ExcludeFromCodeCoverage]
    [ApiController]
    [Route("api/organisations")]
    public class OrganisationsController
    {
        private readonly IOrganisationService organisationService;
        private readonly ILogger<OrganisationsController> _logger;
        private readonly IUserService _userService;
        private User _user;
        private OrganisationResponseModel _organisation;

        public OrganisationsController(IOrganisationService organisationService, IUserService userService, ILogger<OrganisationsController> logger)
        {
            this.organisationService = organisationService;
            _logger = logger;
            _userService = userService;
        }

        [HttpGet]
        [Route("organisationDetails")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrganisationsDetails(string createdOrModifiedAfter)
        {
            var organisationList = await this.organisationService.GetOrganisationsDetails(createdOrModifiedAfter);
            if (organisationList.Count > 0)
            {
                return Ok(organisationList);
            }
            else
            {
                return NoContent();
            }
        }
    }

}