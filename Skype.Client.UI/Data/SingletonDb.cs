using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skype.Client.UI.Data
{
    static class SingletonDb
    {
        public static readonly FilterDbContext FilterDb;
        static SingletonDb()
        {
            FilterDb = new FilterDbContextFactory().CreateDbContext(null);
            FilterDb.ChangeTracker.LazyLoadingEnabled = false;
        }
    }
}
