namespace ParcelRegistry.Projections.Feed.Contract
{
    public static class ParcelEventTypes
    {
        public const string CreateV1 = "basisregisters.parcel.create.v1";
        public const string UpdateV1 = "basisregisters.parcel.update.v1";
        public const string DeleteV1 = "basisregisters.parcel.delete.v1";
    }

    public static class ParcelAttributeNames
    {
        public const string StatusName = "perceelStatus";
        public const string Geometry = "perceelGeometrie";
        public const string GeometryGmlType = "geometrieType";
    }
}
