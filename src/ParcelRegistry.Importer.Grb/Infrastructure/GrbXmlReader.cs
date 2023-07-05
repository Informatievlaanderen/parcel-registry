namespace ParcelRegistry.Importer.Grb.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Api.BackOffice.Abstractions.Extensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.GML2;

    public sealed record GrbParcel(CaPaKey GrbCaPaKey, Geometry Geometry);

    public class GrbAddXmlReader : GrbXmlReader
    {
        public GrbAddXmlReader(string filePath) : base(filePath)
        { }

        public override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is { InnerText: "1" };
        }
    }

    public class GrbUpdateXmlReader : GrbXmlReader
    {
        public GrbUpdateXmlReader(string filePath) : base(filePath)
        { }

        public override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:VERSIE", NamespaceManager);
            return versionNode is not { InnerText: "1" };
        }
    }

    public class GrbDeleteXmlReader : GrbXmlReader
    {
        public GrbDeleteXmlReader(string filePath) : base(filePath)
        { }

        public override bool IsValid(XmlNode featureMemberNode)
        {
            var versionNode = featureMemberNode.SelectSingleNode(".//agiv:BEWERK", NamespaceManager);
            return versionNode is { InnerText: "1" };
        }
    }

    public class GrbXmlReader
    {
        private readonly string _filePath;
        private readonly GMLReader _gmlReader;
        protected XmlNamespaceManager NamespaceManager { get; private set; }

        public GrbXmlReader(string filePath)
        {
            _filePath = filePath;
            _gmlReader = GmlHelpers.CreateGmlReader();
        }

        public IEnumerable<GrbParcel> Read()
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(_filePath);

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

                if(!IsValid(featureMemberNode))
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

        public virtual bool IsValid(XmlNode featureMemberNode) => true;
    }
}
