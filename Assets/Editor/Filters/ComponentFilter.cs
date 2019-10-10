using UnityEditor;
using UnityEngine;

class ComponentFilter : CommandFilter
{
    public override void AddSelf()
    {
        ShortcutHelper.GlobalFilters.Add(new ComponentFilter());
    }

    public override bool IsCommandAvaliable(CommandPair command)
    {
        if(command.Prefix == "ParticleSystem")
            return Selection.activeGameObject?.GetComponent<ParticleSystem>() != null;
        if(command.Prefix == "Terrain")
            return Selection.activeGameObject?.GetComponent<Terrain>() != null;
        return false;
    }
}
