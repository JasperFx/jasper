using System;
using System.Collections.Generic;
using System.Linq;
using StoryTeller.Results;
using StoryTeller.Util;

namespace Jasper.Storyteller.Logging
{
    public class BusErrors : Report
    {
        public readonly IList<Exception> Exceptions = new List<Exception>();

        public string ToHtml()
        {
            var div = new HtmlTag("div");

            foreach (var exception in Exceptions.Where(x => x != null).ToArray())
                div.Add("pre").Style("margin", "10px").Text(exception.ToString());


            return div.ToString();
        }

        public string Title { get; } = "Logged Bus Errors";
        public string ShortTitle { get; } = "Bus Errors";
        public int Count => Exceptions.Count;
    }
}
