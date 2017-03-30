using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Jasper.Diagnostics.Util
{
    public class HtmlTag
    {
        private readonly Dictionary<string, HtmlAttribute> _attributes = new Dictionary<string, HtmlAttribute>();
        private readonly List<HtmlTag> _children = new List<HtmlTag>();
        private string _innerText;

        public HtmlTag(string tag)
        {
            Tag = tag.ToLower();
        }

        public string Tag { get; }
        public string InnerText => _innerText;

        public IEnumerable<HtmlTag> Children => _children;

        public string Attr(string key)
        {
            HtmlAttribute attr;

            if(_attributes.TryGetValue(key, out attr))
            {
                return attr.Value;
            }

            return null;
        }

        public HtmlTag Attr(string key, string value)
        {
            var attr = new HtmlAttribute(key, value);
            _attributes[key] = attr;
            return this;
        }

        public HtmlTag Append(HtmlTag tag)
        {
            _children.Add(tag);
            return this;
        }

        public HtmlTag Append(params HtmlTag[] tags)
        {
            foreach (var tag in tags)
            {
                Append(tag);
            }

            return this;
        }

        public HtmlTag Text(string text)
        {
            _innerText = text;
            return this;
        }

        public virtual async Task WriteHtml(IHtmlTextWriter writer)
        {
            await WriteBeginningTag(writer);
            await WriteContent(writer);
            await WriteEndingTag(writer);
        }

        protected virtual async Task WriteBeginningTag(IHtmlTextWriter writer)
        {
            await writer.WriteAsync($"<{Tag}");
            foreach(var attr in _attributes.Values)
            {
                await writer.WriteAsync(" ");
                await attr.WriteHtml(writer);
            }
            await writer.WriteAsync(">");
        }

        protected virtual async Task WriteContent(IHtmlTextWriter writer)
        {
            if(_innerText != null)
            {
                await writer.WriteAsync(_innerText);
            }

            foreach(var child in Children)
            {
                await child.WriteHtml(writer);
            }
        }

        protected virtual async Task WriteEndingTag(IHtmlTextWriter writer)
        {
            await writer.WriteAsync($"</{Tag}>");
        }

        public override string ToString()
        {
            using(var stream = new MemoryStream())
            {
                var writer = new ResponseHtmlTextWriter(stream);

                WriteHtml(writer).GetAwaiter().GetResult();

                stream.Position = 0;
                var result = Encoding.UTF8.GetString(stream.ToArray());
                return result;
            }
        }
    }
}
