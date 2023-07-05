namespace ParcelRegistry
{
    using NetTopologySuite.Geometries;

    public static class GeometryValidator
    {
        public static bool IsValid(Geometry geometry)
        {
            var validOp =
                new NetTopologySuite.Operation.Valid.IsValidOp(geometry)
                {
                    IsSelfTouchingRingFormingHoleValid = true
                };

            return validOp.IsValid;
        }
    }
}
