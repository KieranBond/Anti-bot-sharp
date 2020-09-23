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
        ListBlacklist,
        ListFilter,
        GetID,
        Cleanup
    }
}
