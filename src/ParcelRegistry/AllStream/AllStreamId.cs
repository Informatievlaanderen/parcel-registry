namespace ParcelRegistry.AllStream
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public sealed class AllStreamId : ValueObject<AllStreamId>
    {
        public static readonly AllStreamId Instance = new();

        private readonly Guid _id = new("e8ceb8e6-4cc6-4c15-bcef-7792b3dbc547");

        private AllStreamId() { }

        protected override IEnumerable<object> Reflect()
        {
            yield return _id;
        }

        public override string ToString() => _id.ToString("D");
    }
}
