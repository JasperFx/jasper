using Jasper.Attributes;

namespace InMemoryMediator.Items
{
    // SAMPLE: InMemoryMediator-Items
    public class ItemHandler
    {
        // This attribute applies Jasper's EF Core transactional
        // middleware
        [Transactional]
        public static ItemCreated Handle(
            // This would be the message
            CreateItemCommand command,

            // Any other arguments are assumed
            // to be service dependencies
            ItemsDbContext db)
        {
            // Create a new Item entity
            var item = new Item
            {
                Name = command.Name
            };

            // Add the item to the current
            // DbContext unit of work
            db.Items.Add(item);

            // This event being returned
            // by the handler will be automatically sent
            // out as a "cascading" message
            return new ItemCreated
            {
                Id = item.Id
            };
        }
    }
    // ENDSAMPLE
}
