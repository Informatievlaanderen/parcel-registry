namespace ParcelRegistry.Api.Oslo.Parcel.ChangeFeed
{
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Filters;

    public sealed class ParcelFeedResultExample : IExamplesProvider<object>
    {
        private readonly ResponseOptions _responseOptions;

        public ParcelFeedResultExample(IOptions<ResponseOptions> responseOptions)
        {
            _responseOptions = responseOptions.Value;
        }

        public object GetExamples()
        {
            var json = $$"""
                         [
                            {
                                "specversion": "1.0",
                                 "id": "2",
                                 "time": "2023-11-02T07:37:09.2309729+01:00",
                                 "type": "basisregisters.parcel.create.v1",
                                 "source": "{{_responseOptions.ParcelFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_responseOptions.ParcelFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "ParcelWasMigrated",
                                 "basisregisterscausationid": "0870f9b0-bba0-5444-9f76-4316e9f8cc0f",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/perceel/34034B0003-00_000",
                                     "objectId": "34034B0003-00_000",
                                     "naamruimte": "https://data.vlaanderen.be/id/perceel",
                                     "versieId": "2023-11-02T07:37:09+01:00",
                                     "attributen": [
                                         {
                                             "naam": "perceelStatus",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": "gerealiseerd"
                                         },
                                         {
                                             "naam": "adresIds",
                                             "oudeWaarde": null,
                                             "nieuweWaarde": [
                                                 "https://data.vlaanderen.be/id/adres/200001"
                                             ]
                                         }
                                     ]
                                 }
                             }
                         ]
                         """;
            return JArray.Parse(json);
        }
    }
}
