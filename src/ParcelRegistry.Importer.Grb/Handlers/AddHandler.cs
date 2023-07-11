namespace ParcelRegistry.Importer.Grb.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public record AddParcel() : IRequest<AddParcelResponse>;

    public record AddParcelResponse();

    public class AddHandler : IRequestHandler<AddParcel, AddParcelResponse>
    {
        public Task<AddParcelResponse> Handle(AddParcel request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
