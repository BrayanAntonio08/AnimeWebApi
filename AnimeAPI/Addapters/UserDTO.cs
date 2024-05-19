using AnimeAPI.Models;

namespace AnimeAPI.Addapters
{
    /// <summary>
    /// The User DTO is a simpler representation of the user and role entity, so for role just the id is needed.
    /// In this case to avoid complexity the adapter functions are integrated as static methods in this class
    /// </summary>
    public class UserDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int RoleId { get; set; }

        /// <summary>
        /// Value used for validating if the user can register as an admin user
        /// </summary>
        public string? AdminCode { get; set; }

        public static User Map(UserDTO userDTO)
        {
            return new User()
            {
                UserId = userDTO.Id,
                Username = userDTO.Username,
                Password = userDTO.Password,
                RoleId = userDTO.RoleId
            };
        }

        public static UserDTO Map(User user)
        {
            return new UserDTO()
            {
                Id = user.UserId,
                Username = user.Username,
                Password = user.Password,
                RoleId = user.RoleId
            };
        }

    }
}
