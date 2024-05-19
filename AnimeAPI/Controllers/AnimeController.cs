using AnimeAPI.Addapters;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AnimeAPI.Controllers
{
    [Route("api/anime")]
    [ApiController]
    public class AnimeController : ControllerBase
    {
        private IAnimeRepository _animeRepository;
        private IRoleRepository _roleRepository;

        public AnimeController(IAnimeRepository repo, IRoleRepository roleRepository)
        {
            _animeRepository = repo;
            _roleRepository = roleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnimeList()
        {
            return Ok(AnimeDTO.Map(await _animeRepository.GetAllAsync()));
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAnime(int id)
        {
            Anime? result = await _animeRepository.GetByIdAsync(id);
            if(result == null)
            {
                return NotFound("No item found with this id");
            }
            return Ok(AnimeDTO.Map(result));
        }

        [HttpGet]
        [Route("search/{name}")]
        public async Task<IActionResult> SearchAnime(string name)
        {
            List<AnimeDTO> result = AnimeDTO.Map(await _animeRepository.FindByName(name)).ToList();
            if (result.Count == 0)
            {
                return NotFound("No items found with this name");
            }
            return Ok(result);
        }

        [HttpGet("favourites")]
        [Authorize]
        public async Task<IActionResult> GetFavouritesAnimeList()
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) 
                return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only client users are allowed to save favourites animes
            if (await _roleRepository.IsAdminRole(role_id)) 
                return BadRequest("The given token has no permission to make this request");

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userClaim == null) 
                return BadRequest("Needed atributes are not provided in authentication");
            int userId = Convert.ToInt32(userClaim);

            return Ok(AnimeDTO.Map(await _animeRepository.GetFavourites(userId)));
        }

        [HttpPost("favourite/{animeId}")]
        [Authorize]
        public async Task<IActionResult> AddFavouriteAnime(int animeId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only client users are allowed to save favourites animes
            if (await _roleRepository.IsAdminRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userClaim == null)
                return BadRequest("Needed atributes are not provided in authentication");
            int userId = Convert.ToInt32(userClaim);

            return Ok(await _animeRepository.AddFavourite(animeId, userId));
        }

        [HttpDelete("favourite/{animeId}")]
        [Authorize]
        public async Task<IActionResult> RemoveFavouriteAnime(int animeId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only client users are allowed to save favourites animes
            if (await _roleRepository.IsAdminRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userClaim == null)
                return BadRequest("Needed atributes are not provided in authentication");
            int userId = Convert.ToInt32(userClaim);

            return Ok(await _animeRepository.DeleteFavourite(animeId, userId));
        }

        [HttpGet("isfavourite/{animeId}")]
        [Authorize]
        public async Task<IActionResult> IsFavouriteAnime(int animeId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only client users are allowed to save favourites animes
            if (await _roleRepository.IsAdminRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userClaim == null)
                return BadRequest("Needed atributes are not provided in authentication");
            int userId = Convert.ToInt32(userClaim);

            return Ok(await _animeRepository.IsFavourite(animeId, userId));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAnime([FromBody]AnimeDTO anime)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            if (roleClaim == null || roleClaim.Value == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim.Value);

            // Only admin users are allowed to save favourites animes
            if (await _roleRepository.IsClientRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            anime.Id = 0;
            try
            {
                Anime result = await _animeRepository.AddAsync(AnimeDTO.Map(anime));
                return Ok(AnimeDTO.Map(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);;
            }
        }

        [HttpPost]
        [Route("range")]
        [Authorize]
        public async Task<IActionResult> CreateAnimeRange([FromBody] IEnumerable<AnimeDTO> animes)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only admin users are allowed to save favourites animes
            if (await _roleRepository.IsClientRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            if (animes == null) return BadRequest("Object given is not valid");
            if (animes.Count() == 0) return BadRequest("Array given has no elements");

            IEnumerable<Anime> values = AnimeDTO.Map(animes);
            foreach(Anime anime in values)
            {
                anime.Id = 0;
                await _animeRepository.AddAsync(anime);

            }

            return Ok(AnimeDTO.Map(values));
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateAnime([FromBody] AnimeDTO anime)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only admin users are allowed to save favourites animes
            if (await _roleRepository.IsClientRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            try
            {
                Anime result = await _animeRepository.UpdateAsync(AnimeDTO.Map(anime));
                return Ok(AnimeDTO.Map(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ex); ;
            }
        }

        [HttpDelete("{animeId}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnime(int animeId)
        {
            var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            if (roleClaim == null) return BadRequest("Needed atributes are not provided in authentication");
            int role_id = Convert.ToInt32(roleClaim);

            // Only admin users are allowed to save favourites animes
            if (await _roleRepository.IsClientRole(role_id))
                return BadRequest("The given token has no permission to make this request");

            bool removed = await _animeRepository.RemoveAsync(animeId);
            return Ok (removed);
        }
    }
}
