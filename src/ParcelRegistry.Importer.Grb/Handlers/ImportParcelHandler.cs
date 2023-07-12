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

    public record ImportParcelRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public class ImportParcelHandler : IRequestHandler<ImportParcelRequest>
    {
        private readonly ICommandHandlerResolver _commandHandlerResolver;

        public ImportParcelHandler(ICommandHandlerResolver commandHandlerResolver)
        {
            _commandHandlerResolver = commandHandlerResolver;
        }

        public async Task Handle(ImportParcelRequest request, CancellationToken cancellationToken)
        {
            var extendedWkbGeometry = ExtendedWkbGeometry.CreateEWkb(request.GrbParcel.Geometry.ToBinary());

            var command = new ImportParcel(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey.VbrCaPaKey),
                extendedWkbGeometry,
                new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.ParcelRegistry,
                    new Reason("Uniek Percelen Plan"),
                    new Operator("Parcel Registry"),
                    Modification.Insert,
                    Organisation.DigitaalVlaanderen));

            await _commandHandlerResolver.Dispatch(command.CreateCommandId(), command, cancellationToken: cancellationToken);
        }
    }
}
