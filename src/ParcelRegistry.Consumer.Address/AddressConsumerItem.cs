namespace ParcelRegistry.Consumer.Address
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using ParcelRegistry.Infrastructure;

    public class AddressConsumerItem
    {
        public int AddressPersistentLocalId { get; set; }
        public Guid? AddressId { get; set; }
        public AddressStatus Status { get; set; }
        public bool IsRemoved { get; set; }

        //Needed for EF
        private AddressConsumerItem()
        { }

        public AddressConsumerItem(
            int addressPersistentLocalId,
            AddressStatus status)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = status;
            IsRemoved = false;
        }

        public AddressConsumerItem(
            int addressPersistentLocalId,
            Guid addressId,
            AddressStatus status,
            bool isRemoved)
            : this(addressPersistentLocalId, status)
        {
            AddressId = addressId;
            IsRemoved = isRemoved;
        }
    }

    public struct AddressStatus
    {
        public static readonly AddressStatus Proposed = new AddressStatus("Proposed");
        public static readonly AddressStatus Current = new AddressStatus("Current");
        public static readonly AddressStatus Retired = new AddressStatus("Retired");
        public static readonly AddressStatus Rejected = new AddressStatus("Rejected");

        public string Status { get; }

        private AddressStatus(string status) => Status = status;

        public static AddressStatus Parse(string status)
        {
            if (status != Proposed.Status &&
                status != Current.Status &&
                status != Retired.Status &&
                status != Rejected.Status)
            {
                throw new NotImplementedException($"Cannot parse {status} to AddressStatus");
            }

            return new AddressStatus(status);
        }

        public static implicit operator string(AddressStatus status) => status.Status;
    }

    public class AddressConsumerItemConfiguration : IEntityTypeConfiguration<AddressConsumerItem>
    {
        private const string TableName = "Addresses";

        public void Configure(EntityTypeBuilder<AddressConsumerItem> builder)
        {
            builder.ToTable(TableName, Schema.ConsumerAddress)
                .HasKey(x => x.AddressPersistentLocalId)
                .IsClustered();

            builder.Property(x => x.AddressPersistentLocalId)
                .ValueGeneratedNever();

            builder.Property(x => x.AddressId);
            builder.Property(x => x.IsRemoved);

            builder
                .Property(x => x.Status)
                .HasConversion(
                    addressStatus => addressStatus.Status,
                    status => AddressStatus.Parse(status));
            

            builder.HasIndex(x => x.AddressId);
            builder.HasIndex(x => x.IsRemoved);
        }
    }
}
