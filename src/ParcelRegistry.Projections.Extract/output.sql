BEGIN TRANSACTION;
GO

DROP TABLE [ParcelRegistryExtract].[ParcelLinks];
GO

ALTER TABLE [ParcelRegistryExtract].[ParcelLinksWithCount] DROP CONSTRAINT [PK_ParcelLinksWithCount];
GO

EXEC sp_rename N'[ParcelRegistryExtract].[ParcelLinksWithCount]', N'ParcelLinks';
GO

ALTER TABLE [ParcelRegistryExtract].[ParcelLinks] ADD CONSTRAINT [PK_ParcelLinks] PRIMARY KEY NONCLUSTERED ([ParcelId], [AddressPersistentLocalId]);
GO

EXEC sp_rename N'[ParcelRegistryExtract].[ParcelLinks].[IX_ParcelLinksWithCount_CaPaKey]', N'IX_ParcelLinks_CaPaKey', N'INDEX';
GO

EXEC sp_rename N'[ParcelRegistryExtract].[ParcelLinks].[IX_ParcelLinksWithCount_AddressPersistentLocalId]', N'IX_ParcelLinks_AddressPersistentLocalId', N'INDEX';
GO

EXEC sp_rename N'[ParcelRegistryExtract].[ParcelLinks].[IX_ParcelLinksWithCount_ParcelId]', N'IX_ParcelLinks_ParcelId', N'INDEX';
GO

ALTER TABLE [ParcelRegistryExtract].[ParcelV2] DROP CONSTRAINT [PK_ParcelV2];
GO

EXEC sp_rename N'[ParcelRegistryExtract].[ParcelV2]', N'Parcels';
GO

ALTER TABLE [ParcelRegistryExtract].[Parcels] ADD CONSTRAINT [PK_Parcels] PRIMARY KEY NONCLUSTERED ([ParcelId]);
GO

EXEC sp_rename N'[ParcelRegistryExtract].[Parcels].[IX_ParcelV2_CaPaKey]', N'IX_Parcels_CaPaKey', N'INDEX';
GO

INSERT INTO [ParcelRegistryExtract].[__EFMigrationsHistoryExtract] ([MigrationId], [ProductVersion])
VALUES (N'20240607143051_DeleteOldLinksRenameNew', N'8.0.3');
GO

COMMIT;
GO

