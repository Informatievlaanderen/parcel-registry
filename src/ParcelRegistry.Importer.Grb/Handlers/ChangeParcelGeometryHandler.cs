namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
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
        private readonly ICommandHandlerResolver _commandHandlerResolver;
        private readonly ConsumerAddressContext _addresses;

        public ChangeParcelGeometryHandler(ICommandHandlerResolver commandHandlerResolver, ConsumerAddressContext addresses)
        {
            _commandHandlerResolver = commandHandlerResolver;
            _addresses = addresses;
        }

        public async Task Handle(ChangeParcelGeometryRequest request, CancellationToken cancellationToken)
        {
            var addressesWithinParcel = _addresses
                .FindAddressesWithinGeometry(request.GrbParcel.Geometry)
                .Select(x => new AddressPersistentLocalId(x.AddressPersistentLocalId))
                .ToList();

            var extendedWkbGeometry = ExtendedWkbGeometry.CreateEWkb(request.GrbParcel.Geometry.ToBinary());

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

            await _commandHandlerResolver.Dispatch(command.CreateCommandId(), command, cancellationToken: cancellationToken);
        }
    }
}
