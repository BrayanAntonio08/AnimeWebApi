using AnimeAPI.Addapters;
using AnimeAPI.Controllers;
using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using AnimeAPI.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Query.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace APITesting
{
    public class AnimeControllerTests
    {

        private AnimeDBContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AnimeDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Usa un nuevo Guid para cada prueba
                .Options;

            var dbContext = new AnimeDBContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task GetAnimeList_ReturnsOkResult_WithListOfAnimes()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            context.Animes.Add(new Anime { Id = 1, EnglishTitle = "Test Anime" });
            context.SaveChanges();

            // Act
            var result = await controller.GetAnimeList();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.IsType<List<AnimeDTO>>(okResult.Value);
        }
        [Fact]
        public async Task GetAnime_ReturnsOkResult_WhenAnimeExists()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var anime = new Anime { Id = 1, EnglishTitle = "Test Anime" };
            context.Animes.Add(anime);
            context.SaveChanges();

            // Act
            var result = await controller.GetAnime(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AnimeDTO>(okResult.Value);
            Assert.Equal(1, returnValue.Id);
        }

        [Fact]
        public async Task GetAnime_ReturnsNotFound_WhenAnimeDoesNotExist()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            // Act
            var result = await controller.GetAnime(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No item found with this id", notFoundResult.Value);
        }

        [Fact]
        public async Task SearchAnime_ReturnsOkResult_WithMatchingAnimes()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var anime = new Anime { Id = 1, EnglishTitle = "Test Anime" };
            context.Animes.Add(anime);
            context.SaveChanges();

            // Act
            var result = await controller.SearchAnime("Test");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<List<AnimeDTO>>(okResult.Value);
            Assert.Single(returnValue);
        }

        [Fact]
        public async Task SearchAnime_ReturnsNotFound_WhenNoMatchingAnimes()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            // Act
            var result = await controller.SearchAnime("NonExistent");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No items found with this name", notFoundResult.Value);
        }

        [Fact]
        public async Task GetFavouritesAnimeList_ReturnsBadRequest_WhenRoleClaimIsNull()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.GetFavouritesAnimeList();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Needed atributes are not provided in authentication", badRequestResult.Value);
        }

        [Fact]
        public async Task GetFavouritesAnimeList_ReturnsBadRequest_WhenRoleIsNotClient()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "1")
            }));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            context.Roles.Add(new Role { RoleId = 1, Name = "Admin" });
            context.SaveChanges();

            // Act
            var result = await controller.GetFavouritesAnimeList();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The given token has no permission to make this request", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateAnime_ReturnsOkResult_WhenAnimeIsCreated()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 1, Name = "Admin" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var animeDto = new AnimeDTO { English_title = "New Anime" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.CreateAnime(animeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AnimeDTO>(okResult.Value);
            Assert.NotEqual(0, returnValue.Id);
        }

        [Fact]
        public async Task CreateAnime_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 2, Name = "Client" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var animeDto = new AnimeDTO { English_title = "New Anime" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.CreateAnime(animeDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The given token has no permission to make this request", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAnime_ReturnsOkResult_WhenAnimeIsUpdated()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 1, Name = "Admin" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var anime = new Anime { Id = 1, EnglishTitle = "Original Anime" };
            context.Animes.Add(anime);
            context.Roles.Add(role);
            context.SaveChanges();

            // Detach the entity
            context.Entry(anime).State = EntityState.Detached;

            var animeDto = new AnimeDTO { Id = 1, English_title = "Updated Anime" };

            // Act
            var result = await controller.UpdateAnime(animeDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<AnimeDTO>(okResult.Value);
            Assert.Equal("Updated Anime", returnValue.English_title);
        }

        [Fact]
        public async Task UpdateAnime_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 2, Name = "Client" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var anime = new Anime { Id = 1, EnglishTitle = "Original Anime" };
            context.Animes.Add(anime);
            context.Roles.Add(role);
            context.SaveChanges();

            // Detach the entity
            context.Entry(anime).State = EntityState.Detached;

            var animeDto = new AnimeDTO { Id = 1, English_title = "Updated Anime" };

            // Act
            var result = await controller.UpdateAnime(animeDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The given token has no permission to make this request", badRequestResult.Value);
        }


        [Fact]
        public async Task DeleteAnime_ReturnsOkResult_WhenAnimeIsDeleted()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 1, Name = "Admin" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var anime = new Anime { Id = 1, EnglishTitle = "Test Anime" };
            context.Animes.Add(anime);
            context.Roles.Add(role);
            context.SaveChanges();

            // Act
            var result = await controller.DeleteAnime(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.True(returnValue); ;
        }

        [Fact]
        public async Task DeleteAnime_ReturnsBadRequest_WhenUserIsNotAdmin()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);

            var role = new Role { RoleId = 2, Name = "Client" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var anime = new Anime { Id = 1, EnglishTitle = "Test Anime" };
            context.Animes.Add(anime);
            context.Roles.Add(role);
            context.SaveChanges();

            // Act
            var result = await controller.DeleteAnime(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The given token has no permission to make this request", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteAnime_ReturnsOkResult_WhenAnimeIsNotFound()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeRepository animeRepo = new AnimeRepository(context);
            RoleRepository roleRepo = new RoleRepository(context);
            AnimeController controller = new AnimeController(animeRepo, roleRepo);


            var role = new Role { RoleId = 1, Name = "Admin" };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
             new Claim(ClaimTypes.Role, role.RoleId.ToString())
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };


            // Act
            var result = await controller.DeleteAnime(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<bool>(okResult.Value);
            Assert.False(returnValue); ;
        }

        #region Favourite Endpoint Tests
        /// <summary>
        /// Auxiliar method for getting the controller configuration
        /// </summary>
        /// <param name="context"></param>
        /// <param name="role"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private AnimeController SetupController(AnimeDBContext context, string role, string userId)
        {
            var animeRepo = new AnimeRepository(context);
            var roleRepo = new RoleRepository(context);
            var controller = new AnimeController(animeRepo, roleRepo);

            var userClaims = new List<Claim>();
            if (!string.IsNullOrEmpty(role))
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }
            if (!string.IsNullOrEmpty(userId))
            {
                userClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            }

            var user = new ClaimsPrincipal(new ClaimsIdentity(userClaims, "mock"));
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task AddFavouriteAnime_ReturnsOkResult_WhenClientUserAddsFavourite()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            context.Roles.Add(new Role { RoleId = 2, Name = "Client" });
            context.Users.Add(new User { UserId = 1, RoleId = 2, Username = "Test User" });
            context.SaveChanges();

            AnimeController controller = SetupController(context, "2", "1");

            // Act
            var result = await controller.AddFavouriteAnime(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result); 
            Assert.True((bool)okResult.Value);
        }

        [Fact]
        public async Task AddFavouriteAnime_ReturnsBadRequest_WhenAdminUserAddsFavourite()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            context.Roles.Add(new Role { RoleId = 1, Name = "Admin" });
            context.Users.Add(new User { UserId = 1, RoleId = 1, Username = "Test Admin" });
            context.SaveChanges();

            AnimeController controller = SetupController(context, "1", "1");

            // Act
            var result = await controller.AddFavouriteAnime(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("The given token has no permission to make this request", badRequestResult.Value);
        }

        [Fact]
        public async Task AddFavouriteAnime_ReturnsBadRequest_WhenRoleClaimIsMissing()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            AnimeController controller = SetupController(context, null, "1");

            // Act
            var result = await controller.AddFavouriteAnime(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Needed atributes are not provided in authentication", badRequestResult.Value);
        }

        [Fact]
        public async Task AddFavouriteAnime_ReturnsBadRequest_WhenUserClaimIsMissing()
        {
            // Arrange
            AnimeDBContext context = GetInMemoryDbContext();
            context.Roles.Add(new Role { RoleId = 2, Name = "Client" });
            context.SaveChanges();

            AnimeController controller = SetupController(context, "2", null);

            // Act
            var result = await controller.AddFavouriteAnime(1);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Needed atributes are not provided in authentication", badRequestResult.Value);
        }
        #endregion
    }
}
