﻿using TasksManagement.Core.Enums;

namespace TasksManagement.Core.Entities;

public class TaskEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Status Status { get; set; }
    public string? AssignedTo { get; set; }
}
