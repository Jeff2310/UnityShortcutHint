public abstract class CommandFilter
{
    public abstract void AddSelf();

    public virtual bool IsCommandAvaliable(CommandPair command)
    {
        return true;
    }
}