﻿using Beemo_Server.Data.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Beemo_Server.Data.Context
{
    public class BeemoContext : DbContext
    {
        public BeemoContext(DbContextOptions<BeemoContext> options)
            : base(options) { }

    }
}
