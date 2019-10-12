namespace JasperHttp.ContentHandling
{
    public class SetContentType : SetHeader
    {
        public SetContentType(string value) : base("content-type", value)
        {
        }
    }
}
