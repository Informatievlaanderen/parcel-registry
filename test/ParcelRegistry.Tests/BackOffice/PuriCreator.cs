using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParcelRegistry.Tests.BackOffice
{
    public static class PuriCreator
    {
        public static string CreateAdresId(int persistentLocalId) => $"https://data.vlaanderen.be/id/adressen/{persistentLocalId}";
    }
}
