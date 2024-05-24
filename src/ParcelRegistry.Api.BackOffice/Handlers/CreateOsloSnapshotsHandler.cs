namespace ParcelRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using AllStream;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public sealed class CreateOsloSnapshotsHandler : SqsHandler<CreateOsloSnapshotsSqsRequest>
    {
        public const string Action = "CreateOsloSnapshots";

        public CreateOsloSnapshotsHandler(
            ISqsQueue sqsQueue,
            ITicketing ticketing,
            ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(CreateOsloSnapshotsSqsRequest request)
        {
            return AllStreamId.Instance;
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, CreateOsloSnapshotsSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(ParcelRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId }
            };
        }
    }
}
