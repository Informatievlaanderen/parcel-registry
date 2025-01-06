namespace ParcelRegistry.Api.Oslo.Parcel.Sync
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Swashbuckle.AspNetCore.Filters;
    using Polygon = NetTopologySuite.Geometries.Polygon;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class ParcelSyndicationResponse
    {
        private static readonly WKBReader WkbReader = WKBReaderFactory.Create();

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
            {
                var geometry = parcel.ExtendedWkbGeometry is null
                    ? null
                    : WkbReader.Read(parcel.ExtendedWkbGeometry);

                content.Object = new ParcelSyndicationContent(
                    parcel.ParcelId,
                    naamruimte,
                    parcel.CaPaKey,
                    parcel.LastChangedOn.ToBelgianDateTimeOffset(),
                    parcel.Status.MapToPerceelStatusSyndication(),
                    parcel.AddressIds,
                    geometry is MultiPolygon
                        ? GmlMultiSurfaceBuilder.Build(parcel.ExtendedWkbGeometry!, WkbReader)
                        : null,
                    geometry is Polygon
                        ? PolygonBuilder.Build(parcel.ExtendedWkbGeometry!, WkbReader)?.XmlPolygon
                        : null,
                    parcel.Organisation,
                    parcel.Reason);
            }

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
        /// De geometrie multi-polygoon van het perceel.
        /// </summary>
        [DataMember(Name = "GeometrieMultiPolygoon", Order = 5)]
        public SyndicationMultiSurface MultiSurfacePolygon  { get; set; }

        /// <summary>
        /// De geometrie polygoon van het perceel.
        /// </summary>
        [DataMember(Name = "GeometriePolygoon", Order = 6)]
        public SyndicationPolygon Polygon  { get; set; }

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
            GmlMultiSurface? gmlMultiSurface,
            GmlPolygon? gmlPolygon,
            Organisation? organisation,
            string reason)
        {
            ParcelId = parcelId;
            Identificator = new PerceelIdentificator(naamruimte, string.IsNullOrEmpty(caPaKey) ? string.Empty : caPaKey, version);
            Status = status;
            AddressIds = addressIds.ToList();
            if (gmlMultiSurface != null)
            {
                MultiSurfacePolygon = new SyndicationMultiSurface { XmlMultiSurface = gmlMultiSurface };
            }

            if (gmlPolygon != null)
            {
                Polygon = new SyndicationPolygon { XmlPolygon = gmlPolygon };
            }

            Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    public class ParcelSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        private const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v2/feeds/percelen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'percelen'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'percelen'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.3.16.3"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-11-06T19:36:06Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/percelen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/percelen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/percelen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/percelen?from=2&amp;limit=100&amp;embed=event,object"" rel=""next"" />
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
<entry>
  <id>121611749</id>
  <title>ParcelWasMigrated-121611749</title>
  <updated>2023-11-02T07:37:09+01:00</updated>
  <published>2023-11-02T07:37:09+01:00</published>
  <link href=""https://data.vlaanderen.be/id/perceel/31021A0112-00_000"" rel=""related"" />
  <author>
    <name>Digitaal Vlaanderen</name>
  </author>
  <category term=""percelen"" />
  <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
    <Event>
      <ParcelWasMigrated>
        <OldParcelId>24fc60e4-a00b-536e-8619-a7858a18a96c</OldParcelId>
        <ParcelId>63394bf5-28c3-51c2-81bd-91601d074448</ParcelId>
        <CaPaKey>31021A0112-00_000</CaPaKey>
        <ParcelStatus>Realized</ParcelStatus>
        <IsRemoved>false</IsRemoved>
        <AddressPersistentLocalIds />
        <ExtendedWkbGeometry>0103000000010000001800000000B22ADBAD58F240705B265E509B09410174DCD07A56F240F050600E7D9D09410034AE26A555F2407C52B177299F0941FE098E391555F240009FABF33A9F0941027215F93B54F2408A9BFF9D4F9F0941006C0E457753F240066E89A25A9F094100024BE36D53F2400A317B1F599F0941023C4E065D53F24080658914459F094101B2F6AC6153F24074F82F561E9F094100C20F033353F24007FBE12DF29E0941FE61B6990053F240FE2626B5CA9E0941FEE33E6ED552F24086142C1B889E094100D22C05BA52F240718F757E4C9E0941FF276E117E52F240FD691A210E9E0941022A3FA5F051F240704F8DA4BF9D0941029CFD7DBC51F24002F59B96A29D0941FEDFA02B5754F240737796BBC19A09410104F7716A54F2408CDA4EF6C19A094101DC69D90055F240102BD435AD9A0941FDE974004A57F2400C009C795C9A09410124251D4B57F240F1AB49525C9A094100609F523B59F240909378DB179A094102D4D56F3C5AF240F3A4E261F499094100B22ADBAD58F240705B265E509B0941</ExtendedWkbGeometry>
        <Provenance>
          <Timestamp>2023-11-02T06:37:09Z</Timestamp>
          <Organisation>DigitaalVlaanderen</Organisation>
          <Reason>Migrate Parcel aggregate.</Reason>
        </Provenance>
      </ParcelWasMigrated>
    </Event>
    <Object>
      <Id>63394bf5-28c3-51c2-81bd-91601d074448</Id>
      <Identificator>
        <Id>https://data.vlaanderen.be/id/perceel/31021A0112-00_000</Id>
        <Naamruimte>https://data.vlaanderen.be/id/perceel</Naamruimte>
        <ObjectId>31021A0112-00_000</ObjectId>
        <VersieId>2023-11-02T07:37:09+01:00</VersieId>
      </Identificator>
      <PerceelStatus>Gerealiseerd</PerceelStatus>
      <AdressenIds xmlns:d3p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" />
      <GeometrieMultiPolygoon i:nil=""true"" />
      <GeometriePolygoon>
        <polygon>
          <exterior>
            <LinearRing>
              <posList>75146.86600751430 209770.04597159801 75111.67599149050 209839.63201964600 75098.32194347680 209893.18344368401 75089.32655147460 209895.36897968501 75075.74831146750 209897.95214768901 75063.45435945690 209899.32936368900 75062.86799145490 209899.14037168800 75061.81403945389 209896.63502768800 75062.10472745450 209891.79208368401 75059.18824744970 209886.27240367999 75056.03752744940 209881.33845167601 75053.33941544589 209873.01326767000 75051.62626344711 209865.56174766601 75047.87925544380 209857.76616366199 75039.04034344110 209847.95534765301 75035.78075943890 209844.32353965199 75077.44815146920 209752.21659558601 75078.65282346310 209752.24526758899 75088.05307947101 209749.65128358500 75124.62511149789 209739.55937957799 75124.69461549820 209739.54017958001 75155.70767152309 209730.98216357501 75171.77730353180 209726.54779557101 75146.86600751430 209770.04597159801</posList>
            </LinearRing>
          </exterior>
        </polygon>
      </GeometriePolygoon>
      <Creatie>
        <Tijdstip>2023-11-02T07:37:09+01:00</Tijdstip>
        <Organisatie>Digitaal Vlaanderen</Organisatie>
        <Reden>Migrate Parcel aggregate.</Reden>
      </Creatie>
    </Object>
  </Content>]]></content>
</entry>
<entry>
  <id>133679733</id>
  <title>ParcelWasImported-133679733</title>
  <updated>2023-12-22T04:04:02+01:00</updated>
  <published>2023-12-22T04:04:02+01:00</published>
  <link href=""https://data.vlaanderen.be/id/perceel/41039B0425-00D002"" rel=""related"" />
  <author>
    <name>Digitaal Vlaanderen</name>
  </author>
  <category term=""percelen"" />
  <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
    <Event>
      <ParcelWasImported>
        <ParcelId>f507ab6b-36b6-5008-aef9-c7ab15d8a329</ParcelId>
        <CaPaKey>41039B0425-00D002</CaPaKey>
        <ExtendedWkbGeometry>01030000208A7A0000010000000C000000F09AF772DF6100410C02423F03140641FE48F5D70361004101BAB30316150641F55F24EE01610041F5CDB96818150641090EA4C5016100410A27ED9A18150641FBBF97FCDF600041093D8E7303150641F6E18B309D6000418BAF48A0D91406410C64E0BA7A600041F8CFA50CC414064106950C3AC1600041F4FDF55E6C1406410007999E1061004106A67A8608140641086444DC5461004179C9F50AB313064100CE7900636100410E8C2154A1130641F09AF772DF6100410C02423F03140641</ExtendedWkbGeometry>
        <Provenance>
          <Timestamp>2023-12-22T03:04:02Z</Timestamp>
          <Organisation>DigitaalVlaanderen</Organisation>
          <Reason>Uniek Percelenplan</Reason>
        </Provenance>
      </ParcelWasImported>
    </Event>
    <Object>
      <Id>f507ab6b-36b6-5008-aef9-c7ab15d8a329</Id>
      <Identificator>
        <Id>https://data.vlaanderen.be/id/perceel/41039B0425-00D002</Id>
        <Naamruimte>https://data.vlaanderen.be/id/perceel</Naamruimte>
        <ObjectId>41039B0425-00D002</ObjectId>
        <VersieId>2023-12-22T04:04:02+01:00</VersieId>
      </Identificator>
      <PerceelStatus>Gerealiseerd</PerceelStatus>
      <AdressenIds xmlns:d3p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays"" />
      <GeometrieMultiPolygoon i:nil=""true"" />
      <GeometriePolygoon>
        <polygon>
          <exterior>
            <LinearRing>
              <posList>134203.93113633199 180864.40588761901 134176.48044831300 180898.75180764499 134176.24128031699 180899.05113564400 134176.22150431600 180899.07564764499 134171.99833631501 180896.43142364200 134163.64870430500 180891.20326363700 134159.34124830400 180888.50617563701 134168.15334431099 180877.54636763001 134178.07744031399 180865.06566362100 134186.60755232000 180854.38035161400 134188.37523232400 180852.16607961099 134203.93113633199 180864.40588761901</posList>
            </LinearRing>
          </exterior>
        </polygon>
      </GeometriePolygoon>
      <Creatie>
        <Tijdstip>2023-12-22T04:04:02+01:00</Tijdstip>
        <Organisatie>Digitaal Vlaanderen</Organisatie>
        <Reden>Uniek Percelenplan</Reden>
      </Creatie>
    </Object>
  </Content>]]></content>
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
