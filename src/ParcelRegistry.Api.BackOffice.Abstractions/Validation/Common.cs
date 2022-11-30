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

            public class ParcelNotFound
            {
                public static string Code => "PerceelIsOnbestaand";
                public static string Message => "Onbestaand perceel.";

                public static TicketError ToTicketError => new(Message, Code);
            }

            public class AddressNotFound
            {
                public static string Code => "AdresOngeldig";
                public static string Message => "Ongeldig AdresId.";
                public static TicketError ToTicketError => new(Message, Code);
            }

            public class AddressRemoved
            {
                public static string Code => "VerwijderdAddress";
                public static string Message => "Verwijderd address.";
                public static TicketError ToTicketError => new(Message, Code);
            }
        }
    }
}
