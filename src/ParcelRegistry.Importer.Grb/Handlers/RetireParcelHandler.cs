namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using MediatR;
    using NodaTime;
    using Parcel.Commands;

    public record RetireParcelRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public class RetireParcelHandler : IRequestHandler<RetireParcelRequest>
    {
        private readonly ICommandHandlerResolver _commandHandlerResolver;

        public RetireParcelHandler(ICommandHandlerResolver commandHandlerResolver)
        {
            _commandHandlerResolver = commandHandlerResolver;
        }

        public async Task Handle(RetireParcelRequest request, CancellationToken cancellationToken)
        {
            var command = new RetireParcelV2(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey.VbrCaPaKey),
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
