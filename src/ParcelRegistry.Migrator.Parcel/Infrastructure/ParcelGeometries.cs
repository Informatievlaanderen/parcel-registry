namespace ParcelRegistry.Migrator.Parcel.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Importer.Grb.Infrastructure;

    public sealed class ParcelGeometries
    {
        private readonly IAmazonS3 _s3Client;
        private readonly GrbXmlReader _grbXmlReader;

        public ParcelGeometries(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
            _grbXmlReader = new GrbXmlReader();
        }

        public async Task<IEnumerable<GrbParcel>> ReadParcelGeometriesFrom(string bucketName, string key)
        {
            using var response = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            });

            await using var responseStream = response.ResponseStream;

            return _grbXmlReader.Read(responseStream);
        }
    }
}
