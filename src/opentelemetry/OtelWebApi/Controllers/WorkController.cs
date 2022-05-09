using System.Threading.Tasks;
using Jasper;
using Microsoft.AspNetCore.Mvc;
using OtelMessages;

namespace OtelWebApi.Controllers
{
    public class WorkController : ControllerBase
    {
        private readonly IMessagePublisher _publisher;

        public WorkController(IMessagePublisher publisher)
        {
            _publisher = publisher;
        }

        [HttpPost("/local/now")]
        public Task Now()
        {
            return _publisher.InvokeAsync(new Work());
        }

        [HttpPost("/local/later")]
        public Task Later()
        {
            return _publisher.EnqueueAsync(new Work());
        }

        [HttpPost("/subscriber1/inline")]
        public Task PublishInline()
        {
            return _publisher.PublishAsync(new InlineMessage());
        }
    }
}
