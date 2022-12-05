namespace ParcelRegistry.Api.Legacy.Parcel.Responses
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Convertors;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Query;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class ParcelSyndicationResponse
    {
        public static async Task WriteParcel(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category,
            ParcelSyndicationQueryResult parcel)
        {
            var item = new SyndicationItem
            {
                Id = parcel.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{parcel.ChangeType}-{parcel.Position}",
                Published = parcel.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = parcel.LastChangedOn.ToBelgianDateTimeOffset(),
                Description = BuildDescription(parcel, responseOptions.Value.Naamruimte)
            };

            item.AddLink(
                new SyndicationLink(
                    new Uri($"{responseOptions.Value.Naamruimte}/{parcel.CaPaKey}"),
                    AtomLinkTypes.Related));

            //item.AddLink(
            //    new SyndicationLink(
            //        new Uri(string.Format(responseOptions.Value.DetailUrl, parcel.CaPaKey)),
            //        AtomLinkTypes.Self));

            //item.AddLink(
            //    new SyndicationLink(
            //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", parcel.CaPaKey)),
            //            AtomLinkTypes.Alternate)
            //    { MediaType = MediaTypeNames.Application.Xml });

            //item.AddLink(
            //    new SyndicationLink(
            //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", parcel.CaPaKey)),
            //            AtomLinkTypes.Alternate)
            //    { MediaType = MediaTypeNames.Application.Json });

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    parcel.Organisation == null ? Organisation.Unknown.ToName() : parcel.Organisation.Value.ToName(),
                    string.Empty,
                    AtomContributorTypes.Author));

            await writer.Write(item);
        }

        private static string BuildDescription(ParcelSyndicationQueryResult parcel, string naamruimte)
        {
            if (!parcel.ContainsEvent && !parcel.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();
            if(parcel.ContainsObject)
                content.Object = new ParcelSyndicationContent(
                    parcel.ParcelId,
                    naamruimte,
                    parcel.CaPaKey,
                    parcel.LastChangedOn.ToBelgianDateTimeOffset(),
                    parcel.Status.MapToPerceelStatusSyndication(),
                    parcel.AddressIds,
                    parcel.XCoordinate,
                    parcel.YCoordinate,
                    parcel.Organisation,
                    parcel.Reason);

            if (parcel.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(parcel.EventDataAsXml);
                content.Event = doc.DocumentElement;
            }

            return content.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public ParcelSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "Perceel", Namespace = "")]
    public class ParcelSyndicationContent
    {
        /// <summary>
        /// De technische id van het perceel.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public Guid ParcelId { get; set; }

        /// <summary>
        /// De identificator van het perceel.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public PerceelIdentificator Identificator { get; set; }

        /// <summary>
        /// De status van het perceel.
        /// </summary>
        [DataMember(Name = "PerceelStatus", Order = 3)]
        public PerceelStatus? Status { get; set; }

        /// <summary>
        /// De aan het perceel gelinkte adressen
        /// </summary>
        [DataMember(Name = "AdressenIds", Order = 4)]
        public List<string> AddressIds { get; set; }

        /// <summary>
        /// Het x-coordinaat van de centroïde van het perceel
        /// </summary>
        [DataMember(Name = "XCoordinaat", Order = 5)]
        public decimal? XCoordinate { get; set; }

        /// <summary>
        /// Het y-coordinaat van de centroïde van het perceel
        /// </summary>
        [DataMember(Name = "YCoordinaat", Order = 6)]
        public decimal? YCoordinate { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 7)]
        public Provenance Provenance { get; set; }

        public ParcelSyndicationContent(
            Guid parcelId,
            string naamruimte,
            string caPaKey,
            DateTimeOffset version,
            PerceelStatus? status,
            IEnumerable<string> addressIds,
            decimal? xCoordinate,
            decimal? yCoordinate,
            Organisation? organisation,
            string reason)
        {
            ParcelId = parcelId;
            Identificator = new PerceelIdentificator(naamruimte, string.IsNullOrEmpty(caPaKey) ? string.Empty : caPaKey, version);
            Status = status;
            AddressIds = addressIds.ToList();
            XCoordinate = xCoordinate;
            YCoordinate = yCoordinate;

            Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    public class ParcelSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        private const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v1/feeds/percelen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'percelen'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'percelen'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.3.16.3"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-11-06T19:36:06Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/percelen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/percelen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/percelen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v1/feeds/percelen?from=2&amp;limit=100&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>ParcelWasRegistered-0</title>
        <updated>2012-09-23T23:13:50+02:00</updated>
        <published>2012-09-23T23:13:50+02:00</published>
        <link href=""https://data.vlaanderen.be/id/perceel/0"" rel=""related"" />
        <author>
            <name>Gemeente</name>
        </author>
        <category term=""percelen"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><ParcelWasRegistered><ParcelId>10ad670a-ab6e-5fda-9a8a-733e11f59902</ParcelId><VbrCaPaKey>0</VbrCaPaKey><Provenance><Timestamp>2012-09-23T21:13:50Z</Timestamp><Organisation>Municipality</Organisation><Reason>Decentrale bijhouding CRAB</Reason></Provenance>
    </ParcelWasRegistered>
  </Event><Object><Id>10ad670a-ab6e-5fda-9a8a-733e11f59902</Id><Identificator><Id>https://data.vlaanderen.be/id/perceel/0</Id><Naamruimte>https://data.vlaanderen.be/id/perceel</Naamruimte><ObjectId>0</ObjectId><VersieId>2012-09-23T23:13:50+02:00</VersieId></Identificator><PerceelStatus>Gerealiseerd</PerceelStatus><AdressenIds xmlns:d3p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" /><Creatie><Tijdstip>2012-09-23T23:13:50+02:00</Tijdstip><Organisatie>Gemeente</Organisatie><Reden>Decentrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>1</id>
    <title>ParcelWasRealized-1</title>
    <updated>2012-09-23T23:13:50+02:00</updated>
    <published>2012-09-23T23:13:50+02:00</published>
    <link href=""https://data.vlaanderen.be/id/perceel/0"" rel=""related"" />
    <author>
        <name>Gemeente</name>
    </author>
    <category term=""percelen"" />
    <content>
        <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><ParcelWasRealized><ParcelId>10ad670a-ab6e-5fda-9a8a-733e11f59902</ParcelId><Provenance><Timestamp>2012-09-23T21:13:50Z</Timestamp><Organisation>Municipality</Organisation><Reason>Decentrale bijhouding CRAB</Reason></Provenance>
    </ParcelWasRealized>
  </Event><Object><Id>10ad670a-ab6e-5fda-9a8a-733e11f59902</Id><Identificator><Id>https://data.vlaanderen.be/id/perceel/0</Id><Naamruimte>https://data.vlaanderen.be/id/perceel</Naamruimte><ObjectId>0</ObjectId><VersieId>2012-09-23T23:13:50+02:00</VersieId></Identificator><PerceelStatus>Gerealiseerd</PerceelStatus><AdressenIds xmlns:d3p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" /><Creatie><Tijdstip>2012-09-23T23:13:50+02:00</Tijdstip><Organisatie>Gemeente</Organisatie><Reden>Decentrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
</feed>";

        public XmlElement GetExamples()
        {
            var example = new XmlDocument();
            example.LoadXml(RawXml);
            return example.DocumentElement;
        }
    }
}
