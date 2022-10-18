﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Onion.App.Data.Database.Entities;
using Onion.Shared.Helpers;

namespace Onion.Impl.App.Data.Database.EntityConfigurations;

internal class TodoListConfiguration : BaseEntityConfiguration<TodoList>
{
    public override void Configure(EntityTypeBuilder<TodoList> builder)
    {
        Guard.NotNull(builder, nameof(builder));

        base.Configure(builder);

        builder.Property(i => i.Title).IsRequired();
        builder.HasIndex(i => i.Title).IsUnique();
    }
}