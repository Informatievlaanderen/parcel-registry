namespace ParcelRegistry.Projections.Syndication.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using System.Threading;
    using System.Threading.Tasks;

    public class AddressPersistentLocalIdProjection : AtomEntryProjectionHandlerModule<AddressEvent, SyndicationContent<Address>, SyndicationContext>
    {
        public AddressPersistentLocalIdProjection()
        {
            When(AddressEvent.AddressWasRegistered, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameComplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressBecameIncomplete, AddSyndicationItemEntry);
            When(AddressEvent.AddressPersistentLocalIdentifierWasAssigned, AddSyndicationItemEntry);
            When(AddressEvent.AddressWasRemoved, RemoveSyndicationItemEntry);

            When(AddressEvent.AddressBecameCurrent, DoNothing);
            When(AddressEvent.AddressBecameNotOfficiallyAssigned, DoNothing);
            When(AddressEvent.AddressWasProposed, DoNothing);
            When(AddressEvent.AddressWasRetired, DoNothing);
            When(AddressEvent.AddressWasOfficiallyAssigned, DoNothing);
            When(AddressEvent.AddressWasCorrectedToCurrent, DoNothing);
            When(AddressEvent.AddressWasCorrectedToProposed, DoNothing);
            When(AddressEvent.AddressWasCorrectedToRetired, DoNothing);
            When(AddressEvent.AddressStatusWasCorrectedToRemoved, DoNothing);
            When(AddressEvent.AddressStatusWasRemoved, DoNothing);
            When(AddressEvent.AddressWasCorrectedToNotOfficiallyAssigned, DoNothing);
            When(AddressEvent.AddressWasCorrectedToOfficiallyAssigned, DoNothing);
            When(AddressEvent.AddressOfficialAssignmentWasRemoved, DoNothing);

            When(AddressEvent.AddressBoxNumberWasChanged, DoNothing);
            When(AddressEvent.AddressBoxNumberWasCorrected, DoNothing);
            When(AddressEvent.AddressBoxNumberWasRemoved, DoNothing);

            When(AddressEvent.AddressHouseNumberWasChanged, DoNothing);
            When(AddressEvent.AddressHouseNumberWasCorrected, DoNothing);

            When(AddressEvent.AddressWasPositioned, DoNothing);
            When(AddressEvent.AddressPositionWasRemoved, DoNothing);
            When(AddressEvent.AddressPositionWasCorrected, DoNothing);

            When(AddressEvent.AddressPostalCodeWasChanged, DoNothing);
            When(AddressEvent.AddressPostalCodeWasCorrected, DoNothing);
            When(AddressEvent.AddressPostalCodeWasRemoved, DoNothing);

            When(AddressEvent.AddressStreetNameWasChanged, DoNothing);
            When(AddressEvent.AddressStreetNameWasCorrected, DoNothing);
        }

        private static async Task AddSyndicationItemEntry(AtomEntry<SyndicationContent<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressId = Guid.Parse(entry.Content.Object.AddressId);

            var latestItem =
                await context
                    .AddressPersistentLocalIds
                    .FindAsync(addressId);

            if (latestItem == null)
            {
                latestItem = new AddressPersistentLocalIdItem
                {
                    AddressId = addressId,
                    Version = entry.Content.Object.Identificator?.Versie,
                    Position = long.Parse(entry.FeedEntry.Id),
                    PersistentLocalId = entry.Content.Object.Identificator?.ObjectId,
                    IsComplete = entry.Content.Object.IsComplete,
                };

                await context
                      .AddressPersistentLocalIds
                      .AddAsync(latestItem, ct);
            }
            else
            {
                latestItem.Version = entry.Content.Object.Identificator?.Versie;
                latestItem.Position = long.Parse(entry.FeedEntry.Id);
                latestItem.PersistentLocalId = entry.Content.Object.Identificator?.ObjectId;
                latestItem.IsComplete = entry.Content.Object.IsComplete;
            }
        }

        private static async Task RemoveSyndicationItemEntry(AtomEntry<SyndicationContent<Address>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressId = Guid.Parse(entry.Content.Object.AddressId);
            var latestItem =
                await context
                    .AddressPersistentLocalIds
                    .FindAsync(addressId);

            latestItem.Version = entry.Content.Object.Identificator?.Versie;
            latestItem.Position = long.Parse(entry.FeedEntry.Id);
            latestItem.PersistentLocalId = entry.Content.Object.Identificator?.ObjectId;
            latestItem.IsComplete = entry.Content.Object.IsComplete;
            latestItem.IsRemoved = true;
        }

        private static Task DoNothing(AtomEntry<SyndicationContent<Address>> entry, SyndicationContext context, CancellationToken ct) => Task.CompletedTask;
    }
}
