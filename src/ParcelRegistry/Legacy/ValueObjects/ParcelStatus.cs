namespace ParcelRegistry.Legacy
{
    using System;
    using System.Collections.Generic;

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

    public static class ParcelStatusHelpers
    {
        private static readonly IDictionary<ParcelStatus, ParcelRegistry.Parcel.ParcelStatus> Statusses =
            new Dictionary<ParcelStatus, ParcelRegistry.Parcel.ParcelStatus>
            {
                { ParcelStatus.Realized, ParcelRegistry.Parcel.ParcelStatus.Realized },
                { ParcelStatus.Retired, ParcelRegistry.Parcel.ParcelStatus.Retired }
            };

        public static ParcelRegistry.Parcel.ParcelStatus Map(this ParcelStatus status)
        {
            return Statusses.ContainsKey(status)
                ? Statusses[status]
                : throw new ArgumentOutOfRangeException(nameof(status), status, $"Non existing status '{status}'.");
        }
    }
}
