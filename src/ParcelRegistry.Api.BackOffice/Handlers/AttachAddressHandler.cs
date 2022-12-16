namespace ParcelRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Parcel;
    using TicketingService.Abstractions;

    public class AttachAddressHandler : SqsHandler<AttachAddressSqsRequest>
    {
        public const string Action = "AttachAddressParcel";

        public AttachAddressHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(AttachAddressSqsRequest request)
        {
            return ParcelId.CreateFor(new VbrCaPaKey(request.VbrCaPaKey));
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, AttachAddressSqsRequest sqsRequest)
        {
            return new Dictionary<string, string>
            {
                { RegistryKey, nameof(ParcelRegistry) },
                { ActionKey, Action },
                { AggregateIdKey, aggregateId },
                { ObjectIdKey, sqsRequest.VbrCaPaKey }
            };
        }
    }
}
