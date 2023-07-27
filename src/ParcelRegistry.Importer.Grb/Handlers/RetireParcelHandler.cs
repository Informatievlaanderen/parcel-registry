namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure;
    using MediatR;
    using NodaTime;
    using Parcel.Commands;

    public sealed record RetireParcelRequest(GrbParcel GrbParcel) : ParcelRequest(GrbParcel);

    public sealed class RetireParcelHandler : IRequestHandler<RetireParcelRequest>
    {
        private readonly ILifetimeScope _lifetimeScope;

        public RetireParcelHandler(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public async Task Handle(RetireParcelRequest request, CancellationToken cancellationToken)
        {
            var command = new RetireParcelV2(
                new VbrCaPaKey(request.GrbParcel.GrbCaPaKey),
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
