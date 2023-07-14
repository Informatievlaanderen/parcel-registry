namespace ParcelRegistry.Importer.Grb.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public sealed record GrbParcel(CaPaKey GrbCaPaKey, Geometry Geometry, int Version);

    public sealed class GrbAddXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is { InnerText: "1" };
        }
    }

    public sealed class GrbUpdateXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is not { InnerText: "1" };
        }
    }

    public sealed class GrbDeleteXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:BEWERK", NamespaceManager);
            return versionNode is { InnerText: "1" };
        }

        public override IEnumerable<GrbParcel> Read(string filePath)
        {
            var parcels = base.Read(filePath);
            return parcels.Select(x => x with { Version = x.Version + 1 });
        }

        public override IEnumerable<GrbParcel> Read(Stream fileStream)
        {
            var parcels = base.Read(fileStream);
            return parcels.Select(x => x with { Version = x.Version + 1 });
        }
    }

    public class GrbXmlReader
    {
        private readonly GMLReader _gmlReader;
        protected XmlNamespaceManager NamespaceManager { get; private set; }

        public GrbXmlReader()
        {
            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        public virtual IEnumerable<GrbParcel> Read(string filePath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            return GetParcelsFromXml(xmlDoc);
        }

        public virtual IEnumerable<GrbParcel> Read(Stream fileStream)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(fileStream);

            return GetParcelsFromXml(xmlDoc);
        }

        private IEnumerable<GrbParcel> GetParcelsFromXml(XmlDocument xmlDoc)
        {
            NamespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            NamespaceManager.AddNamespace("agiv", "http://www.agiv.be/agiv");
            NamespaceManager.AddNamespace("gml", "http://www.opengis.net/gml");

            var features = xmlDoc.SelectNodes("//gml:featureMember", NamespaceManager);

            foreach (XmlNode featureMemberNode in features)
            {
                // Process each featureMemberNode
                var caPaKeyNode = featureMemberNode.SelectSingleNode(".//agiv:CAPAKEY", NamespaceManager);
                var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
                var polygonNode = featureMemberNode.SelectSingleNode(".//gml:polygonProperty", NamespaceManager);
                var multiPolygonNode = featureMemberNode.SelectSingleNode(".//gml:multiPolygonProperty", NamespaceManager);

                if (!IsValid(featureMemberNode))
                    continue;

                if (polygonNode != null)
                {
                    yield return new GrbParcel(CaPaKey.CreateFrom(caPaKeyNode.InnerText), _gmlReader.Read(polygonNode.InnerXml), Convert.ToInt32(versionNode.InnerText));
                }
                else if (multiPolygonNode != null)
                {
                    yield return new GrbParcel(CaPaKey.CreateFrom(caPaKeyNode.InnerText), _gmlReader.Read(multiPolygonNode.InnerXml), Convert.ToInt32(versionNode.InnerText));
                }
                else
                {
                    throw new InvalidOperationException("Cannot create parcel from XML without a polygon or multipolygon.");
                }
            }
        }

        protected virtual bool IsValid(XmlNode featureMemberNode) => true;
    }
}
