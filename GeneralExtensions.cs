using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;

namespace DotNetWikiBotExtensions
{
    public static class GeneralExtensions
    {
        public static IEnumerable<Page> ToEnumerable(this PageList pl)
        {
            foreach (Page p in pl) yield return p;
        }

        private static readonly Dictionary<Page, Dictionary<string, string>> Placeholders = new Dictionary<Page, Dictionary<string, string>>();
        private static readonly Dictionary<Placeholder, Regex> PlaceholderRegexes = new Dictionary<Placeholder, Regex>()
            {
                { Placeholder.ExternalLinks, new Regex(@"(?<!\[)\[[^[\]]*\](?!\])") },
                { Placeholder.InternalLinks, new Regex(@"\[\[.+?\]\][^\s\[]*") },
                { Placeholder.InterwikiLinks, new Regex(@"\[\[(en|es|fr):.+?\]\]") },
                { Placeholder.Categorys, new Regex(@"\[\[(Category|Kategorie):.+?\]\]") },
                { Placeholder.Templates, new Regex(@"\{\{((?<x>\{\{)|[^{}]|\{\{\{[^}]+\}\}\}|(?<-x>\}\}))*\}\}(?(x)(?!))") },
                { Placeholder.Nowiki, new Regex(@"\<nowiki\>.*?\<\/nowiki\>") },
                { Placeholder.Noinclude, new Regex(@"\<noinclude\>.*?\<\/noinclude\>") },
                { Placeholder.Includeonly, new Regex(@"\<includeonly\>.*?\<\/includeonly\>") },
                { Placeholder.Comments, new Regex(@"\<!--.*?--\>") }
            };
        public static void InsertPlaceholders(this Page page, Placeholder placeholder)
        {
            if (!Placeholders.ContainsKey(page))
                Placeholders.Add(page, new Dictionary<string, string>());

            InternalInsertPlaceholder(page, placeholder, Placeholder.ExternalLinks);
            InternalInsertPlaceholder(page, placeholder, Placeholder.InternalLinks);
            InternalInsertPlaceholder(page, placeholder, Placeholder.InterwikiLinks);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Categorys);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Templates);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Nowiki);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Noinclude);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Includeonly);
            InternalInsertPlaceholder(page, placeholder, Placeholder.Comments);
        }
        private static void InternalInsertPlaceholder(Page page, Placeholder placeholder, Placeholder p)
        {
            if (placeholder.HasFlag(p))
                page.text = PlaceholderRegexes[p].Replace(page.text, match =>
                {
                    var x = "~:" + Placeholders[page].Count + ":~";
                    Placeholders[page].Add(x, match.Value);
                    return x;
                });
        }
        public static void RemovePlaceholders(this Page page)
        {
            if (!Placeholders.ContainsKey(page)) return;
            for (var i = Placeholders[page].Count - 1; i >= 0; --i)
            {
                var key = Placeholders[page].ElementAt(i).Key;
                page.text = page.text.Replace(key, Placeholders[page][key]);
                Placeholders[page].Remove(key);
            }
            Placeholders.Remove(page);
        }

        [Flags]
        public enum Placeholder
        {
            None = 0,
            ExternalLinks = 1,
            InternalLinks = 2,
            InterwikiLinks = 4,
            Categorys = 8,
            Templates = 16,
            Nowiki = 32,
            Noinclude = 64,
            Includeonly = 128,
            Comments = 256,

            /// <summary>
            /// ExternalLinks | InterwikiLinks | Comments
            /// </summary>
            Default = ExternalLinks | InterwikiLinks | Comments
        }
    }
}