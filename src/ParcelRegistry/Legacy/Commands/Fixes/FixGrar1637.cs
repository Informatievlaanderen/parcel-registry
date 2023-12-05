namespace ParcelRegistry.Legacy.Commands.Fixes
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;

    [Obsolete("This is a legacy command and should not be used anymore.")]
    public class FixGrar1637
    {
        private static readonly Guid Namespace = new Guid("ea29bd27-3b3f-4d35-a51a-67609e7dc57c");

        public ParcelId ParcelId { get; }

        public FixGrar1637(ParcelId parcelId)
        {
            ParcelId = parcelId;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"FixGrar1637-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields);

        private IEnumerable<object> IdentityFields()
        {
            yield return ParcelId;
        }
    }
}
