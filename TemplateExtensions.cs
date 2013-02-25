using System.Collections.Generic;
using System.Linq;
using DotNetWikiBot;
using System.Text.RegularExpressions;

namespace DotNetWikiBotExtensions
{
    public static class TemplateExtension
    {
        /// <summary>
        /// Get all templates.
        /// </summary>
        /// <returns>Returns all templates used on the specified site</returns>
        public static IEnumerable<Template> GetAllTemplates(this Page page)
        {
            return page.GetTemplatesWithParams().Select(s => new Template(page, s));
        }
    }

    public class Template
    {
        internal Template(Page p, string s)
        {
            Page = p;
            Text = s;
            Parameters = p.site.ParseTemplate(s);
            Title = Page.site.RemoveNSPrefix(Regex.Match(s, @"^(.+?)($|\|)", RegexOptions.Multiline).Groups[1].Value.Trim(), 10);
        }

        /// <summary>
        /// The page containing this template
        /// </summary>
        public Page Page { get; private set; }

        /// <summary>
        /// The Title of the template
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The innertext of the template (including title, bot NOT the outer brackets)
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The parameters of the template
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Save changes of the template to the page
        /// </summary>
        public void Save()
        {
            var oldTemplate = new Regex(Regex.Escape(Text));
            var newTemplate = Page.site.FormatTemplate(Title, Parameters, Text);
            newTemplate = newTemplate.Substring(2, newTemplate.Length - 4);
            Page.text = oldTemplate.Replace(Page.text, newTemplate, 1);
        }
    }
}