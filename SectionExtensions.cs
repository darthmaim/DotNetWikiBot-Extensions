using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DotNetWikiBot;

namespace DotNetWikiBotExtensions
{
    public static class SectionExtensions
    {
        internal static readonly Regex HeaderRegex = new Regex(@"^(=+)\s?([^=]+?)\s?\1\s*$", RegexOptions.Multiline);

        public static IEnumerable<Section> GetAllSections(this Page page)
        {
            return GetAllSections(page.text, page);
        }

        public static IEnumerable<Section> GetAllSections(string text, Page page, int minLevel = 1)
        {
            var headerMatches = HeaderRegex.Matches(text).Cast<Match>()
                                           .OrderBy(m => m.Groups[1].Length)
                                           .Where(m => m.Groups[1].Length >= minLevel)
                                           .ToList();


            if(!headerMatches.Any()) yield break;

            var lowestHeading = headerMatches.First().Groups[1].Length;

            var lastHeadingPos = 0;
            var headingsFound = 0;

            foreach (var hMatch in headerMatches.Where(m => m.Groups[1].Length == lowestHeading))
            {
                headingsFound++;

                if(hMatch.Index > 0 && headingsFound > 1)
                    yield return new Section(text.Substring(lastHeadingPos, hMatch.Index - lastHeadingPos), page, lowestHeading);
                lastHeadingPos = hMatch.Index;
            }
            if(lastHeadingPos < text.Length)
                yield return new Section(text.Substring(lastHeadingPos, text.Length - lastHeadingPos), page, lowestHeading);
        } 

        public static Section GetByName(this Page page, string name, bool recursive)
        {
            return GetByName(page.GetAllSections(), name, recursive);
        }

        public static Section GetByName(this IEnumerable<Section> allSections, string name, bool recursive)
        {
            var sections = new Queue<Section>(allSections);

            while (sections.Count > 0)
            {
                var section = sections.Dequeue();
                if (section.Title == name) return section;

                if (recursive)
                    foreach (var s in section.Subsections) sections.Enqueue(s);
            }

            return null;
        }

        public static Section GetByPath(this Page page, params string[] path)
        {
            return GetByPath(page.GetAllSections(), path);
        }

        public static Section GetByPath(this IEnumerable<Section> allSections, params string[] path)
        {
            if(path.Length == 0) throw  new ArgumentException("path cant be empty");

            var depth = 0;
            var sections = new Queue<Section>(allSections);

            while(sections.Count > 0)
            {
                var section = sections.Dequeue();
                if (section.Title != path[depth]) continue;

                depth++;
                if (depth == path.Length)
                    return section;
                sections = new Queue<Section>(section.Subsections);
            }

            return null;
        }
    }

    public class Section
    {
        private string _originalContent;
        private string _title;
        private string _content;
        public Page Page { get; private set; }
        public int Level { get; private set; }
        public IEnumerable<Section> Subsections { get; private set; }
        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                var match = SectionExtensions.HeaderRegex.Match(_content);
                Level = match.Groups[1].Length;
                _title = match.Groups[2].Value;
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                SectionExtensions.HeaderRegex.Replace(Content, string.Format(@"\1 {0} \1", value));
            }
        }

        public Section(string content, Page page, int level)
        {
            _originalContent = content;
            Content = content;
            Page = page;
            Level = level;

            Subsections = SectionExtensions.GetAllSections(Content, page, level + 1);
        }

        /// <summary>
        /// Saves changes to the section to the page. This does NOT save the page.
        /// </summary>
        public void Save()
        {
            Page.text = Page.text.Replace(_originalContent, Content);
            _originalContent = Content;
        }
    }
}