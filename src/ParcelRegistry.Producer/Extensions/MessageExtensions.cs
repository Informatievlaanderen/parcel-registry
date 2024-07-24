namespace ParcelRegistry.Producer.Extensions
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Contracts = Be.Vlaanderen.Basisregisters.GrAr.Contracts.ParcelRegistry;
    using Legacy = Legacy.Events;
    using ParcelAggregate = Parcel.Events;

    public static class MessageExtensions
    {
        #region Legacy

        public static Contracts.ParcelAddressWasAttached ToContract(this Legacy.ParcelAddressWasAttached message) =>
            new Contracts.ParcelAddressWasAttached(message.ParcelId.ToString("D"), message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasDetached ToContract(this Legacy.ParcelAddressWasDetached message) =>
            new Contracts.ParcelAddressWasDetached(message.ParcelId.ToString("D"), message.AddressId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasCorrectedToRealized ToContract(this Legacy.ParcelWasCorrectedToRealized message) =>
            new Contracts.ParcelWasCorrectedToRealized(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasCorrectedToRetired ToContract(this Legacy.ParcelWasCorrectedToRetired message) =>
            new Contracts.ParcelWasCorrectedToRetired(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasMarkedAsMigrated ToContract(this Legacy.ParcelWasMarkedAsMigrated message) =>
            new Contracts.ParcelWasMarkedAsMigrated(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasRecovered ToContract(this Legacy.ParcelWasRecovered message) =>
            new Contracts.ParcelWasRecovered(message.ParcelId.ToString("D"), message.Provenance.ToContract());

         public static Contracts.ParcelWasRemoved ToContract(this Legacy.ParcelWasRemoved message) =>
            new Contracts.ParcelWasRemoved(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasRetired ToContract(this Legacy.ParcelWasRetired message) =>
            new Contracts.ParcelWasRetired(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasRealized ToContract(this Legacy.ParcelWasRealized message) =>
            new Contracts.ParcelWasRealized(message.ParcelId.ToString("D"), message.Provenance.ToContract());

        public static Contracts.ParcelWasRegistered ToContract(this Legacy.ParcelWasRegistered message) =>
            new Contracts.ParcelWasRegistered(message.ParcelId.ToString("D"), message.VbrCaPaKey, message.Provenance.ToContract());

        #endregion

        public static Contracts.ParcelAddressWasAttachedV2 ToContract(this ParcelAggregate.ParcelAddressWasAttachedV2 message) =>
            new Contracts.ParcelAddressWasAttachedV2(message.ParcelId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasReplacedBecauseOfMunicipalityMerger ToContract(this ParcelAggregate.ParcelAddressWasReplacedBecauseOfMunicipalityMerger message) =>
            new Contracts.ParcelAddressWasReplacedBecauseOfMunicipalityMerger(message.ParcelId.ToString("D"), message.CaPaKey, message.NewAddressPersistentLocalId, message.PreviousAddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasDetachedV2 ToContract(this ParcelAggregate.ParcelAddressWasDetachedV2 message) =>
            new Contracts.ParcelAddressWasDetachedV2(message.ParcelId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasDetachedBecauseAddressWasRejected ToContract(this ParcelAggregate.ParcelAddressWasDetachedBecauseAddressWasRejected message) =>
            new Contracts.ParcelAddressWasDetachedBecauseAddressWasRejected(message.ParcelId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasDetachedBecauseAddressWasRetired ToContract(this ParcelAggregate.ParcelAddressWasDetachedBecauseAddressWasRetired message) =>
            new Contracts.ParcelAddressWasDetachedBecauseAddressWasRetired(message.ParcelId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasDetachedBecauseAddressWasRemoved ToContract(this ParcelAggregate.ParcelAddressWasDetachedBecauseAddressWasRemoved message) =>
            new Contracts.ParcelAddressWasDetachedBecauseAddressWasRemoved(message.ParcelId.ToString("D"), message.CaPaKey, message.AddressPersistentLocalId, message.Provenance.ToContract());

        public static Contracts.ParcelAddressWasReplacedBecauseAddressWasReaddressed ToContract(this ParcelAggregate.ParcelAddressWasReplacedBecauseAddressWasReaddressed message) =>
            new Contracts.ParcelAddressWasReplacedBecauseAddressWasReaddressed(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.NewAddressPersistentLocalId,
                message.PreviousAddressPersistentLocalId,
                message.Provenance.ToContract());

        public static Contracts.ParcelAddressesWereReaddressed ToContract(this ParcelAggregate.ParcelAddressesWereReaddressed message) =>
            new Contracts.ParcelAddressesWereReaddressed(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.AttachedAddressPersistentLocalIds,
                message.DetachedAddressPersistentLocalIds,
                message.AddressRegistryReaddresses.Select(x =>
                    new Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.AddressRegistryReaddress(
                        x.SourceAddressPersistentLocalId,
                        x.SourceAddressPersistentLocalId)),
                message.Provenance.ToContract());

        public static Contracts.ParcelWasMigrated ToContract(this ParcelAggregate.ParcelWasMigrated message) =>
            new Contracts.ParcelWasMigrated(
                message.OldParcelId.ToString("D"),
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.ParcelStatus,
                message.IsRemoved,
                message.AddressPersistentLocalIds,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.ParcelWasImported ToContract(this ParcelAggregate.ParcelWasImported message) =>
            new Contracts.ParcelWasImported(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.ParcelWasRetiredV2 ToContract(this ParcelAggregate.ParcelWasRetiredV2 message) =>
            new Contracts.ParcelWasRetiredV2(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.Provenance.ToContract());

        public static Contracts.ParcelGeometryWasChanged ToContract(this ParcelAggregate.ParcelGeometryWasChanged message) =>
            new Contracts.ParcelGeometryWasChanged(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        public static Contracts.ParcelWasCorrectedFromRetiredToRealized ToContract(this ParcelAggregate.ParcelWasCorrectedFromRetiredToRealized message) =>
            new Contracts.ParcelWasCorrectedFromRetiredToRealized(
                message.ParcelId.ToString("D"),
                message.CaPaKey,
                message.ExtendedWkbGeometry,
                message.Provenance.ToContract());

        private static Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance ToContract(this ProvenanceData provenance)
        => new Be.Vlaanderen.Basisregisters.GrAr.Contracts.Common.Provenance(
            provenance.Timestamp.ToString(),
            provenance.Application.ToString(),
            provenance.Modification.ToString(),
            provenance.Organisation.ToString(),
            provenance.Reason);
    }
}
