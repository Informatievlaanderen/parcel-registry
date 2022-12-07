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

    public class AttachAddressLambdaRequestBuilder
    {
        private readonly Fixture _fixture;

        private ParcelId? _parcelId;
        private string? _adresId;
        private Guid? _ticketId;
        private string? _ifMatchHeaderValue;

        public AttachAddressLambdaRequestBuilder(Fixture fixture)
        {
            _fixture = fixture;
        }

        public AttachAddressLambdaRequestBuilder WithParcelId(ParcelId parcelId)
        {
            _parcelId = parcelId;
            return this;
        }

        public AttachAddressLambdaRequestBuilder WithAdresId(int addressPersistentLocalId)
        {
            _adresId = PuriCreator.CreateAdresId(addressPersistentLocalId);
            return this;
        }

        public AttachAddressLambdaRequestBuilder WithTicketId(
            Guid ticketId)
        {
            _ticketId = ticketId;
            return this;
        }

        public AttachAddressLambdaRequestBuilder WithIfMatchHeaderValue(
            string ifMatchHeaderValue)
        {
            _ifMatchHeaderValue = ifMatchHeaderValue;
            return this;
        }

        public AttachAddressLambdaRequest Build()
        {
            var parcelId = _parcelId ?? _fixture.Create<ParcelId>();
            var ticketId = _ticketId ?? _fixture.Create<Guid>();
            var adresId = _adresId ?? PuriCreator.CreateAdresId(123);

            return new AttachAddressLambdaRequest(
                messageGroupId: parcelId,
                new AttachAddressSqsRequest
                {
                    ParcelId = parcelId,
                    Request = new AttachAddressRequest { AdresId = adresId },
                    TicketId = ticketId,
                    IfMatchHeaderValue = _ifMatchHeaderValue,
                    Metadata = new Dictionary<string, object?>(),
                    ProvenanceData = _fixture.Create<ProvenanceData>()
                }
            );
        }
    }
}
