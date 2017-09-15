using System.Collections.Generic;
using StoryTeller.Results;
using StoryTeller.Util;

namespace Jasper.Storyteller.Logging
{
    public class BusActivity : Report
    {
        public readonly IList<string> Messages = new List<string>();

        public string ToHtml()
        {
            var ul = new HtmlTag("ul");

            foreach (var message in Messages)
            {
                ul.Add("li").Text(message);
            }


            return ul.ToString();
        }

        public string Title { get; } = "Bus Activity";
        public string ShortTitle { get; } = "Bus Activity";
        public int Count => Messages.Count;
    }
}