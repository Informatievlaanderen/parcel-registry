namespace ParcelRegistry.AllStream.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public sealed class CreateOsloSnapshots : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("ed0ab2d7-6fd2-4cef-95fa-089f494d34cb");

        public IReadOnlyList<VbrCaPaKey> CaPaKeys { get; }

        public Provenance Provenance { get; }

        public CreateOsloSnapshots(
            IEnumerable<VbrCaPaKey> caPaKeys,
            Provenance provenance)
        {
            CaPaKeys = caPaKeys.ToList();
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CreateOsloSnapshots-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return CaPaKeys;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
