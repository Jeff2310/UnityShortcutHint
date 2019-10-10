using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class ShortcutHelper
{
    public static Dictionary<string, CommandFilter> WindowFilters = new Dictionary<string, CommandFilter>();
    public static List<CommandFilter> GlobalFilters = new List<CommandFilter>();

    static ShortcutHelper()
    {
        var asm = Assembly.GetCallingAssembly();
        var allTypes = asm.GetTypes();
        var cfType = typeof(CommandFilter);
        // add all CommandFilter
        foreach(var t in allTypes)
        {
            if(cfType.IsAssignableFrom(t) && t != cfType)
            {
                var filter = Activator.CreateInstance(t);
                t.GetMethod("AddSelf").Invoke(filter, null);
            }
        }
    }

    public static List<CommandPair> GetActiveCommands(IEnumerable<CommandPair> commands)
    {
        var kw = EditorWindow.focusedWindow;
        var mw = EditorWindow.mouseOverWindow;
        var activeCommands = new List<CommandPair>();

        foreach (var c in commands)
        {
            if (kw != null) {
                CommandFilter wf;
                if (WindowFilters.TryGetValue(kw.GetType().FullName, out wf))
                {
                    if (wf.IsCommandAvaliable(c))
                    {
                        activeCommands.Add(c);
                        continue;
                    }
                }
            }
            for (int i = 0; i < GlobalFilters.Count; i++)
            {
                if (GlobalFilters[i].IsCommandAvaliable(c)) { 
                    activeCommands.Add(c);
                    continue;
                }
            }
        }
        return activeCommands;
    }
}
