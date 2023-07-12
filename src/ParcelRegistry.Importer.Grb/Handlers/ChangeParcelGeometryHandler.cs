namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using MediatR;
    using NodaTime;
    using Parcel;
    using Parcel.Commands;

    public record ChangeParcelGeometryRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public class ChangeParcelGeometryHandler : IRequestHandler<ChangeParcelGeometryRequest>
    {
        private readonly ICommandHandlerResolver _commandHandlerResolver;

        public ChangeParcelGeometryHandler(ICommandHandlerResolver commandHandlerResolver)
        {
            _commandHandlerResolver = commandHandlerResolver;
        }

        public async Task Handle(ChangeParcelGeometryRequest request, CancellationToken cancellationToken)
        {
            var extendedWkbGeometry = ExtendedWkbGeometry.CreateEWkb(request.GrbParcel.Geometry.ToBinary());

            var command = new ChangeParcelGeometry(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey.VbrCaPaKey),
                extendedWkbGeometry,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelen Plan"),
                    new Operator("Parcel Registry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen));

            await _commandHandlerResolver.Dispatch(command.CreateCommandId(), command, cancellationToken: cancellationToken);
        }
    }
}
