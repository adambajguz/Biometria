using System.Collections.Generic;
using KeystrokeDynamics.Storage.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace KeystrokeDynamics.Storage
{
    public class KeystrokeDynamicsContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=keystrokeDynamics.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //https://www.jerriepelser.com/blog/store-dictionary-as-json-using-ef-core-21/
            modelBuilder.Entity<User>()
                .Property(b => b.KeystrokeVector)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<char, long>>(v));
        }
    }
}