﻿namespace TaskForge.Domain.Enums
{
    public enum ProjectRole
    {
        Admin = 0,   // Full control: can manage project, members, and settings
        Contributor = 1,   // Can create, update, and manage tasks but no admin rights
        Viewer = 2     // Can only view project and tasks
    }
}
