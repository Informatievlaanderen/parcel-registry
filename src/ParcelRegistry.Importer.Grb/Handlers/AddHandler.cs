namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using MediatR;

    public record GrbAddParcelRequest(GrbParcel GrbParcel) : GrbParcelRequest(GrbParcel);

    public class AddHandler : IRequestHandler<GrbAddParcelRequest, ParcelResponse>
    {
        public Task<ParcelResponse> Handle(GrbAddParcelRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
