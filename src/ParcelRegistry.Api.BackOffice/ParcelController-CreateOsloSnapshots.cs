namespace ParcelRegistry.Api.BackOffice
{
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    public partial class ParcelController
    {
        /// <summary>
        /// Creëer nieuwe OSLO snapshots.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/oslosnapshots")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
        public async Task<IActionResult> CreateOsloSnapshots(
            [FromBody] CreateOsloSnapshotsRequest request,
            CancellationToken cancellationToken = default)
        {
            var provenance = _provenanceFactory.Create(new Reason(request.Reden), Modification.Unknown);

            var sqsRequest = new CreateOsloSnapshotsSqsRequest
            {
                Request = request,
                Metadata = GetMetadata(),
                ProvenanceData = new ProvenanceData(provenance)
            };

            var sqsResult = await _mediator.Send(sqsRequest, cancellationToken);

            return Accepted(sqsResult);
        }
    }
}
