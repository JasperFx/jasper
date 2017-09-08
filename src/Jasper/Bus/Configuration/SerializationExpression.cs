using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;

namespace Jasper.Bus.Configuration
{
    public class SerializationExpression
    {
        private readonly ServiceBusFeature _parent;
        private readonly JasperRegistry _top;


        public SerializationExpression(ServiceBusFeature parent, JasperRegistry top)
        {
            _parent = parent;
            _top = top;
        }


        public SerializationExpression Add<T>() where T : ISerializer
        {
            _parent.Services.For<ISerializer>().Add<T>();
            return this;
        }


        public void DisallowNonVersionedSerialization()
        {
            _top.Settings.Alter<BusSettings>(x => x.AllowNonVersionedSerialization = false);
        }
    }
}
