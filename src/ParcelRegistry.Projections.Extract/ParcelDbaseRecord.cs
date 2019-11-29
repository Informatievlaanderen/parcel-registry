namespace ParcelRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ParcelDbaseRecord : DbaseRecord
    {
        public static readonly ParcelDbaseSchema Schema = new ParcelDbaseSchema();

        public DbaseString id { get; }
        public DbaseString perceelid { get; }
        public DbaseString versieid { get; }
        public DbaseString status { get; }

        public ParcelDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            perceelid = new DbaseString(Schema.perceelid);
            versieid = new DbaseString(Schema.versieid);
            status = new DbaseString(Schema.status);

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
