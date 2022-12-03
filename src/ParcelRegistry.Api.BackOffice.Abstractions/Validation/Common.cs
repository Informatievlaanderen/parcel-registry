namespace ParcelRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class ParcelRemoved
            {
                private static string Code => "VerwijderdPerceel";
                private static string Message => "Verwijderd perceel.";

                public static TicketError ToTicketError => new(Message, Code);
            }

            public static class ParcelNotFound
            {
                public static string Code => "OnbestaandPerceel";
                public static string Message => "Onbestaand perceel.";

                public static TicketError ToTicketError => new(Message, Code);
            }

            public static class AddressNotFound
            {
                public static string Code => "AdresOngeldig";
                public static string Message => "Ongeldig AdresId.";
                public static TicketError ToTicketError => new(Message, Code);
            }

            public static class AddressRemoved
            {
                public static string Code => "VerwijderdAdres";
                public static string Message => "Verwijderd adres.";
                public static TicketError ToTicketError => new(Message, Code);
            }
        }
    }
}
