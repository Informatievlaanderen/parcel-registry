namespace ParcelRegistry.Legacy.Commands.Fixes
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class FixGrar1475
    {
        private static readonly Guid Namespace = new Guid("e7e7caad-964e-48f9-8bf8-5e2072c15ee9");

        public ParcelId ParcelId { get; }

        public FixGrar1475(ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"FixGrar1475-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
        }
    }
}
