namespace ParcelRegistry.Importer.Grb.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public sealed record GrbParcel(CaPaKey GrbCaPaKey, Geometry Geometry);

    public class GrbAddXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is { InnerText: "1" };
        }
    }

    public class GrbUpdateXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is not { InnerText: "1" };
        }
    }

    public class GrbDeleteXmlReader : GrbXmlReader
    {
        protected override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:BEWERK", NamespaceManager);
            return versionNode is { InnerText: "1" };
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

        public IEnumerable<GrbParcel> Read(string filePath)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);

            return GetParcelsFromXml(xmlDoc);
        }

        public IEnumerable<GrbParcel> Read(Stream fileStream)
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
                var polygonNode = featureMemberNode.SelectSingleNode(".//gml:polygonProperty", NamespaceManager);
                var multiPolygonNode = featureMemberNode.SelectSingleNode(".//gml:multiPolygonProperty", NamespaceManager);

                if (!IsValid(featureMemberNode))
                    continue;

                if (polygonNode != null)
                {
                    yield return new GrbParcel(CaPaKey.CreateFrom(caPaKeyNode.InnerText), _gmlReader.Read(polygonNode.InnerXml));
                }
                else if (multiPolygonNode != null)
                {
                    yield return new GrbParcel(CaPaKey.CreateFrom(caPaKeyNode.InnerText), _gmlReader.Read(multiPolygonNode.InnerXml));
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
