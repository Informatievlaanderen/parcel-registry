namespace ParcelRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;

    public class CreateOsloSnapshotsRequest
    {
        public List<string> CaPaKeys { get; set; }

        public string Reden { get; set; }
    }
}
