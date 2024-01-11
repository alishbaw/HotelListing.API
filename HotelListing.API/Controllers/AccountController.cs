using HotelListing.API.Contracts;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _authManager;

        public AccountController(IAuthManager authManager)
        {
            this._authManager = authManager;
        }

        //POST: api/Account/register
        [HttpPost] //set request type as a post request
        [Route("register")] //set the route to this endpoint
        [ProducesResponseType(StatusCodes.Status400BadRequest)] //setting response types. if validation fails it produces a bad request response
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult> Register([FromBody] ApiUserDTO apiUserDTO) //letting method know the dto should only be expected from the body, not other parameters
        {
            var errors = await _authManager.Register(apiUserDTO);

            if (errors.Any())
            {
                foreach (var error in errors) //if any errors, iterate through them and add to the model state which is what holds errors and displays when a bad request comes in 
                {
                    ModelState.AddModelError(error.Code, error.Description); //identity error object comes with a code and an error message/description
                }
                return BadRequest(ModelState);
            }

            return Ok();
        }

        //POST: api/Account/login
        [HttpPost] 
        [Route("login")] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult> Login([FromBody] LoginDTO loginDTO) 
        {
            var authResponse = await _authManager.Login(loginDTO);

            if (authResponse == null)
            {
                return Unauthorized(); //returns unauthorized status 401 response which means user is unauthenticated
            }

            return Ok(authResponse);
        }
    }
}
