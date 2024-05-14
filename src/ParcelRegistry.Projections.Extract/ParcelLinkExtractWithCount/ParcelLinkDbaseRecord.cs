namespace ParcelRegistry.Projections.Extract.ParcelLinkExtractWithCount
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public sealed class ParcelLinkDbaseRecord : DbaseRecord
    {
        public static readonly ParcelLinkDbaseSchema Schema = new ParcelLinkDbaseSchema();

        public DbaseCharacter objecttype { get; }
        public DbaseCharacter adresobjid { get; }
        public DbaseInt32 adresid { get; }

        public ParcelLinkDbaseRecord()
        {
            objecttype = new DbaseCharacter(Schema.objecttype);
            adresobjid = new DbaseCharacter(Schema.adresobjid);
            adresid = new DbaseInt32(Schema.adresid);

            Values = new DbaseFieldValue[]
            {
                objecttype,
                adresobjid,
                adresid
            };
        }
    }
}
