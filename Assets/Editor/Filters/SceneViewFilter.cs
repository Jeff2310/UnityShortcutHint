class SceneViewFilter : CommandFilter
{
    public override void AddSelf()
    {
        ShortcutHelper.WindowFilters.Add("UnityEditor.SceneView", new SceneViewFilter());
    }

    public override bool IsCommandAvaliable(CommandPair command)
    {
        if (command.Prefix == "Scene View" || command.Prefix == "Scene Visibility")
            return true;
        return false;
    }
}