namespace ParcelRegistry.Api.BackOffice.Validators
{
    using System.Threading;
    using System.Threading.Tasks;
    using Parcel;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class ParcelExistsValidator
    {
        private readonly IStreamStore _streamStore;

        public ParcelExistsValidator(IStreamStore streamStore)
        {
            _streamStore = streamStore;
        }

        public async Task<bool> Exists(ParcelId parcelId, CancellationToken cancellationToken)
        {
            var page = await _streamStore.ReadStreamBackwards(
                new StreamId(new ParcelStreamId(parcelId)),
                StreamVersion.End,
                maxCount: 1,
                prefetchJsonData: false,
                cancellationToken);

            return page.Status == PageReadStatus.Success;
        }
    }
}
