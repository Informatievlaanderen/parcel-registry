namespace ParcelRegistry
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.AggregateSource;

    public class CaPaKey : ValueObject<CaPaKey>
    {
        private static readonly Regex OldCrabCaPaKeyFormat = new Regex("^[0-9]{5}_[A-Z]_[0-9]{4}_[A-Z_0]_[0-9]{3}_[0-9]{2}$");
        private static readonly Regex NewCrabCaPaKeyFormat = new Regex(@"^[0-9]{5}[A-Z][0-9]{4}\/[0-9]{2}[A-Z_0][0-9]{3}$");

        public string CaPaKeyCrabNotation1 { get; private set; }
        public string CaPaKeyCrabNotation2 { get; private set; }
        public VbrCaPaKey VbrCaPaKey { get; private set; }

        public static CaPaKey CreateFrom(string identifierTerrainObject)
        {
            var capakey = new CaPaKey();

            var trimmed = identifierTerrainObject.Trim();

            if (OldCrabCaPaKeyFormat.IsMatch(trimmed))
            {
                capakey.CaPaKeyCrabNotation1 = trimmed;
                var groups = capakey.CaPaKeyCrabNotation1.Split('_');
                // We could handle this but there is probably too much rubbish in crab which we cannot ignore.
                //if (groups.Length != 6)
                //    throw new ApplicationException("Wrong capakey format");

                string vbrKey;
                if (groups.Length == 6)
                    vbrKey = groups[0] + groups[1] + groups[2] + "-" + groups[5] + groups[3] + groups[4];
                else if (groups.Length == 7)
                    vbrKey = groups[0] + groups[1] + groups[2] + "-" + groups[6] + "_" + groups[5];
                else
                    throw new NotImplementedException();

                capakey.CaPaKeyCrabNotation2 = vbrKey.Replace('-', '/');
                capakey.VbrCaPaKey = new VbrCaPaKey(vbrKey);
            }
            else if (NewCrabCaPaKeyFormat.IsMatch(trimmed))
            {
                capakey.CaPaKeyCrabNotation2 = trimmed.Trim();
                capakey.CaPaKeyCrabNotation1 = capakey.CaPaKeyCrabNotation2.Substring(0, 5) + "_"
                    + capakey.CaPaKeyCrabNotation2.Substring(5, 1) + "_"
                    + capakey.CaPaKeyCrabNotation2.Substring(6, 4) + "_"
                    + capakey.CaPaKeyCrabNotation2.Substring(13, 1) + "_"
                    + capakey.CaPaKeyCrabNotation2.Substring(14, 3) + "_"
                    + capakey.CaPaKeyCrabNotation2.Substring(11, 2);

                capakey.VbrCaPaKey = new VbrCaPaKey(capakey.CaPaKeyCrabNotation2.Replace('/', '-'));
            }
            else
            {
                if (trimmed.Contains("-")) // VBR Format
                    return CreateFrom(trimmed.Replace('-', '/'));

                capakey.CaPaKeyCrabNotation1 = null;
                capakey.CaPaKeyCrabNotation2 = trimmed;
                capakey.VbrCaPaKey = new VbrCaPaKey(capakey.CaPaKeyCrabNotation2.Replace('/', '-'));
            }

            return capakey;
        }

        protected override IEnumerable<object> Reflect()
        {
            yield return CaPaKeyCrabNotation1;
            yield return CaPaKeyCrabNotation2;
            yield return VbrCaPaKey;
        }
    }
}
