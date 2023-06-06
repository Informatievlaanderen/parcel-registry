namespace ParcelRegistry.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Events;
    using Events.Crab;
    using Exceptions;
    using NodaTime;
    using ParcelRegistry.Parcel;
    using ParcelRegistry.Parcel.Commands;

    public partial class Parcel : AggregateRootEntity, ISnapshotable
    {
        public MigrateParcel CreateMigrateCommand(IEnumerable<AddressPersistentLocalId> addressPersistentLocalIds)
        {
            return new MigrateParcel(
                ParcelId,
                CaPaKey,
                IsRealized
                    ? ParcelStatus.Realized
                    : ParcelStatus.Retired,
                IsRemoved,
                addressPersistentLocalIds,
                XCoordinate is not null ? new Coordinate(XCoordinate) : null,
                YCoordinate is not null ? new Coordinate(YCoordinate) : null,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Migrate Parcel aggregate."),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen));
        }

        public static Parcel Register(ParcelId id, VbrCaPaKey vbrCaPaKey, IParcelFactory factory)
        {
            var parcel = factory.Create();
            parcel.ApplyChange(new ParcelWasRegistered(id, vbrCaPaKey));
            return parcel;
        }

        public void ImportTerrainObjectFromCrab(
            CrabTerrainObjectId terrainObjectId,
            CrabIdentifierTerrainObject identifierTerrainObject,
            CrabTerrainObjectNatureCode terrainObjectNatureCode,
            CrabCoordinate xCoordinate,
            CrabCoordinate yCoordinate,
            CrabBuildingNature buildingNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            if (IsRemoved && modification == CrabModification.Insert)
                ApplyChange(new ParcelWasRecovered(ParcelId));
            else if (IsRemoved)
                throw new ParcelRemovedException($"Cannot change removed parcel for parcel id {ParcelId}");

            if (modification == CrabModification.Delete)
            {
                ApplyChange(new ParcelWasRemoved(ParcelId));
            }
            else
            {
                if (lifetime.EndDateTime.HasValue && !IsRetired)
                {
                    if (modification == CrabModification.Correction)
                        ApplyChange(new ParcelWasCorrectedToRetired(ParcelId));
                    else
                        ApplyChange(new ParcelWasRetired(ParcelId));
                }
                else if (!lifetime.EndDateTime.HasValue && !IsRealized)
                {
                    if (modification == CrabModification.Correction)
                        ApplyChange(new ParcelWasCorrectedToRealized(ParcelId));
                    else
                        ApplyChange(new ParcelWasRealized(ParcelId));
                }
            }

            ApplyChange(new TerrainObjectWasImportedFromCrab(
                terrainObjectId,
                identifierTerrainObject,
                terrainObjectNatureCode,
                xCoordinate,
                yCoordinate,
                buildingNature,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        public void ImportTerrainObjectHouseNumberFromCrab(
            CrabTerrainObjectHouseNumberId terrainObjectHouseNumberId,
            CrabTerrainObjectId terrainObjectId,
            CrabHouseNumberId houseNumberId,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var legacyEvent = new TerrainObjectHouseNumberWasImportedFromCrab(
                terrainObjectHouseNumberId,
                terrainObjectId,
                houseNumberId,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation);

            var addressId = AddressId.CreateFor(houseNumberId);

            if (_addressCollection.Contains(addressId) &&
                (modification == CrabModification.Delete || lifetime.EndDateTime.HasValue))
            {
                if (_activeHouseNumberIdsByTerreinObjectHouseNr.Values.Count(x => x == houseNumberId) == 1)
                {
                    foreach (var addressIdToRemove in _addressCollection.AddressIdsEligableToRemoveFor(houseNumberId))
                        ApplyChange(new ParcelAddressWasDetached(ParcelId, addressIdToRemove));

                    ApplyChange(new ParcelAddressWasDetached(ParcelId, addressId));
                }
            }
            else
            {
                if (_activeHouseNumberIdsByTerreinObjectHouseNr.ContainsKey(terrainObjectHouseNumberId) &&
                    _activeHouseNumberIdsByTerreinObjectHouseNr[terrainObjectHouseNumberId] != houseNumberId)
                {
                    var currentCrabHouseNumberId = _activeHouseNumberIdsByTerreinObjectHouseNr[terrainObjectHouseNumberId];
                    if (_activeHouseNumberIdsByTerreinObjectHouseNr.Values.Count(x => x == currentCrabHouseNumberId) == 1)
                    {
                        foreach (var addressIdToRemove in _addressCollection.AddressIdsEligableToRemoveFor(
                            currentCrabHouseNumberId))
                            ApplyChange(new ParcelAddressWasDetached(ParcelId, addressIdToRemove));

                        ApplyChange(new ParcelAddressWasDetached(ParcelId,
                            AddressId.CreateFor(currentCrabHouseNumberId)));
                    }
                }

                if (!_addressCollection.Contains(addressId) &&
                    modification != CrabModification.Delete &&
                    !lifetime.EndDateTime.HasValue)
                {
                    ApplyChange(new ParcelAddressWasAttached(ParcelId, addressId));

                    foreach (var addressIdToAdd in _addressCollection.AddressIdsEligableToAddFor(houseNumberId))
                        ApplyChange(new ParcelAddressWasAttached(ParcelId, addressIdToAdd));
                }
            }

            ApplyChange(legacyEvent);
        }

        public void ImportSubaddressFromCrab(
            CrabSubaddressId subaddressId,
            CrabHouseNumberId houseNumberId,
            BoxNumber boxNumber,
            CrabBoxNumberType boxNumberType,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            GuardRemoved(modification);

            var addressId = AddressId.CreateFor(subaddressId);

            if (_addressCollection.Contains(AddressId.CreateFor(houseNumberId)) &&
                !_addressCollection.Contains(addressId) &&
                modification != CrabModification.Delete &&
                !lifetime.EndDateTime.HasValue)
            {
                ApplyChange(new ParcelAddressWasAttached(ParcelId, addressId));
            }
            else if (_addressCollection.Contains(addressId) &&
                     (modification == CrabModification.Delete || lifetime.EndDateTime.HasValue))
            {
                ApplyChange(new ParcelAddressWasDetached(ParcelId, addressId));
            }

            ApplyChange(new AddressSubaddressWasImportedFromCrab(
                subaddressId,
                houseNumberId,
                boxNumber,
                boxNumberType,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation));
        }

        /// <summary>
        /// Fixes issue where address attachments happened after the parcel was removed.
        /// Detaches all parcel addresses and re-applies removed event.
        /// </summary>
        public void FixGrar1475()
        {
            if (!IsRemoved)
                return;

            foreach (var addressId in _addressCollection.AllAddressIds().ToList())
                ApplyChange(new ParcelAddressWasDetached(ParcelId, addressId));

            ApplyChange(new ParcelWasRemoved(ParcelId));
        }

        /// <summary>
        /// Fixes issue where parcel should've been recovered but hasn't due to change applied later.
        /// </summary>
        public void FixGrar1637()
        {
            if (!IsRemoved)
                return;

            ApplyChange(new ParcelWasRecovered(ParcelId));
            ApplyChange(new ParcelWasRealized(ParcelId));
        }

        /// <summary>
        /// Fixes state of aggregate because of possible invalid snapshot.
        /// </summary>
        public void FixGrar3581(
            ParcelStatus parcelStatus,
            IList<AddressId> addressIds)
        {
            if (IsRemoved)
            {
                return;
            }

            if (parcelStatus == ParcelStatus.Realized && !IsRealized)
            {
                ApplyChange(new ParcelWasCorrectedToRealized(ParcelId));
            }

            if (parcelStatus == ParcelStatus.Retired && !IsRetired)
            {
                ApplyChange(new ParcelWasCorrectedToRetired(ParcelId));
            }

            AddressIds
                .Except(addressIds)
                .ToList()
                .ForEach(addressId => ApplyChange(new ParcelAddressWasDetached(ParcelId, addressId)));

            addressIds
                .Except(AddressIds)
                .ToList()
                .ForEach(addressId => ApplyChange(new ParcelAddressWasAttached(ParcelId, addressId)));
        }

        public void MarkAsMigrated()
        {
            if (IsMigrated)
            {
                return;
            }

            ApplyChange(new ParcelWasMarkedAsMigrated(ParcelId));
        }

        private void GuardRemoved(CrabModification? modification)
        {
            if (IsRemoved && modification != CrabModification.Delete)
                throw new ParcelRemovedException($"Cannot change removed parcel for parcel id {ParcelId}");
        }
    }
}
