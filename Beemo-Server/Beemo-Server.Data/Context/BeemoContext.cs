﻿using Beemo_Server.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beemo_Server.Data.Context
{
    public class BeemoContext : DbContext
    {
        public BeemoContext(DbContextOptions<BeemoContext> options)
            : base(options) { }

        #region DbSets
        public DbSet<User> Users { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
