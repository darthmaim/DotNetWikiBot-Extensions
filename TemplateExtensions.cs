using System;
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
        private bool _removed = false;
        private string _title;
        private string _text;
        private Dictionary<string, string> _parameters;

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
        public string Title
        {
            get { return _title; }
            set
            {
                ThrowIfRemoved();
                _title = value;
            }
        }

        /// <summary>
        /// The innertext of the template (including title, bot NOT the outer brackets)
        /// </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                ThrowIfRemoved();
                _text = value;
            }
        }

        /// <summary>
        /// The parameters of the template
        /// </summary>
        public Dictionary<string, string> Parameters
        {
            get { return _parameters; }
            set
            {
                ThrowIfRemoved();
                _parameters = value;
            }
        }

        public void ChangeParametername(string oldKey, string newKey)
        {
            ThrowIfRemoved();

            if (oldKey == null)
                throw new ArgumentNullException("oldKey");
            if (newKey == null)
                throw new ArgumentNullException("newKey");

            var newParameter = new Dictionary<string, string>(Parameters.Count);

            foreach (var pair in Parameters)
            {
                newParameter.Add(pair.Key.Equals(oldKey) ? newKey : pair.Key, pair.Value);
            }

            Parameters = newParameter;
        }


        /// <summary>
        /// Save changes of the template to the page
        /// </summary>
        public void Save()
        {
            if(_removed) return;

            var oldTemplate = new Regex(Regex.Escape(Text));
            var newTemplate = Page.site.FormatTemplate(Title, Parameters, Text);
            newTemplate = newTemplate.Substring(2, newTemplate.Length - 4);
            Page.text = oldTemplate.Replace(Page.text, newTemplate, 1);
        }

        /// <summary>
        /// Removes the template from the Page
        /// </summary>
        public void Remove()
        {
            ThrowIfRemoved("This template is already removed");

            Page.text = Regex.Replace(Page.text, string.Format(@"{0}([^\S\n]*\n)?", Regex.Escape("{{" + Text + "}}")), "");

            _removed = true;
        }

        private void ThrowIfRemoved(string msg = "This template is removed")
        {
            if(_removed) throw new Exception(msg);
        }
    }
}