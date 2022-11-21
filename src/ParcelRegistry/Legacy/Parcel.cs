namespace ParcelRegistry.Legacy
{
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events;
    using Events.Crab;
    using Exceptions;

    public partial class Parcel : AggregateRootEntity, ISnapshotable
    {
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
                ApplyChange(new ParcelWasRecovered(_parcelId));
            else if (IsRemoved)
                throw new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}");

            if (modification == CrabModification.Delete)
            {
                ApplyChange(new ParcelWasRemoved(_parcelId));
            }
            else
            {
                if (lifetime.EndDateTime.HasValue && !IsRetired)
                {
                    if (modification == CrabModification.Correction)
                        ApplyChange(new ParcelWasCorrectedToRetired(_parcelId));
                    else
                        ApplyChange(new ParcelWasRetired(_parcelId));
                }
                else if (!lifetime.EndDateTime.HasValue && !IsRealized)
                {
                    if (modification == CrabModification.Correction)
                        ApplyChange(new ParcelWasCorrectedToRealized(_parcelId));
                    else
                        ApplyChange(new ParcelWasRealized(_parcelId));
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
                        ApplyChange(new ParcelAddressWasDetached(_parcelId, addressIdToRemove));

                    ApplyChange(new ParcelAddressWasDetached(_parcelId, addressId));
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
                            ApplyChange(new ParcelAddressWasDetached(_parcelId, addressIdToRemove));

                        ApplyChange(new ParcelAddressWasDetached(_parcelId,
                            AddressId.CreateFor(currentCrabHouseNumberId)));
                    }
                }

                if (!_addressCollection.Contains(addressId) &&
                    modification != CrabModification.Delete &&
                    !lifetime.EndDateTime.HasValue)
                {
                    ApplyChange(new ParcelAddressWasAttached(_parcelId, addressId));

                    foreach (var addressIdToAdd in _addressCollection.AddressIdsEligableToAddFor(houseNumberId))
                        ApplyChange(new ParcelAddressWasAttached(_parcelId, addressIdToAdd));
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
                ApplyChange(new ParcelAddressWasAttached(_parcelId, addressId));
            }
            else if (_addressCollection.Contains(addressId) &&
                     (modification == CrabModification.Delete || lifetime.EndDateTime.HasValue))
            {
                ApplyChange(new ParcelAddressWasDetached(_parcelId, addressId));
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
                ApplyChange(new ParcelAddressWasDetached(_parcelId, addressId));

            ApplyChange(new ParcelWasRemoved(_parcelId));
        }

        /// <summary>
        /// Fixes issue where parcel should've been recovered but hasn't due to change applied later.
        /// </summary>
        public void FixGrar1637()
        {
            if (!IsRemoved)
                return;

            ApplyChange(new ParcelWasRecovered(_parcelId));
            ApplyChange(new ParcelWasRealized(_parcelId));
        }

        public void MarkAsMigrated()
        {
            if (IsMigrated)
            {
                return;
            }

            ApplyChange(new ParcelWasMarkedAsMigrated(_parcelId));
        }

        private void GuardRemoved(CrabModification? modification)
        {
            if (IsRemoved && modification != CrabModification.Delete)
                throw new ParcelRemovedException($"Cannot change removed parcel for parcel id {_parcelId}");
        }
    }
}
