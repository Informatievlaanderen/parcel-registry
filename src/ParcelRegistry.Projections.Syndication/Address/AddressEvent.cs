namespace ParcelRegistry.Projections.Syndication.Address
{
    public enum AddressEvent
    {
        AddressWasRegistered,

        AddressBecameComplete,
        AddressBecameIncomplete,

        AddressBecameCurrent,
        AddressBecameNotOfficiallyAssigned,
        AddressOfficialAssignmentWasRemoved,
        AddressStatusWasCorrectedToRemoved,
        AddressStatusWasRemoved,
        AddressWasCorrectedToCurrent,
        AddressWasCorrectedToNotOfficiallyAssigned,
        AddressWasCorrectedToOfficiallyAssigned,
        AddressWasCorrectedToProposed,
        AddressWasCorrectedToRetired,
        AddressWasOfficiallyAssigned,
        AddressWasProposed,
        AddressWasRetired,

        AddressBoxNumberWasChanged,
        AddressBoxNumberWasCorrected,
        AddressBoxNumberWasRemoved,

        AddressHouseNumberWasChanged,
        AddressHouseNumberWasCorrected,

        AddressPositionWasCorrected,
        AddressPositionWasRemoved,
        AddressWasPositioned,

        AddressPostalCodeWasChanged,
        AddressPostalCodeWasCorrected,
        AddressPostalCodeWasRemoved,

        AddressStreetNameWasChanged,
        AddressStreetNameWasCorrected,

        AddressWasRemoved,
        AddressPersistentLocalIdentifierWasAssigned,
    }
}
