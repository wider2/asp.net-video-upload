using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VircaVideoCore.Models;

namespace VircaVideoCore.Dal
{
    public class VircaContext : DbContext
    {
        public VircaContext()
        {
        }

        public VircaContext(DbContextOptions<VircaContext> options) : base(options)
        {
            //otherwise, AddDbContext was called with configuration, but the context type 'VircaContext' only declares a parameterless constructor. This means that the configuration passed to AddDbContext will never be used.
        }

        public static string ConnectionString { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();
            var builder = new DbContextOptionsBuilder<VircaContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            ConnectionString = connectionString;

            optionsBuilder.UseSqlServer(ConnectionString);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            /*
            modelBuilder.Entity<StoredProcRow>(entity =>
            {
                
            });
            */
        }


        public virtual DbSet<Virca_Orders_Video> Virca_Orders_Video { get; set; }


    }
}
