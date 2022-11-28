namespace ParcelRegistry.Tests.BackOffice.Validators
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Moq;
    using Parcel;
    using ParcelRegistry.Api.BackOffice.Validators;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Xunit;

    public class ParcelExistsValidatorTests
    {
        [Theory]
        [InlineData("91f3a764-7a7e-4889-bc66-96d36db6642b", true)]
        [InlineData("81a8ed9d-f844-410a-b2dd-68dc585ff645", false)]
        public async Task GivenId_ThenReturnsExpectedResult(string parcelIdAsString, bool expectedResult)
        {
            var streamStoreMock = new Mock<IStreamStore>();

            var buildingPersistentLocalId = new ParcelId(Guid.Parse(parcelIdAsString));
            var streamId = new StreamId(new ParcelStreamId(buildingPersistentLocalId));

            streamStoreMock
                .Setup(store => store.ReadStreamBackwards(streamId, StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() => expectedResult
                    ? new ReadStreamPage(streamId, PageReadStatus.Success, 1, 2, 2, 2, ReadDirection.Backward, false, messages: new []{ new StreamMessage() })
                    : new ReadStreamPage(streamId, PageReadStatus.StreamNotFound, -1, -1, -1, -1, ReadDirection.Backward, false, messages: Array.Empty<StreamMessage>()));

            var sut = new ParcelExistsValidator(streamStoreMock.Object);

            var result = await sut.Exists(buildingPersistentLocalId, CancellationToken.None);

            result.Should().Be(expectedResult);
        }
    }
}
