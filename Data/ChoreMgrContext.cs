using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChoreMgr.Models;

namespace ChoreMgr.Data
{
    public class ChoreMgrContext : DbContext
    {
        public ChoreMgrContext (DbContextOptions<ChoreMgrContext> options)
            : base(options)
        {
        }

        public DbSet<ChoreMgr.Models.Chore> Chores { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Chore>().ToTable("Chore");
        }
    }
}
