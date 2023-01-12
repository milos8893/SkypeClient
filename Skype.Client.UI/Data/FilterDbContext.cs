using Microsoft.EntityFrameworkCore;
using Skype.Client.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.Data
{
    internal class FilterDbContext : DbContext
    {
        public FilterDbContext(DbContextOptions<FilterDbContext> options)
           : base(options)
        {
            Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Filter>()
            //    .HasKey(e => e.Id);

            //modelBuilder.Entity<SourceProfileVM>()
            //    .HasKey(s => s.Id);
            //modelBuilder.Entity<DestinationProfileVM>()
            //    .HasKey(d => d.Id);

            modelBuilder.Entity<SourceProfileVM>()
              .HasOne<Filter>(d => d.Filter)
              .WithMany(dm => dm.SourceChats)
              .HasForeignKey(dkey => dkey.FilterId)
              .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DestinationProfileVM>()
              .HasOne<Filter>(d => d.Filter)
              .WithMany(dm => dm.DestinationChats)
              .HasForeignKey(dkey => dkey.FilterId)
              .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Filter> Filters { get; set; }
        public DbSet<SourceProfileVM> SourceProfiles { get; set; }
        public DbSet<DestinationProfileVM> DestinationProfiles { get; set; }
    }
}
