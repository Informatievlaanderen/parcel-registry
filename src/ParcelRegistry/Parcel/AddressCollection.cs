namespace ParcelRegistry.Parcel
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Events.Crab;

    public class AddressCollection
    {
        private readonly List<AddressId> _addressIds = new List<AddressId>();
        private readonly List<AddressSubaddressWasImportedFromCrab> _importedSubaddressFromCrabs = new List<AddressSubaddressWasImportedFromCrab>();

        public Dictionary<CrabSubaddressId, AddressSubaddressWasImportedFromCrab> LastSubaddressRecordBySubaddressId =>
            _importedSubaddressFromCrabs
                .GroupBy(x => x.SubaddressId)
                .ToDictionary(x => new CrabSubaddressId(x.Key), y => y.OrderBy(t => t.Timestamp).Last());

        public Dictionary<CrabHouseNumberId, List<AddressSubaddressWasImportedFromCrab>> LastSubaddressRecordsByHouseNumberId =>
            LastSubaddressRecordBySubaddressId
                .Values
                .GroupBy(x => x.HouseNumberId)
                .ToDictionary(x => new CrabHouseNumberId(x.Key), y => y.ToList());


        public void Add(AddressSubaddressWasImportedFromCrab @event) => _importedSubaddressFromCrabs.Add(@event);

        internal void Add(AddressId addressId) => _addressIds.Add(addressId);

        public void Remove(AddressId addressId) => _addressIds.Remove(addressId);

        public bool Contains(AddressId addressId) => _addressIds.Contains(addressId);

        public IEnumerable<AddressId> AddressIdsEligableToAddFor(CrabHouseNumberId houseNumberId)
        {
            if (LastSubaddressRecordsByHouseNumberId.ContainsKey(houseNumberId))
            {
                return LastSubaddressRecordsByHouseNumberId[houseNumberId]
                            .Where(x => x.Modification != CrabModification.Delete &&
                                !x.EndDateTime.HasValue &&
                                !_addressIds.Contains(AddressId.CreateFor(new CrabSubaddressId(x.SubaddressId))))
                            .Select(x => AddressId.CreateFor(new CrabSubaddressId(x.SubaddressId)));
            }

            return new List<AddressId>();
        }

        public IEnumerable<AddressId> AddressIdsEligableToRemoveFor(CrabHouseNumberId houseNumberId)
        {
            if (LastSubaddressRecordsByHouseNumberId.ContainsKey(houseNumberId))
            {
                return LastSubaddressRecordsByHouseNumberId[houseNumberId]
                    .Where(x => _addressIds.Contains(AddressId.CreateFor(new CrabSubaddressId(x.SubaddressId))))
                    .Select(x => AddressId.CreateFor(new CrabSubaddressId(x.SubaddressId)));
            }

            return new List<AddressId>();
        }
    }
}
