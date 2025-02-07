﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AspNetCore.Identity.CosmosDb.EntityConfigurations
{
    public class RoleClaimEntityTypeConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<string>>
    {
        private readonly string _tableName;

        public RoleClaimEntityTypeConfiguration(string tableName = "Identity_Roles")
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<IdentityRoleClaim<string>> builder)
        {
            builder
                .Property(_ => _.Id)
                .HasConversion(_ => _.ToString(), _ => Convert.ToInt32(_));

            builder
                .UseETagConcurrency()
                .HasPartitionKey(_ => _.Id);

            builder.ToContainer(_tableName);
        }
    }
}