namespace Jasper.Diagnostics.Util
{
    public class CssTag : HtmlTag
    {
        public CssTag(string href) : base("link")
        {
            Attr("rel", "stylesheet");
            Attr("href", href);
            Attr("type", "text/css");
            Attr("media", "all");
        }
    }
}
