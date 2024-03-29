namespace ParcelRegistry.Infrastructure.Modules
{
    using Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Microsoft.Extensions.Configuration;

    public class EditModule : Module
    {
        private readonly IConfiguration _configuration;

        public EditModule(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterModule(new CommandHandlingModule(_configuration))
                .RegisterSnapshotModule(_configuration);
        }
    }
}
