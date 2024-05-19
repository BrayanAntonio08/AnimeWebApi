using AnimeAPI.Data;
using AnimeAPI.IRepositories;
using AnimeAPI.Models;
using AnimeAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace APITesting
{
    /// <summary>
    /// The class test the methods for anime repository and data control in DB
    /// the definitions of methods are provided by ChatGPT using a temporary memory
    /// The memory is created by a guid so that the context is infividual for any test
    /// </summary>
    public class AnimeRepositoryTest
    {


        private AnimeDBContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AnimeDBContext>()
                .UseInMemoryDatabase(databaseName: $"AnimeDatabase_{Guid.NewGuid()}")
                .Options;

            var dbContext = new AnimeDBContext(options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsAnime_WhenAnimeExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Naruto" };
            dbContext.Animes.Add(anime);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Naruto", result.EnglishTitle);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllAnimes()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime1 = new Anime { Id = 1, EnglishTitle = "Naruto", JapaneseTitle = "ナルト" };
            var anime2 = new Anime { Id = 2, EnglishTitle = "One Piece", JapaneseTitle = "ワンピース" };
            dbContext.Animes.AddRange(anime1, anime2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddAsync_AddsAnimeSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { EnglishTitle = "Attack on Titan", JapaneseTitle = "進撃の巨人" };

            // Act
            var result = await repository.AddAsync(anime);

            // Assert
            var addedAnime = await dbContext.Animes.FindAsync(result.Id);
            Assert.NotNull(addedAnime);
            Assert.Equal("Attack on Titan", addedAnime.EnglishTitle);
        }

        [Fact]
        public async Task AddRangeAsync_AddsAnimesSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var animes = new List<Anime>
            {
                new Anime { EnglishTitle = "My Hero Academia", JapaneseTitle = "僕のヒーローアカデミア" },
                new Anime { EnglishTitle = "Demon Slayer", JapaneseTitle = "鬼滅の刃" }
            };

            // Act
            var result = await repository.AddRangeAsync(animes);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal(2, await dbContext.Animes.CountAsync());
        }

        [Fact]
        public async Task UpdateAsync_UpdatesAnimeSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Fairy Tail", JapaneseTitle = "フェアリーテイル" };
            dbContext.Animes.Add(anime);
            await dbContext.SaveChangesAsync();
            anime.EnglishTitle = "Fairy Tail Updated";

            // Act
            var result = await repository.UpdateAsync(anime);

            // Assert
            var updatedAnime = await dbContext.Animes.FindAsync(result.Id);
            Assert.NotNull(updatedAnime);
            Assert.Equal("Fairy Tail Updated", updatedAnime.EnglishTitle);
        }

        [Fact]
        public async Task RemoveAsync_RemovesAnimeSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Death Note", JapaneseTitle = "デスノート" };
            dbContext.Animes.Add(anime);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.RemoveAsync(1);

            // Assert
            Assert.True(result);
            Assert.Null(await dbContext.Animes.FindAsync(1));
        }

        [Fact]
        public async Task FindByName_ReturnsCorrectAnimes()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime1 = new Anime { EnglishTitle = "Naruto", JapaneseTitle = "ナルト" };
            var anime2 = new Anime { EnglishTitle = "Naruto Shippuden", JapaneseTitle = "ナルト 疾風伝" };
            dbContext.Animes.AddRange(anime1, anime2);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.FindByName("Naruto");

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.EnglishTitle == "Naruto");
            Assert.Contains(result, a => a.EnglishTitle == "Naruto Shippuden");
        }

        [Fact]
        public async Task AddFavourite_ReturnsTrue_WhenFavouriteIsAdded()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Naruto" };
            var role = new Role { RoleId = 2, Name = "Client" };
            var user = new User { UserId = 1, Username = "user1", Role = role };

            dbContext.Animes.Add(anime);
            dbContext.Roles.Add(role);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.AddFavourite(anime.Id, user.UserId);

            // Assert
            Assert.True(result);
            Assert.Single(dbContext.Favourites);
            var favourite = dbContext.Favourites.First();
            Assert.Equal(anime.Id, favourite.AnimeId);
            Assert.Equal(user.UserId, favourite.UserId);
        }

        [Fact]
        public async Task AddFavourite_ReturnsFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Naruto" };

            dbContext.Animes.Add(anime);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.AddFavourite(anime.Id, 999); // Non-existent user ID

            // Assert
            Assert.False(result);
            Assert.Empty(dbContext.Favourites);
        }

        [Fact]
        public async Task AddFavourite_ReturnsFalse_WhenUserIsAdmin()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new AnimeRepository(dbContext);
            var anime = new Anime { Id = 1, EnglishTitle = "Naruto" };
            var role = new Role { RoleId = 1, Name = "Admin" };
            var user = new User { UserId = 1, Username = "admin", Role = role };

            dbContext.Animes.Add(anime);
            dbContext.Roles.Add(role);
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.AddFavourite(anime.Id, user.UserId);

            // Assert
            Assert.False(result);
            Assert.Empty(dbContext.Favourites);
        }

        [Fact]
        public async Task RemoveFavourite_RemovesSuccessfully()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var repository = new AnimeRepository(context);

            var favourite = new Favourite { AnimeId = 1, UserId = 1 };
            context.Favourites.Add(favourite);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.DeleteFavourite(1, 1);

            // Assert
            Assert.True(result);

            // Verify the favourite was deleted
            var deletedFavourite = await context.Favourites.FindAsync(1, 1);
            Assert.Null(deletedFavourite);
        }
    }
}