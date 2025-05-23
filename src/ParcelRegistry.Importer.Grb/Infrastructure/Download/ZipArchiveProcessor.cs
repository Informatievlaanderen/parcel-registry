﻿namespace ParcelRegistry.Importer.Grb.Infrastructure.Download
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    public interface IZipArchiveProcessor
    {
        public Dictionary<GrbParcelActions, Stream> Open(ZipArchive zipArchive);
    }

    public sealed class ZipArchiveProcessor : IZipArchiveProcessor
    {
        public Dictionary<GrbParcelActions, Stream> Open(ZipArchive zipArchive)
        {
            var filesByAction = new Dictionary<GrbParcelActions, Stream>();
            var adpAddEntry = zipArchive.GetEntry("GML/AdpAdd.gml")
                              ?? zipArchive.GetEntry("GML\\AdpAdd.gml")
                              ?? throw new InvalidOperationException("AdpAdd.gml not found in zip archive.");

            var adpDelEntry = zipArchive.GetEntry("GML/AdpDel.gml")
                              ?? zipArchive.GetEntry("GML\\AdpDel.gml")
                              ?? throw new InvalidOperationException("AdpDel.gml not found in zip archive.");

            filesByAction.Add(GrbParcelActions.Add, adpAddEntry.Open());
            filesByAction.Add(GrbParcelActions.Update, adpAddEntry.Open());
            filesByAction.Add(GrbParcelActions.Delete, adpDelEntry.Open());

            return filesByAction;
        }
    }

    public enum GrbParcelActions
    {
        Add,
        Update,
        Delete
    }
}
