namespace ParcelRegistry.Projections.Integration.ParcelVersion
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using System.Linq;
    using Microsoft.EntityFrameworkCore;

    public static class ParcelVersionExtensions
    {
        public static async Task<ParcelVersion> CreateNewParcelVersion<T>(
            this IntegrationContext context,
            Guid parcelId,
            Envelope<T> message,
            Action<ParcelVersion> applyEventInfoOn,
            CancellationToken ct,
            bool renewAddresses = true) where T : IHasProvenance, IMessage
        {
            var parcelVersion = await context.LatestPosition(parcelId, ct);

            if (parcelVersion is null)
            {
                throw DatabaseItemNotFound(parcelId);
            }

            var provenance = message.Message.Provenance;

            var newParcelVersion = parcelVersion.CloneAndApplyEventInfo(
                message.Position,
                provenance.Timestamp,
                applyEventInfoOn);

            await context
                .ParcelVersions
                .AddAsync(newParcelVersion, ct);

            if (renewAddresses)
            {
                var parcelVersionAddresses = await LatestParcelAddress(context, parcelId, parcelVersion.Position, ct);
                foreach (var parcelVersionAddress in parcelVersionAddresses)
                {
                    var newParcelAddressVersion = parcelVersionAddress.CloneAndApplyEventInfo(
                        message.Position,
                        provenance.Timestamp);

                    await context
                        .ParcelVersionAddresses
                        .AddAsync(newParcelAddressVersion, ct);
                }
            }

            return newParcelVersion;
        }

        static async Task<ParcelVersion?> LatestPosition(
            this IntegrationContext context,
            Guid parcelId,
            CancellationToken ct)
            => context
                   .ParcelVersions
                   .Local
                   .Where(x => x.ParcelId == parcelId)
                   .MaxBy(x => x.Position)
               ?? await context
                   .ParcelVersions
                   .Where(x => x.ParcelId == parcelId)
                   .OrderByDescending(x => x.Position)
                   .FirstOrDefaultAsync(ct);

        static async Task<List<ParcelVersionAddress>> LatestParcelAddress(
            this IntegrationContext context,
            Guid parcelId,
            long position,
            CancellationToken ct)
        {
            var localAddresses = context
                .ParcelVersionAddresses
                .Local
                .Where(x => x.ParcelId == parcelId && x.Position == position)
                .ToList();

            if (!localAddresses.Any())
            {
                return await context
                    .ParcelVersionAddresses
                    .Where(x => x.ParcelId == parcelId && x.Position == position)
                    .OrderByDescending(x => x.Position)
                    .ToListAsync(cancellationToken: ct);
            }

            return localAddresses;
        }


        private static ProjectionItemNotFoundException<ParcelVersionProjections> DatabaseItemNotFound(Guid parcelId)
            => new(parcelId.ToString());
    }
}
