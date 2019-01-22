namespace ParcelRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ParcelDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField perceelid => Fields[1];
        public DbaseField versie => Fields[2];
        public DbaseField status => Fields[3];

        public ParcelDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateStringField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(254)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(perceelid)), new DbaseFieldLength(30)),
            DbaseField.CreateDateTimeField(new DbaseFieldName(nameof(versie))),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(50))
        };
    }
}
