namespace ParcelRegistry.Projections.Syndication.Address
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using System.Threading;
    using System.Threading.Tasks;

    public class AddressOsloIdProjection : AtomEntryProjectionHandlerModule<AddressEvent, SyndicationContent<Address>, SyndicationContext>
    {
        public AddressOsloIdProjection()
        {
            When(AddressEvent.AddressWasRegistered, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameComplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameIncomplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressOsloIdWasAssigned, AddSyndicationItemEntry);
            When(AddressEvent.AddressWasRemoved, RemoveSyndicationItemEntry);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationContent<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .AddressOsloIds
                .FindAsync(entry.Content.Object.AddressId);

            if (latestItem == null)
            {
                latestItem = new AddressOlsoIdItem
                {
                    AddressId = entry.Content.Object.AddressId,
                    Version = entry.Content.Object.Identificator?.Versie.Value,
                    Position = long.Parse(entry.FeedEntry.Id),
                    OsloId = entry.Content.Object.Identificator?.ObjectId,
                    IsComplete = entry.Content.Object.IsComplete,
                };

                await context
                      .AddressOsloIds
                      .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.Version = entry.Content.Object.Identificator?.Versie.Value;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.OsloId = entry.Content.Object.Identificator?.ObjectId;
                latestItem.IsComplete = entry.Content.Object.IsComplete;
            }
        }

        private static async Task RemoveSyndicationItemEntry(AtomEntry<SyndicationContent<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var latestItem = await context
                .AddressOsloIds
                .FindAsync(entry.Content.Object.AddressId);

            latestItem.Version = entry.Content.Object.Identificator?.Versie.Value;
            latestItem.Position = long.Parse(entry.FeedEntry.Id);
            latestItem.OsloId = entry.Content.Object.Identificator?.ObjectId;
            latestItem.IsComplete = entry.Content.Object.IsComplete;
            latestItem.IsRemoved = true;
        }
    }
}
