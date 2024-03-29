﻿using Cleanish.App.Data.Database.Entities.Fields;

namespace Cleanish.App.Logic.Users.Models;

public class UserRes
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public UserRole Role { get; set; }
}
