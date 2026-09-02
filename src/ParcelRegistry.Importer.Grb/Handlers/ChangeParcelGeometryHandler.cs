namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Address;
    using Infrastructure;
    using MediatR;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;

    public sealed record ChangeParcelGeometryRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public sealed class ChangeParcelGeometryHandler : IRequestHandler<ChangeParcelGeometryRequest>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ConsumerAddressContext _addresses;
        private readonly UseLambert2008EventStoreToggle _useLambert2008EventStore;

        public ChangeParcelGeometryHandler(
            ILifetimeScope lifetimeScope,
            ConsumerAddressContext addresses,
            UseLambert2008EventStoreToggle useLambert2008EventStore)
        {
            _lifetimeScope = lifetimeScope;
            _addresses = addresses;
            _useLambert2008EventStore = useLambert2008EventStore;
        }

        public async Task Handle(ChangeParcelGeometryRequest request, CancellationToken cancellationToken)
        {
            // Deliberately the geometry as GRB delivered it: FindAddressesWithinGeometry dispatches on where
            // the coordinates are, so normalizing first would make which addresses are matched depend on the
            // parcel event store's reference system rather than on GRB's. See ADR 0004.
            var addressesWithinParcel = _addresses
                .FindAddressesWithinGeometry(request.GrbParcel.Geometry)
                .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))
                .ToList();

            // Normalized to whatever the event store holds, so a reference system GRB did not send us in
            // cannot reach the aggregate. See ADR 0005.
            var extendedWkbGeometry = ExtendedWkbGeometry.CreateEWkb(
                request.GrbParcel.Geometry.ToReferenceSystem(_useLambert2008EventStore.EventStoreSrid))!;

            var command = new ChangeParcelGeometry(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey),
                extendedWkbGeometry,
                addressesWithinParcel,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelenplan"),
                    new Operator("Parcel Registry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen));

            await using var scope = _lifetimeScope.BeginLifetimeScope();
            await scope
                .Resolve<ICommandHandlerResolver>()
                .Dispatch(command.CreateCommandId(), command, cancellationToken: cancellationToken);
        }
    }
}
