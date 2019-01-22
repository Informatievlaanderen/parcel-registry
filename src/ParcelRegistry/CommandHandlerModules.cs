namespace ParcelRegistry
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.CommandHandling.SqlStreamStore.Autofac;
    using Autofac;
    using Parcel;

    public static class CommandHandlerModules
    {
        public static void Register(ContainerBuilder containerBuilder)
        {
            containerBuilder
                .RegisterSqlStreamStoreCommandHandler<ParcelCommandHandlerModule>(
                    c => handler =>
                        new ParcelCommandHandlerModule(
                            c.Resolve<Func<IParcels>>(),
                            c.Resolve<Func<ConcurrentUnitOfWork>>(),
                            handler));
        }
    }
}
