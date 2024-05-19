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
        #region variable declarations - constructor
        private IAnimeRepository _animeRepository;
        private IRoleRepository _roleRepository;


        public AnimeController(IAnimeRepository repo, IRoleRepository roleRepository)
        {
            // all interface implementation are made by dependency injection
            _animeRepository = repo;
            _roleRepository = roleRepository;
        }
        #endregion

        #region HTTTP-GET

        /// <summary>
        /// An endpoint that returns all the animes stored.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="OkObjectResult"/> with a list of <see cref="AnimeDTO"/> representing all the animes.
        /// </returns>
        /// <response code="200">Returns a list of all animes stored.</response>
        [HttpGet]
        public async Task<IActionResult> GetAnimeList()
        {
            return Ok(AnimeDTO.Map(await _animeRepository.GetAllAsync()));
        }

        /// <summary>
        /// Allows to look for a specific anime by its id.
        /// </summary>
        /// <param name="id">The id of the anime to search for.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="NotFoundObjectResult"/> if no anime is found with the provided id.
        /// - <see cref="OkObjectResult"/> with an <see cref="AnimeDTO"/> object if the anime is found.
        /// </returns>
        /// <response code="200">Returns the anime with the given id.</response>
        /// <response code="404">Returns an error message if no anime is found with the given id.</response>
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

        /// <summary>
        /// Searches for animes by name and returns a list of matching animes.
        /// </summary>
        /// <param name="name">The name of the anime to search for.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="OkObjectResult"/> with a list of <see cref="AnimeDTO"/> if matching animes are found.
        /// - <see cref="NotFoundObjectResult"/> if no matching animes are found.
        /// </returns>
        /// <response code="200">Returns a list of animes that match the given name.</response>
        /// <response code="404">Returns an error message if no animes are found with the given name.</response>
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

        /// <summary>
        /// Retrieves the list of favourite animes for the authenticated user.
        /// </summary>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="OkObjectResult"/> with a list of <see cref="AnimeDTO"/> if the request is successful.
        /// - <see cref="BadRequestObjectResult"/> if the required authentication attributes are not provided or the user does not have the necessary permissions.
        /// </returns>
        /// <response code="200">Returns a list of favourite animes for the authenticated user.</response>
        /// <response code="400">Returns an error message if the necessary authentication attributes are not provided or the user does not have permission to access the resource.</response>
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

        /// <summary>
        /// Checks if a specific anime is marked as favourite by the authenticated user.
        /// </summary>
        /// <param name="animeId">The ID of the anime to check.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if necessary authentication attributes are missing or if the user is not allowed to perform this action.
        /// - <see cref="OkObjectResult"/> with a boolean indicating whether the anime is a favourite.
        /// </returns>
        /// <response code="200">Returns a boolean indicating if the anime is a favourite.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing or if the user is not permitted to perform this action.</response>
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

        #endregion

        #region #HTTTP-POST

        /// <summary>
        /// Adds an anime to the authenticated user's favourites list.
        /// </summary>
        /// <param name="animeId">The ID of the anime to add to favourites.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing or if the user is not permitted to perform this action.
        /// - <see cref="OkObjectResult"/> with a boolean indicating whether the anime was successfully added to favourites.
        /// </returns>
        /// <response code="200">Returns a boolean indicating if the anime was successfully added to favourites.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing or if the user is not permitted to perform this action.</response>
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

        /// <summary>
        /// Creates a new anime entry in the database.
        /// </summary>
        /// <param name="anime">The anime data transfer object containing the anime details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing, if the user is not permitted to perform this action, or if an error occurs during the creation process.
        /// - <see cref="OkObjectResult"/> with the created anime's data transfer object.
        /// </returns>
        /// <response code="200">Returns the created anime's data transfer object.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing, if the user is not permitted to perform this action, or if an error occurs during the creation process.</response>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAnime([FromBody] AnimeDTO anime)
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
                return BadRequest(ex.Message); ;
            }
        }

        /// <summary>
        /// Creates multiple new anime entries in the database.
        /// </summary>
        /// <param name="animes">The collection of anime data transfer objects containing the anime details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing, if the user is not permitted to perform this action, if the given object is not valid, or if the array has no elements.
        /// - <see cref="OkObjectResult"/> with the created animes' data transfer objects.
        /// </returns>
        /// <response code="200">Returns the created animes' data transfer objects.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing, if the user is not permitted to perform this action, if the given object is not valid, or if the array has no elements.</response>
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
            foreach (Anime anime in values)
            {
                anime.Id = 0;
                await _animeRepository.AddAsync(anime);

            }

            return Ok(AnimeDTO.Map(values));
        }

        #endregion

        #region HTTP-PUT

        /// <summary>
        /// Updates an existing anime entry in the database.
        /// </summary>
        /// <param name="anime">The anime data transfer object containing the updated anime details.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing, if the user is not permitted to perform this action, or if an error occurs during the update process.
        /// - <see cref="OkObjectResult"/> with the updated anime's data transfer object.
        /// </returns>
        /// <response code="200">Returns the updated anime's data transfer object.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing, if the user is not permitted to perform this action, or if an error occurs during the update process.</response>
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

        #endregion

        #region HTTP-DELETE

        /// <summary>
        /// Removes an anime from the user's list of favourite animes.
        /// </summary>
        /// <param name="animeId">The ID of the anime to be removed from favourites.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing or if the user is not permitted to perform this action.
        /// - <see cref="OkObjectResult"/> with a boolean indicating the success of the removal.
        /// </returns>
        /// <response code="200">Returns a boolean indicating whether the removal was successful.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing or if the user is not permitted to perform this action.</response>
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


        /// <summary>
        /// Deletes an anime entry from the database.
        /// </summary>
        /// <param name="animeId">The ID of the anime to be deleted.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - <see cref="BadRequestObjectResult"/> if required authentication attributes are missing or if the user is not permitted to perform this action.
        /// - <see cref="OkObjectResult"/> with a boolean indicating the success of the deletion.
        /// </returns>
        /// <response code="200">Returns a boolean indicating whether the deletion was successful.</response>
        /// <response code="400">Returns an error message if required authentication attributes are missing or if the user is not permitted to perform this action.</response>
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
        #endregion

    }
}
