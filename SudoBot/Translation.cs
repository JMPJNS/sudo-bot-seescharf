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
        
        public Dictionary<String, Line> Lines = new ();

        public String Translate(String line, Lang lang, List<string> args)
        {
            var found = Lines[line.ToUpper()];

            return FillArgs(found.Text[lang], args);
        }
        
        private String FillArgs(String line, List<String> args)
        {
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
            Lines.Add("TEST", new Line("Test in {{}}", "Test in {{}}"));
            
            Lines.Add("RANKING_CHANNEL_NOT_ALLOWED", new Line("not allowed in this channel, please use {{}}", "In diesem Channel nicht erlaubt, bitte in {{}} verwenden!"));
        }
    }
}