namespace ParcelRegistry.Parcel
{
    using System;

    public struct ParcelStatus
    {
        public static readonly ParcelStatus Realized = new ParcelStatus("Realized");
        public static readonly ParcelStatus Retired = new ParcelStatus("Retired");

        public string Status { get; }

        private ParcelStatus(string status) => Status = status;

        public static ParcelStatus Parse(string status)
        {
            if (status != Realized.Status && status != Retired.Status)
                throw new NotImplementedException($"Cannot parse {status} to ParcelStatus");

            return new ParcelStatus(status);
        }

        public static implicit operator string(ParcelStatus status) => status.Status;
    }
}
