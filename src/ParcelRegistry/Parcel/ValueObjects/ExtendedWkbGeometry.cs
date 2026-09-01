namespace ParcelRegistry.Parcel
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;

    public sealed class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        public const int SridLambert72 = SystemReferenceId.SridLambert72;

        private static readonly WKBWriter WkbWriter = new WKBWriter() { Strict = false, HandleSRID = true };

        [JsonConstructor]
        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        /// <summary>
        /// Wraps a geometry that has already been read and transformed, keeping the SRID it carries. The
        /// EWKB writer lives here, so this is the only place that decides how a geometry is serialized.
        /// </summary>
        public static ExtendedWkbGeometry Create(Geometry geometry)
            => new ExtendedWkbGeometry(WkbWriter.Write(geometry));

        public static ExtendedWkbGeometry? CreateEWkb(Geometry geometry)
        {
            if (geometry.SRID <= 0)
                return null;

            return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
        }
    }
}
