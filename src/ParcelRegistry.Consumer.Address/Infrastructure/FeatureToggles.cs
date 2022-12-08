namespace ParcelRegistry.Consumer.Address.Infrastructure
{
    using FeatureToggle;

    public sealed class EnableCommandHandlingConsumerToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public EnableCommandHandlingConsumerToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }

    public sealed class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool EnableCommandHandlingConsumer { get; set; }
    }
}
