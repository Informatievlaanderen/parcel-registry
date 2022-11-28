namespace ParcelRegistry.Tests.BackOffice
{
    using System;
    using System.Threading;
    using AutoFixture;
    using Moq;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public static class MockStreamStoreExtensions
    {
        public static void SetStreamFound(this Mock<IStreamStore> streamStoreMock)
        {
            streamStoreMock
                .Setup(store => store.ReadStreamBackwards(It.IsAny<StreamId>(), StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() =>
                    new ReadStreamPage(new Fixture().Create<string>(), PageReadStatus.Success, 1, 2, 2, 2, ReadDirection.Backward, false, messages: new []{ new StreamMessage() }));
        }

        public static void SetStreamNotFound(this Mock<IStreamStore> streamStoreMock)
        {
            streamStoreMock
                .Setup(store => store.ReadStreamBackwards(It.IsAny<StreamId>(), StreamVersion.End, 1, false, CancellationToken.None))
                .ReturnsAsync(() =>
                    new ReadStreamPage(new Fixture().Create<string>(), PageReadStatus.StreamNotFound, -1, -1, -1, -1, ReadDirection.Backward, false, messages: Array.Empty<StreamMessage>()));
        }
    }
}
