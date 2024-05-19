using AnimeAPI.Addapters;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using AnimeAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimeAPI.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private IUserRepository _userRepository;
        private IRoleRepository _roleRepository;
        private readonly JwtService _jwtService;

        public AuthenticationController(IUserRepository repo,IRoleRepository roleRepo, JwtService service) 
        { 
            _userRepository = repo; 
            _roleRepository = roleRepo;
            _jwtService = service;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="userDTO">The user data transfer object containing user details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if the userDTO is null or if an invalid admin code is provided for admin registration.
        /// - <see cref="OkObjectResult"/> with the created <see cref="UserDTO"/> object if the user is successfully registered.
        /// - <see cref="BadRequestObjectResult"/> with the exception message if an exception occurs during user creation.
        /// </returns>
        /// <response code="400">If the userDTO is null or if an invalid admin code is provided for admin registration.</response>
        /// <response code="200">If the user is successfully registered.</response>
        /// <response code="400">If an exception occurs during user creation.</response>
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> CreateUser([FromBody]UserDTO userDTO)
        {
            if (userDTO == null)
            {
                return BadRequest(new { message = "You need to insert a valid object for this request" });
            }

            // in this case I made that admin key would be '@admin123', other than that wont let the person register as admin user
            if(
                userDTO.AdminCode != null && 
                !userDTO.AdminCode.Equals("@admin123") && 
                await _roleRepository.IsAdminRole(userDTO.RoleId)
                )
            {
                return BadRequest(new { message = "No valid key for admin register" });
            }
            
            try
            {
                User result = await _userRepository.AddAsync(UserDTO.Map(userDTO));
                return Ok(UserDTO.Map(result));
            }catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        /// <summary>
        /// Authenticates a user and generates a JWT token.
        /// </summary>
        /// <param name="dto">The user data transfer object containing login details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if the dto is null.
        /// - <see cref="OkObjectResult"/> with a success message and the JWT token if login is successful.
        /// - <see cref="OkObjectResult"/> with a failure message indicating incorrect credentials if login fails.
        /// - <see cref="BadRequestObjectResult"/> with the exception message if an exception occurs during login.
        /// </returns>
        /// <response code="400">If the dto is null.</response>
        /// <response code="200">If login is successful, with a success message and the JWT token.</response>
        /// <response code="200">If login fails, with a failure message indicating incorrect credentials.</response>
        /// <response code="400">If an exception occurs during login.</response>
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserDTO dto)
        {
            if (dto == null)
            {
                return BadRequest(new { message = "You need to insert a valid object for this request" });
            }
            try
            {
                User? data = await _userRepository.Login(UserDTO.Map(dto));
                if (data != null)
                {
                    // here we have to generate a toke to save on session
                    string token = _jwtService.GenerateToken(data);
                    return Ok(new {success = true, message="Login successfull", token = token});
                }
                else
                {
                    return Ok(new { success = false, message = "Login failed, check your credentials" });
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Checks if the authenticated user has an admin role.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if the role claim is not provided in authentication.
        /// - <see cref="OkObjectResult"/> with a boolean value indicating whether the user has an admin role.
        /// </returns>
        /// <response code="400">If the role claim is not provided in authentication.</response>
        /// <response code="200">If the check is successful, with a boolean value indicating the admin role status.</response>
        [HttpGet("isAdmin")]
        [Authorize]
        public async Task<IActionResult> IsAdminRole()
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest(new { message = "Needed atributes are not provided in authentication" });
            int role_id = Convert.ToInt32(roleClaim);

            return Ok(await _roleRepository.IsAdminRole(role_id));
        }
    }
}
