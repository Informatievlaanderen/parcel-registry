namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Newtonsoft.Json;

    public sealed class VbrCaPaKey : StringValueObject<VbrCaPaKey>
    {
        private static readonly Guid Namespace = new Guid("28068113-774a-4f6f-8264-9ddb39095bd4");

        public VbrCaPaKey([JsonProperty("value")] string vbrCaPaKey) : base(vbrCaPaKey) { }

        public VbrCaPaKey([JsonProperty("value")] CaPaKey caPaKey) : base(caPaKey.VbrCaPaKey) { }

        public CaPaKey ToCaPaKey() => CaPaKey.CreateFrom(Value);

        public Guid CreateDeterministicId()
            => Deterministic.Create(Namespace, $"VbrCaPaKey-{ToString()}");
    }
}
