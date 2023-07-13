namespace ParcelRegistry.Importer.Grb
{
    using System;

    public class ImportGrbException : Exception
    {
        public ImportGrbException(string message, Exception exception)
            : base(message, exception)
        { }
    }
}
