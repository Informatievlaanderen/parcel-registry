﻿namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public sealed class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        public const int SridLambert72 = 31370;

        private static readonly WKBReader WkbReader = WKBReaderFactory.Create();

        [JsonConstructor]
        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        public static ExtendedWkbGeometry? CreateEWkb(byte[]? wkb)
        {
            if (wkb == null)
            {
                return null;
            }
            try
            {
                var geometry = WkbReader.Read(wkb);
                geometry.SRID = SridLambert72;
                var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
                return new ExtendedWkbGeometry(wkbWriter.Write(geometry));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        public static ExtendedWkbGeometry CreateDummyForRetiredParcel()
        {
            var geometry = new WKTReader(new NtsGeometryServices(
                    new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY),
                    new PrecisionModel(PrecisionModels.Floating),
                    SridLambert72))
                .Read("POLYGON ((0 0,0 1,1 1,1 0,0 0))");

            return CreateEWkb(geometry.AsBinary())!;
        }
    }
}
