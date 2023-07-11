namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using MediatR;

    public record GrbUpdateParcelRequest(GrbParcel GrbParcel) : GrbParcelRequest(GrbParcel);

    public class UpdateHandler : IRequestHandler<GrbUpdateParcelRequest, ParcelResponse>
    {
        public Task<ParcelResponse> Handle(GrbUpdateParcelRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
