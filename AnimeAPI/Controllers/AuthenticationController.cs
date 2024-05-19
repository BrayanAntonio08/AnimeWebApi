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
