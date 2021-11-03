using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Checkers.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Checkers.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<BoardState> BoardStates { get; set; }
        public DbSet<Field> Field { get; set; }
        public DbSet<Message> Messages { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Message>()
                .HasOne<User>(u => u.User)
                .WithMany(m => m.Messages)
                .HasForeignKey(m => m.UserId);
        }
    }
}
