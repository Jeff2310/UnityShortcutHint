class AnimationFilter : CommandFilter
{
    public override void AddSelf()
    {
        ShortcutHelper.WindowFilters.Add("UnityEditor.AnimationWindow", new AnimationFilter());
    }

    public override bool IsCommandAvaliable(CommandPair command)
    {
        return command.Prefix == "Animation";
    }
}
