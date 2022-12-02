namespace ParcelRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using TicketingService.Abstractions;

    public class DetachAddressHandler : SqsHandler<DetachAddressSqsRequest>
    {
        public const string Action = "DetachAddress";

        public DetachAddressHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(DetachAddressSqsRequest request)
        {
            return request.ParcelId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, DetachAddressSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(ParcelRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, aggregateId }
            };
        }
    }
}
