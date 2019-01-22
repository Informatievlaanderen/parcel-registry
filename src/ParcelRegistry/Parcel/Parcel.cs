namespace ParcelRegistry.Parcel
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events;
    using Events.Crab;
    using System;

    public partial class Parcel : AggregateRootEntity
    {
        public static readonly Func<Parcel> Factory = () => new Parcel();

        public static Parcel Register(ParcelId id, VbrCaPaKey vbrCaPaKey)
        {
            var parcel = Factory();
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
            var addressId = AddressId.CreateFor(houseNumberId);
            if (!_addressCollection.Contains(addressId) &&
                modification != CrabModification.Delete &&
                !lifetime.EndDateTime.HasValue)
            {
                ApplyChange(new ParcelAddressWasAttached(_parcelId, addressId));

                foreach (var addressIdToAdd in _addressCollection.AddressIdsEligableToAddFor(houseNumberId))
                    ApplyChange(new ParcelAddressWasAttached(_parcelId, addressIdToAdd));
            }
            else if (_addressCollection.Contains(addressId) &&
                     (modification == CrabModification.Delete || lifetime.EndDateTime.HasValue))
            {
                foreach (var addressIdToAdd in _addressCollection.AddressIdsEligableToRemoveFor(houseNumberId))
                    ApplyChange(new ParcelAddressWasDetached(_parcelId, addressIdToAdd));

                ApplyChange(new ParcelAddressWasDetached(_parcelId, addressId));
            }

            ApplyChange(new TerrainObjectHouseNumberWasImportedFromCrab(
                terrainObjectHouseNumberId,
                terrainObjectId,
                houseNumberId,
                lifetime,
                timestamp,
                @operator,
                modification,
                organisation));
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
    }
}
