namespace ParcelRegistry.Api.BackOffice.Handlers
{
    using System.Collections.Generic;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Sqs;
    using Be.Vlaanderen.Basisregisters.Sqs.Handlers;
    using Parcel;
    using TicketingService.Abstractions;

    public sealed class DetachAddressHandler : SqsHandler<DetachAddressSqsRequest>
    {
        public const string Action = "DetachAddressParcel";

        public DetachAddressHandler(ISqsQueue sqsQueue, ITicketing ticketing, ITicketingUrl ticketingUrl) : base(sqsQueue, ticketing, ticketingUrl)
        { }

        protected override string? WithAggregateId(DetachAddressSqsRequest request)
        {
            return ParcelId.CreateFor(new VbrCaPaKey(request.VbrCaPaKey));
        }

        protected override IDictionary<string, string> WithTicketMetadata(string aggregateId, DetachAddressSqsRequest sqsRequest)
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
