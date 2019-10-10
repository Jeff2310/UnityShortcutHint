class ToolsFilter : CommandFilter
{
    public override void AddSelf()
    {
        ShortcutHelper.GlobalFilters.Add(new ToolsFilter());
    }

    public override bool IsCommandAvaliable(CommandPair command)
    {
        if (command.Prefix == "Tools")
            return UnityEditor.Selection.activeTransform != null;
        return false;
    }
}