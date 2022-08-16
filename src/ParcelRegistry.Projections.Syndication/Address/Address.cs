namespace ParcelRegistry.Projections.Syndication.Address
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using System;
    using System.Runtime.Serialization;

    [DataContract(Name = "Adres", Namespace = "")]
    public class Address
    {
        /// <summary>
        /// De technische id van het adres.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string AddressId { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De id van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 3)]
        public string? SteetnameId { get; set; }

        /// <summary>
        /// De id van de postinfo.
        /// </summary>
        [DataMember(Name = "PostCode", Order = 4)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 5)]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Het nummer van de bus.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 6)]
        public string BoxNumber { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 7)]
        public AdresStatus? AdressStatus { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }
    }
}
