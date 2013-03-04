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
        private static readonly Regex HeaderRegex = new Regex(@"^(=+)\s?([^=]+)\s?\1\s*$", RegexOptions.Multiline);

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
    }

    public class Section
    {
        public string Content { get; set; }
        public Page Page { get; private set; }
        public int Level { get; private set; }
        public IEnumerable<Section> Subsections { get; private set; }
        private string _originalContent;
    
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