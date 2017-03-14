using System.Threading.Tasks;

namespace Jasper.Diagnostics.Util
{
    public class HtmlAttribute
    {
        public HtmlAttribute(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }

        public async Task WriteHtml(IHtmlTextWriter writer)
        {
            if(Value != null)
            {
                await writer.WriteAsync($"{Key}=\"{Value}\"");
            }
            else
            {
                await writer.WriteAsync($"{Key}");
            }
        }
    }
}
