using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.Data
{
    internal class FilterDbContextFactory : IDesignTimeDbContextFactory<FilterDbContext>
    {
        public FilterDbContext CreateDbContext(string[]? args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<FilterDbContext>();
            optionsBuilder.UseSqlite("Data Source = Filters.db");

            return new FilterDbContext(optionsBuilder.Options);
        }
    }
}
