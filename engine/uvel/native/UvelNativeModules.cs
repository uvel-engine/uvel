using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Uvel.Native2D
{
    public static class UvelComponentRegistry
    {
        private static Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static UvelComponentRegistry()
        {
            Register("view", "View"); Register("uvelview", "View");
            Register("card", "Card"); Register("uvelcard", "Card"); Register("glasscard", "Card");
            Register("button", "Button"); Register("uvelbutton", "Button");
            Register("input", "Input"); Register("uvelinput", "Input"); Register("textbox", "Input");
            Register("text", "Text"); Register("textblock", "Text"); Register("label", "Text");
            Register("list", "List"); Register("uvelist", "List");
            Register("listview", "ListView"); Register("uvellistview", "ListView");
            Register("item", "ListItem"); Register("listitem", "ListItem");
            Register("scroll", "Scroll"); Register("scrollview", "Scroll"); Register("uvelscroll", "Scroll");
            Register("tabs", "Tabs"); Register("tabcontrol", "Tabs"); Register("tab", "Tab"); Register("tabitem", "Tab");
            Register("contextmenu", "ContextMenu"); Register("menu", "ContextMenu");
            Register("icon", "Icon"); Register("uvelicon", "Icon");
            Register("divider", "Divider"); Register("badge", "Badge");
            Register("checkbox", "Checkbox"); Register("check", "Checkbox");
            Register("select", "Select"); Register("dropdown", "Select");
        }

        public static void Register(string name, string canonical)
        {
            if (!string.IsNullOrEmpty(name)) _aliases[name] = canonical;
        }

        public static string Resolve(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            string v;
            return _aliases.TryGetValue(name, out v) ? v : name;
        }
    }

    public class UvelBackendRegistry
    {
        private Dictionary<string, Action<UvelNativeContext>> _commands = new Dictionary<string, Action<UvelNativeContext>>(StringComparer.OrdinalIgnoreCase);

        public UvelBackendRegistry()
        {
            Register("uvel.backend.ping", delegate(UvelNativeContext ctx) { ctx.SetText("status", "Backend ready"); });
            Register("uvel.backend.time", delegate(UvelNativeContext ctx) { ctx.SetText("status", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); });
            Register("uvel.backend.clear", delegate(UvelNativeContext ctx) { ctx.SetText("status", ""); });
            Register("uvel.database.save", delegate(UvelNativeContext ctx) { UvelDatabase.Set("lastRun", DateTime.Now.ToString("o")); ctx.SetText("status", "Saved to Uvel database"); });
            Register("uvel.database.load", delegate(UvelNativeContext ctx) { ctx.SetText("status", "DB lastRun: " + UvelDatabase.Get("lastRun", "empty")); });
        }

        public void Register(string name, Action<UvelNativeContext> action)
        {
            if (!string.IsNullOrEmpty(name) && action != null) _commands[name] = action;
        }

        public bool Execute(string name, UvelNativeContext ctx)
        {
            Action<UvelNativeContext> a;
            if (!_commands.TryGetValue(name, out a)) return false;
            a(ctx);
            return true;
        }
    }

    public class UvelNativeContext
    {
        private Action<string, string> _setText;
        public UvelNativeContext(Action<string, string> setText) { _setText = setText; }
        public void SetText(string target, string value) { if (_setText != null) _setText(target, value); }
    }

    public static class UvelDatabase
    {
        private static readonly object _lock = new object();
        private static string DbPath
        {
            get
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Uvel", "Database");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "uvel.db.txt");
            }
        }

        public static void Set(string key, string value)
        {
            lock (_lock)
            {
                Dictionary<string, string> map = ReadAll();
                map[key] = value ?? "";
                WriteAll(map);
            }
        }

        public static string Get(string key, string fallback)
        {
            lock (_lock)
            {
                Dictionary<string, string> map = ReadAll();
                string v;
                return map.TryGetValue(key, out v) ? v : fallback;
            }
        }

        private static Dictionary<string, string> ReadAll()
        {
            Dictionary<string, string> map = new Dictionary<string, string>();
            if (!File.Exists(DbPath)) return map;
            foreach (string line in File.ReadAllLines(DbPath, Encoding.UTF8))
            {
                int i = line.IndexOf('=');
                if (i <= 0) continue;
                map[Unescape(line.Substring(0, i))] = Unescape(line.Substring(i + 1));
            }
            return map;
        }

        private static void WriteAll(Dictionary<string, string> map)
        {
            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> kv in map) lines.Add(Escape(kv.Key) + "=" + Escape(kv.Value));
            File.WriteAllLines(DbPath, lines.ToArray(), Encoding.UTF8);
        }

        private static string Escape(string s) { return (s ?? "").Replace("\\", "\\\\").Replace("\n", "\\n").Replace("=", "\\e"); }
        private static string Unescape(string s) { return (s ?? "").Replace("\\e", "=").Replace("\\n", "\n").Replace("\\\\", "\\"); }
    }

    public static class UvelIconRegistry
    {
        public static string IconText(string name)
        {
            if (string.IsNullOrEmpty(name)) return "◆";
            switch (name.ToLower())
            {
                case "play": return "▶";
                case "download": return "↓";
                case "settings": return "⚙";
                case "user": return "●";
                case "code": return "</>";
                case "database": return "▣";
                case "sparkle": return "✦";
                default: return "◇";
            }
        }
    }
}
