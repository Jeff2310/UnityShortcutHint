class TimelineFilter : CommandFilter
{
    public override void AddSelf()
    {
        ShortcutHelper.WindowFilters.Add("UnityEditor.Timeline.TimelineWindow", new TimelineFilter());
    }

    public override bool IsCommandAvaliable(CommandPair command)
    {
        return command.Prefix == "Timeline";
    }
}
