namespace ParcelRegistry.Tests.BackOffice.Builders
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Abstractions.Requests;
    using ParcelRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using ParcelRegistry.Api.BackOffice.Handlers.Lambda.Requests;

    public class DetachAddressLambdaRequestBuilder
    {
        private readonly Fixture _fixture;

        private ParcelId? _parcelId;
        private AddressPersistentLocalId? _addressPersistentLocalId;
        private Guid? _ticketId;
        private string? _ifMatchHeaderValue;

        public DetachAddressLambdaRequestBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public DetachAddressLambdaRequestBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;
            return this;
        }

        public DetachAddressLambdaRequestBuilder WithAddressPersistentLocalId(
            AddressPersistentLocalId addressPersistentLocalId)
        {
            _addressPersistentLocalId = addressPersistentLocalId;
            return this;
        }

        public DetachAddressLambdaRequestBuilder WithTicketId(
            Guid ticketId)
        {
            _ticketId = ticketId;
            return this;
        }

        public DetachAddressLambdaRequestBuilder WithIfMatchHeaderValue(
            string ifMatchHeaderValue)
        {
            _ifMatchHeaderValue = ifMatchHeaderValue;
            return this;
        }

        public DetachAddressLambdaRequest Build()
        {
            var parcelId = _parcelId ?? _fixture.Create<ParcelId>();
            var addressPersistentLocalId = _addressPersistentLocalId ?? _fixture.Create<AddressPersistentLocalId>();
            var ticketId = _ticketId ?? _fixture.Create<Guid>();

            return new DetachAddressLambdaRequest(
                messageGroupId: parcelId,
                new DetachAddressSqsRequest
                {
                    ParcelId = parcelId,
                    Request = new DetachAddressRequest { AddressPersistentLocalId = addressPersistentLocalId },
                    TicketId = ticketId,
                    IfMatchHeaderValue = _ifMatchHeaderValue,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = _fixture.Create<ProvenanceData>()
                }
            );
        }
    }
}
