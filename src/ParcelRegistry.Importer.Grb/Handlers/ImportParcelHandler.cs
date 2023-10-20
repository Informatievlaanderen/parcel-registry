namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Address;
    using Infrastructure;
    using MediatR;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;

    public sealed record ImportParcelRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public sealed class ImportParcelHandler : IRequestHandler<ImportParcelRequest>
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly ConsumerAddressContext _addresses;

        public ImportParcelHandler(ILifetimeScope lifetimeScope, ConsumerAddressContext addresses)
        {
            _lifetimeScope = lifetimeScope;
            _addresses = addresses;
        }

        public async Task Handle(ImportParcelRequest request, CancellationToken cancellationToken)
        {
            var addressesWithinParcel = _addresses
                .FindAddressesWithinGeometry(request.GrbParcel.Geometry)
                .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))
                .ToList();

            var extendedWkbGeometry = ExtendedWkbGeometry.CreateEWkb(request.GrbParcel.Geometry.ToBinary())!;

            var command = new ImportParcel(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey),
                extendedWkbGeometry,
                addressesWithinParcel,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelenplan"),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen));

            await using var scope = _lifetimeScope.BeginLifetimeScope();
            await scope
                .Resolve<ICommandHandlerResolver>()
                .Dispatch(command.CreateCommandId(), command, cancellationToken: cancellationToken);
        }
    }
}
