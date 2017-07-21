using Jasper.Conneg;

namespace Jasper.Bus
{
    public class SerializationExpression
    {
        private readonly ServiceBusFeature _parent;



        public SerializationExpression(ServiceBusFeature parent)
        {
            _parent = parent;
        }


        public SerializationExpression Add<T>() where T : ISerializer
        {
            _parent.Services.For<ISerializer>().Add<T>();
            return this;
        }


        /// <summary>
        /// Specify or override the preferred order of serialization usage for the application
        /// </summary>
        /// <param name="contentTypes"></param>
        /// <returns></returns>
        public SerializationExpression ContentPreferenceOrder(params string[] contentTypes)
        {
            _parent.Channels.AcceptedContentTypes.Clear();
            _parent.Channels.AcceptedContentTypes.AddRange(contentTypes);
            return this;
        }
    }
}