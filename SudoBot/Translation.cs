using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SudoBot
{
    public class Translation
    {
        public enum Lang
        {
            En,
            De
        }
        public class Line
        {
            public Dictionary<Lang, String> Text = new ();

            public Line(String en, String de = null)
            {
                if (en != null)
                    Text.Add(Lang.En, en);
                if (de != null)
                    Text.Add(Lang.De, de);
            }
        }
        
        private Dictionary<String, Line> _lines = new ();

        public String Translate(String line, Lang lang, List<string> args = null)
        {
            var found = _lines[line.ToUpper()];

            return FillArgs(found.Text[lang], args);
        }
        
        private String FillArgs(String line, List<String> args)
        {
            if (args == null) return line;
            var regex = new Regex(Regex.Escape("{{}}"));

            foreach (var arg in args)
            {
                line = regex.Replace(line, arg, 1);
            }
            
            return line;
        }

        public Translation()
        {
            AddLines();
        }

        private void AddLines()
        {
            _lines.Add("TEST", new Line("Test in {{}}", "Test in {{}}"));
            
            _lines.Add("RANKING_CHANNEL_NOT_ALLOWED", new Line("not allowed in this channel, please use {{}}", "In diesem Channel nicht erlaubt, bitte in {{}} verwenden!"));
            _lines.Add("RANKING_BOT_NO_PERMISSION", new Line("Sudo doesn't have permission to grant this role, grant manage roles permission and move the role under the sudo role", 
                "Sudo hat keine Rechte diese Rolle zu vergeben, es wird dazu die manage roles permission benötigt und die Rolle muss unter der Sudo Rolle sein"));
            _lines.Add("DONE", new Line("done", "fertig"));
        }
    }
}