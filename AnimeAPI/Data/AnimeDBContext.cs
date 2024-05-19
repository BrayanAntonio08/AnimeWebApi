using AnimeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimeAPI.Data
{
    public class AnimeDBContext : DbContext
    {
        public DbSet<Anime> Animes { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Favourite> Favourites { get; set; }

        public AnimeDBContext(DbContextOptions<AnimeDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .HasConstraintName("fk__user__role");
            });

            base.OnModelCreating(modelBuilder);
        }

    }
}
