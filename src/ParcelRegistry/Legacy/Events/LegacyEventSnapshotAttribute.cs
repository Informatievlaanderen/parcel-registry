using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

namespace ParcelRegistry.Legacy.Events
{
    public class LegacyEventSnapshotAttribute : Attribute
    {
        public string EventName { get; }
        public Type SnapshotType { get; }

        public LegacyEventSnapshotAttribute(string eventName, Type snapshotType)
        {
            EventName = eventName;
            SnapshotType = snapshotType;
        }
    }
}
