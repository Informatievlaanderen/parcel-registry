namespace ParcelRegistry.Consumer.Address
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using NetTopologySuite.Geometries;
    using ParcelRegistry.Infrastructure;

    public sealed class AddressConsumerItem
    {
        public int AddressPersistentLocalId { get; set; }
        public Guid? AddressId { get; set; }
        public AddressStatus Status { get; set; }
        public string GeometryMethod { get; set; }
        public string GeometrySpecification { get; set; }
        public Point Position { get; set; }
        public bool IsRemoved { get; set; }

        //Needed for EF
        private AddressConsumerItem()
        {
        }

        public AddressConsumerItem(
            int addressPersistentLocalId,
            AddressStatus status,
            string geometryMethod,
            string geometrySpecification,
            Point position)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            Status = status;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Position = position;
            IsRemoved = false;
        }

        public AddressConsumerItem(
            int addressPersistentLocalId,
            Guid addressId,
            AddressStatus status,
            bool isRemoved,
            string geometryMethod,
            string geometrySpecification,
            Point position)
            : this(addressPersistentLocalId, status, geometryMethod, geometrySpecification, position)
        {
            AddressPersistentLocalId = addressPersistentLocalId;
            AddressId = addressId;
            Status = status;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Position = position;
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

    public sealed class AddressConsumerItemConfiguration : IEntityTypeConfiguration<AddressConsumerItem>
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
            builder.Property(x => x.GeometryMethod);
            builder.Property(x => x.GeometrySpecification);
            builder.Property(x => x.Position).HasColumnType("sys.geometry");
            builder.Property(x => x.IsRemoved);

            builder
                .Property(x => x.Status)
                .HasConversion(
                    addressStatus => addressStatus.Status,
                    status => AddressStatus.Parse(status));

            builder.HasIndex(x => x.AddressId);
            builder.HasIndex(x => x.Position);
            builder.HasIndex(x => x.IsRemoved);
        }
    }
}
