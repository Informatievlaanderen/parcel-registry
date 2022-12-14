using TicketingService.Abstractions;

namespace ParcelRegistry.Api.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class AttachAddress
        {
            public static class InvalidParcelStatus
            {
                public const string Code = "PerceelGehistoreerd";
                public const string Message = "Deze actie is enkel toegestaan op percelen met status 'gerealiseerd'.";

                public static TicketError ToTicketError => new(Message, Code);
            }

            public static class InvalidAddressStatus
            {
                public const string Code = "PerceelAdresAfgekeurdOfGehistoreerd";
                public const string Message = "Het adres is afgekeurd of gehistoreerd.";

                public static TicketError ToTicketError => new(Message, Code);
            }
        }
    }
}
