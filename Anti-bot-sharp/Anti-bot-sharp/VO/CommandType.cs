namespace AntiBotSharp.VO
{
    public enum CommandType
    {
        None,
        Help,
        AddAdmin,
        RemoveAdmin,
        AddBlacklist,
        RemoveBlacklist,
        AddFilter,
        RemoveFilter,
        AddTimeout,
        RemoveTimeout,
        SetTimeoutRole,
        ListBlacklist,
        ListFilter,
        ListAdmins,
        AuditLogTarget,
        ToggleAuditLog,
        GetID,
        Cleanup
    }
}
