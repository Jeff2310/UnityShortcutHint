using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using System.Text;
using System.Linq;

public class ShortcutHintWindow : EditorWindow
{
    struct ShortcutPair
    {
        public KeyCombination       Combination;
        public string               Name;
        public string               Prefix;
    }
    private List<string>            ShortcutIDs;
    private List<string>            IncludePrefixes;
    private List<string>            ExcludePrefixes;
    private List<ShortcutPair>      ShortcutPairs;
    private Vector2 scrollPos;

    const string KEY_PREFIX_STR = "ShortcutHintWindow/Prefix string";
    const string PREFIX_STR_DEFAULT = 
        "!Main Menu/Assets/Create;"+
        "Animation;Curve Editor;" +
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
    private static string prefixString = PREFIX_STR_DEFAULT;

    [MenuItem("Tools/Shortcut Hint")]
    public static void OpenWindow()
    {
        var window = GetWindow(typeof(ShortcutHintWindow), false, "Shortcut Hint", true);
        (window as ShortcutHintWindow).LoadShortcut();
    }

    [PreferenceItem("ShortcutHint")]
    public static void PerferencesGUI()
    {
        if (!prefsLoaded)
        {
            prefixString = EditorPrefs.GetString(KEY_PREFIX_STR, PREFIX_STR_DEFAULT);
            prefsLoaded = true;
        }

        EditorGUILayout.LabelField("Version 0.01");
        prefixString = EditorGUILayout.TextField("Prefix String", prefixString);

        if (GUI.changed)
        {
            EditorPrefs.SetString(KEY_PREFIX_STR, prefixString);
        }
    }

    public void LoadShortcut()
    {
        // check prefix preferences
        string s_prefix;
        if(!EditorPrefs.HasKey(KEY_PREFIX_STR))
        {
            EditorPrefs.SetString(KEY_PREFIX_STR, PREFIX_STR_DEFAULT);
            s_prefix = PREFIX_STR_DEFAULT;
        }
        else
        {
            s_prefix = EditorPrefs.GetString(KEY_PREFIX_STR);
        }
        // prefix parser
        IncludePrefixes = new List<string>();
        ExcludePrefixes = new List<string>();
        var prefixes = s_prefix.Split(';');
        foreach (var prefix in prefixes)
        {
            if (prefix[0] == '!')
                ExcludePrefixes.Add(prefix.TrimStart('!'));
            else
                IncludePrefixes.Add(prefix);
        }

        var scm = ShortcutManager.instance;
        ShortcutIDs = new List<string>(scm.GetAvailableShortcutIds());
        ShortcutPairs = new List<ShortcutPair>();
        var builder = new StringBuilder();
        foreach (var s in ShortcutIDs)
        {
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
            //if (c.modifiers != ShortcutModifiers.None)
            if (true)
            {
                var trimmedName = s.Split('/').Last();
                ShortcutPairs.Add(new ShortcutPair { Combination = c, Name = trimmedName, Prefix = prefix});
            }
        }
    }

    private void OnGUI()
    {
        var shiftPressed = Event.current.shift;
        var ctrlPressed = Event.current.control;
        var altPressed = Event.current.alt;

        var lastPrefix = "";
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach(var sp in ShortcutPairs)
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
}
