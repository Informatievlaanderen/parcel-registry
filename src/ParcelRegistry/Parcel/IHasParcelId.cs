namespace ParcelRegistry.Parcel
{
    using System;

    public interface IHasParcelId
    {
        public Guid ParcelId { get; }
    }
}
