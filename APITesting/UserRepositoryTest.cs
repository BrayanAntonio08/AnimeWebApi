using AnimeAPI.Data;
using AnimeAPI.Models;
using AnimeAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITesting
{
    /// <summary>
    /// The class test the methods for user repository and data control in DB
    /// the definitions of methods are provided by ChatGPT using a temporary memory
    /// The memory is created by a guid so that the context is infividual for any test
    /// </summary>
    public class UserRepositoryTest
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
        public async Task AddAsync_AddsUserSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "testuser", Password = "password" };

            // Act
            var result = await repository.AddAsync(user);

            // Assert
            var addedUser = await dbContext.Users.FindAsync(result.UserId);
            Assert.NotNull(addedUser);
            Assert.Equal(user.Username, addedUser.Username);
            Assert.NotEqual("password", addedUser.Password); // Password should be encrypted
        }

        [Fact]
        public async Task AddAsync_ThrowsException_WhenUsernameAlreadyExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "testuser", Password = "password" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act & Assert
            var newUser = new User { Username = "testuser", Password = "newpassword" };
            await Assert.ThrowsAsync<Exception>(async () => await repository.AddAsync(newUser));
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllUsers()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var users = new List<User>
            {
                new User { Username = "user1", Password = "password1" },
                new User { Username = "user2", Password = "password2" }
            };
            dbContext.Users.AddRange(users);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "user1", Password = "password1" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.GetByIdAsync(user.UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task RemoveAsync_RemovesUserSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "user1", Password = "password1" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await repository.RemoveAsync(user.UserId);

            // Assert
            Assert.True(result);
            Assert.Null(await dbContext.Users.FindAsync(user.UserId));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserSuccessfully()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "user1", Password = "password1" };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            user.Password = "newpassword";

            // Act
            var result = await repository.UpdateAsync(user);

            // Assert
            var updatedUser = await dbContext.Users.FindAsync(result.UserId);
            Assert.NotNull(updatedUser);
            Assert.Equal(result.Password, updatedUser.Password);
        }

        [Fact]
        public async Task Login_ReturnsUser_WhenCredentialsAreCorrect()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "user1", Password = "password1" };
            user.Password = repository.ComputeSha256Hash(user.Password); // Encrypt password before adding
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            var loginUser = new User { Username = "user1", Password = "password1" };

            // Act
            var result = await repository.Login(loginUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task Login_ReturnsNull_WhenCredentialsAreIncorrect()
        {
            // Arrange
            var dbContext = GetInMemoryDbContext();
            var repository = new UserRepository(dbContext);
            var user = new User { Username = "user1", Password = "password1" };
            user.Password = repository.ComputeSha256Hash(user.Password); // Encrypt password before adding
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            var loginUser = new User { Username = "user1", Password = "wrongpassword" };

            // Act
            var result = await repository.Login(loginUser);

            // Assert
            Assert.Null(result);
        }


    }
}
