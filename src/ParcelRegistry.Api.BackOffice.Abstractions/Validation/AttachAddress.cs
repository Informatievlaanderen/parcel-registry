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
                public const string Message = "Enkel een gerealiseerd perceel kan gekoppeld worden.";

                public static TicketError ToTicketError => new(Message, Code);
            }

            public static class InvalidAddressStatus
            {
                public const string Code = "AdresAfgekeurdGehistoreerd";
                public const string Message = "Enkel een voorgesteld of adres in gebruik kan gekoppeld worden.";

                public static TicketError ToTicketError => new(Message, Code);
            }
        }
    }
}
