namespace ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using System;

    public interface IHasParcelId
    {
        public Guid ParcelId { get; }
    }
}
