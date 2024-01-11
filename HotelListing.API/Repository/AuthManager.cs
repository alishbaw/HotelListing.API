using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._configuration = configuration;
        }

        public async Task<AuthResponseDTO> Login(LoginDTO loginDTO)
        {
                var user = await _userManager.FindByEmailAsync(loginDTO.Email); //validate email address. look for user by email 
                bool isValidUser = await _userManager.CheckPasswordAsync(user, loginDTO.Password); //validate password. check password belongs to user

            if (user == null || isValidUser == false)
            {
                return null; 
            }

            var token = await GenerateToken(user);

            return new AuthResponseDTO
            {
                Token = token,
                UserId = user.Id
            };

        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDTO)
        {
            var user = _mapper.Map<ApiUser>(userDTO); //map the object of type ApiUserDTO to type ApiUser
            user.UserName = userDTO.Email; //the username for user is going to be whatever email came through the userDTO object

            var result = await _userManager.CreateAsync(user, userDTO.Password); //create user with the password from the DTO

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User"); //if successful, add user to User role
            }

            return result.Errors;
        }

        private async Task<string> GenerateToken(ApiUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //generate information about the user 

            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList(); //generating a list of claims for the user where it states the type of claims and adds the value of x. x reps each string in list of string (roles)
            var userClaims = await _userManager.GetClaimsAsync(user); //adding claims to user after registration

            var claims = new List<Claim>  //generate actual list of claims for our token 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email), //add default claim where sub is user's email/username. sub = subject/person to whom the token has been issued
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //generate random guid so jti value different with each token to prevent playback attacks 
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id), //custom claim 
            }
            .Union(userClaims).Union(roleClaims); //we now have one big list of claims with whatever came back from database and whatever came back from roles 

            //create token with the parameters we need to validate in program.cs
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token); //returns string which represents our token 
        }
    }
}
