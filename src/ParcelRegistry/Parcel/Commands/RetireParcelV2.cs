namespace ParcelRegistry.Parcel.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using ParcelRegistry.Parcel;

    public sealed class RetireParcelV2 : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("d178e5b3-ea7b-4d99-8751-5d3e9415dcc6");

        public ParcelId ParcelId { get; }

        public VbrCaPaKey CaPaKey { get; }

        public Provenance Provenance { get; }

        public RetireParcelV2(VbrCaPaKey caPaKey, Provenance provenance)
        {
            CaPaKey = caPaKey;
            ParcelId = ParcelId.CreateFor(caPaKey);
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"RetireParcelV2-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
            yield return CaPaKey;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
