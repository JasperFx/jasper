namespace Jasper.Diagnostics.Util
{
    public class ScriptTag : HtmlTag
    {
        public ScriptTag(string src) : base("script")
        {
            Attr("type", "text/javascript");
            Attr("src", src);
        }
    }
}
