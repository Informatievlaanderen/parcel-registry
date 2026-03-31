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
                                 "id": "1",
                                 "time": "2023-11-01T11:44:38.5493268+01:00",
                                 "type": "basisregisters.parcel.create.v1",
                                 "source": "{{_responseOptions.ParcelFeed.FeedUrl}}",
                                 "datacontenttype": "application/json",
                                 "dataschema": "{{_responseOptions.ParcelFeed.DataSchemaUrl}}",
                                 "basisregisterseventtype": "ParcelWasMigrated",
                                 "basisregisterscausationid": "4fe743fb-0736-5246-8df2-da07f9276c88",
                                 "data": {
                                     "@id": "https://data.vlaanderen.be/id/perceel/11001B0001-00S000",
                                     "objectId": "11001B0001-00S000",
                                     "naamruimte": "https://data.vlaanderen.be/id/perceel",
                                     "versieId": "2023-11-01T11:44:38+01:00",
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
