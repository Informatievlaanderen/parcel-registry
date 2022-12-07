namespace ParcelRegistry.Consumer.Address.Infrastructure
{
    using FeatureToggle;

    public class EnableCommandHandlingConsumerToggle : IFeatureToggle
    {
        public bool FeatureEnabled { get; }

        public EnableCommandHandlingConsumerToggle(bool featureEnabled)
        {
            FeatureEnabled = featureEnabled;
        }
    }

    public class FeatureToggleOptions
    {
        public const string ConfigurationKey = "FeatureToggles";
        public bool EnableCommandHandlingConsumer { get; set; }
    }
}
