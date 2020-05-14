namespace Jasper.Http.ContentHandling
{
    public class SetContentType : SetHeader
    {
        public SetContentType(string value) : base("content-type", value)
        {
        }
    }
}
