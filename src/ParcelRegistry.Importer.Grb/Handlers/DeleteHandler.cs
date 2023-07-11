namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using MediatR;

    public record GrbDeleteParcelRequest(GrbParcel GrbParcel) : GrbParcelRequest(GrbParcel);

    public class DeleteHandler : IRequestHandler<GrbDeleteParcelRequest, ParcelResponse>
    {
        public Task<ParcelResponse> Handle(GrbDeleteParcelRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
