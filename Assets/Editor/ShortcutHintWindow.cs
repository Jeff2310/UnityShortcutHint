using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Text;
using System.Linq;

public class ShortcutHintWindow : EditorWindow
{
   
    private List<string>            ShortcutIDs;
    private List<string>            IncludePrefixes;
    private List<string>            ExcludePrefixes;
    private List<CommandPair>       CommandPairs;
    private List<CommandPair>       CommonCommandPairs;

    private Vector2 scrollPos;

    const string KEY_PREFIX_STR = "ShortcutHintWindow/Prefix string";
    const string KEY_COMMON_SHORTCUTS = "ShortcutHintWindow/Main Menu shortcuts";
    const string PREFIX_STR_DEFAULT = 
        "!Main Menu/Assets/Create;"+
        "Main Menu" +
        "Animation;" +
        "Curve Editor;"  +
        "ParticleSystem;" +
        "Scene View;" +
        "Scene Visibility;" +
        "Stage;" +
        "Terrain;" +
        "Timeline;" +
        "Tools;" +
        "Version Control;" +
        "Window";
    private static bool prefsLoaded = false;
    private static bool showCommonShortcuts;
    private static string prefixString;

    public void OnEnable()
    {
        if (!prefsLoaded)
        {
            prefixString = EditorPrefs.GetString(KEY_PREFIX_STR, PREFIX_STR_DEFAULT);
            showCommonShortcuts = EditorPrefs.GetBool(KEY_COMMON_SHORTCUTS, false);
            prefsLoaded = true;
        }
        ShortcutManager.instance.activeProfileChanged += ReloadShortcut;
        ShortcutManager.instance.shortcutBindingChanged += ReloadShortcut;
        LoadShortcut();
    }

    public void OnDisable()
    {
        ShortcutManager.instance.activeProfileChanged -= ReloadShortcut;
        ShortcutManager.instance.shortcutBindingChanged -= ReloadShortcut;
    }

    [MenuItem("Tools/Shortcut Hint")]
    public static void OpenWindow()
    {
        GetWindow(typeof(ShortcutHintWindow), false, "Shortcut Hint", true);
    }

    [PreferenceItem("ShortcutHint")]
    public static void PerferencesGUI()
    {
        EditorGUILayout.LabelField("Version 0.01");
        
        prefixString = EditorGUILayout.TextField("Prefix String", prefixString);
        if (GUI.changed)
        {
            EditorPrefs.SetString(KEY_PREFIX_STR, prefixString);
        }
        showCommonShortcuts = EditorGUILayout.Toggle("Always show common shortcuts", showCommonShortcuts);
        if (GUI.changed)
        {
            EditorPrefs.SetBool(KEY_COMMON_SHORTCUTS, showCommonShortcuts);
        }
    }

    public void LoadShortcut()
    {
        // prefix parser
        IncludePrefixes = new List<string>();
        ExcludePrefixes = new List<string>();
        var prefixes = prefixString.Split(';');
        foreach (var prefix in prefixes)
        {
            if (prefix[0] == '!')
                ExcludePrefixes.Add(prefix.TrimStart('!'));
            else
                IncludePrefixes.Add(prefix);
        }

        var scm = ShortcutManager.instance;
        var ids = scm.GetAvailableShortcutIds();
        ShortcutIDs = new List<string>(ids);
        CommandPairs = new List<CommandPair>();
        CommonCommandPairs = new List<CommandPair>();
        for(int i=0; i<ShortcutIDs.Count; i++)
        {
            var s = ShortcutIDs[i];
            // filter prefix
            bool flag = false;
            string prefix = "";
            foreach (var ep in ExcludePrefixes)
            {
                if (s.StartsWith(ep))
                {
                    flag = true;
                    break;
                }
            }
            if (flag) continue;
            foreach (var ip in IncludePrefixes)
            {
                if (s.StartsWith(ip))
                {
                    flag = true;
                    prefix = ip;
                    break;
                }
            }
            if (!flag) continue;
            // get shortcut name and keycode
            var cs = scm.GetShortcutBinding(s).keyCombinationSequence;
            if (cs.Count() == 0) continue;
            var c = cs.First();
            var trimmedName = s.Replace(prefix+'/', "");

            var pairList = (prefix == "Main Menu" || prefix == "Window")
                        ? CommonCommandPairs : CommandPairs;
            pairList = prefix == "Window" ? CommonCommandPairs : pairList;
            pairList.Add(new CommandPair { Combination = c, Name = trimmedName, Prefix = prefix, Index = i});
        }
        // sort by prefix, than index in shortcut
        CommandPairs.Sort((x, y) => (x.Prefix == y.Prefix ? (x.Index - y.Index) : (string.Compare(x.Prefix, y.Prefix))));
    }

    private void OnGUI()
    {
        var shiftPressed = Event.current.shift;
        var ctrlPressed = Event.current.control;
        var altPressed = Event.current.alt;
        
        //if(focusedWindow != null)
        //    EditorGUILayout.LabelField(focusedWindow.ToString());

        List<CommandPair> commands;
        if (showCommonShortcuts)
        {
            commands = ShortcutHelper.GetActiveCommands(CommandPairs);
            commands.AddRange(CommonCommandPairs);
        }
        else
        {
            commands = new List<CommandPair>(CommandPairs);
            commands.AddRange(CommonCommandPairs);
            commands = ShortcutHelper.GetActiveCommands(commands);
        }
        var lastPrefix = "";
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        var modifiers = string.Format("{0}{1}{2}...", shiftPressed ? "Shift+" : "", ctrlPressed ? "Ctrl+" : "", altPressed ? "Alt+" : "");
        EditorGUILayout.LabelField(modifiers, EditorStyles.boldLabel);
        foreach (var sp in commands)
        {
            if ((sp.Combination.shift ^ shiftPressed)
                || (sp.Combination.action ^ ctrlPressed)
                || (sp.Combination.alt ^ altPressed))
                continue;
            if (sp.Prefix != lastPrefix)
                EditorGUILayout.LabelField(sp.Prefix, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(sp.Combination.keyCode.ToString() + ": ", GUILayout.MaxWidth(200));
            EditorGUILayout.LabelField(sp.Name);
            EditorGUILayout.EndHorizontal();
            lastPrefix = sp.Prefix;
        }
        EditorGUILayout.EndScrollView();
    }

    private void Update()
    {
        Repaint();
    }

    private void ReloadShortcut(ShortcutBindingChangedEventArgs args)
    {
        LoadShortcut();
    }

    private void ReloadShortcut(ActiveProfileChangedEventArgs args)
    {
        LoadShortcut();
    }
}
