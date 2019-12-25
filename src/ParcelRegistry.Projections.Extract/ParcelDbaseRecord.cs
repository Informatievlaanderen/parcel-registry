namespace ParcelRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ParcelDbaseRecord : DbaseRecord
    {
        public static readonly ParcelDbaseSchema Schema = new ParcelDbaseSchema();

        public DbaseCharacter id { get; }
        public DbaseCharacter perceelid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter status { get; }

        public ParcelDbaseRecord()
        {
            id = new DbaseCharacter(Schema.id);
            perceelid = new DbaseCharacter(Schema.perceelid);
            versieid = new DbaseCharacter(Schema.versieid);
            status = new DbaseCharacter(Schema.status);

            Values = new DbaseFieldValue[]
            {
                id,
                perceelid,
                versieid,
                status
            };
        }
    }
}
