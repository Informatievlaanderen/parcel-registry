namespace ParcelRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Requests;
    using TicketingService.Abstractions;

    public class AttachAddressHandler : SqsHandler<AttachAddressSqsRequest>
    {
        public const string Action = "AttachAddress";

        public AttachAddressHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(AttachAddressSqsRequest request)
        {
            return request.ParcelId.ToString();
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, AttachAddressSqsRequest sqsRequest)
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
