IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE TABLE [Companies] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Industry] nvarchar(100) NULL,
        [Website] nvarchar(500) NULL,
        [Location] nvarchar(200) NULL,
        [Notes] nvarchar(2000) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Companies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE TABLE [Applications] (
        [Id] int NOT NULL IDENTITY,
        [CompanyId] int NOT NULL,
        [RoleTitle] nvarchar(200) NOT NULL,
        [JobUrl] nvarchar(1000) NULL,
        [Source] nvarchar(100) NULL,
        [Location] nvarchar(200) NULL,
        [SalaryMin] decimal(12,2) NULL,
        [SalaryMax] decimal(12,2) NULL,
        [Notes] nvarchar(4000) NULL,
        [Status] nvarchar(20) NOT NULL,
        [AppliedDate] date NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [UpdatedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_Applications] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Applications_Companies_CompanyId] FOREIGN KEY ([CompanyId]) REFERENCES [Companies] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE TABLE [Interviews] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationId] int NOT NULL,
        [Stage] nvarchar(20) NOT NULL,
        [ScheduledAt] datetimeoffset NOT NULL,
        [InterviewerName] nvarchar(200) NULL,
        [Outcome] nvarchar(20) NOT NULL,
        [Notes] nvarchar(4000) NULL,
        CONSTRAINT [PK_Interviews] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Interviews_Applications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Applications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE TABLE [StatusEvents] (
        [Id] int NOT NULL IDENTITY,
        [ApplicationId] int NOT NULL,
        [FromStatus] nvarchar(20) NULL,
        [ToStatus] nvarchar(20) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        [Note] nvarchar(1000) NULL,
        CONSTRAINT [PK_StatusEvents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_StatusEvents_Applications_ApplicationId] FOREIGN KEY ([ApplicationId]) REFERENCES [Applications] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Applications_AppliedDate] ON [Applications] ([AppliedDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Applications_CompanyId] ON [Applications] ([CompanyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Applications_Status] ON [Applications] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Companies_Name] ON [Companies] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Interviews_ApplicationId] ON [Interviews] ([ApplicationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Interviews_ScheduledAt] ON [Interviews] ([ScheduledAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusEvents_ApplicationId] ON [StatusEvents] ([ApplicationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_StatusEvents_ChangedAt] ON [StatusEvents] ([ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260916211536_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260916211536_InitialCreate', N'10.0.12');
END;

COMMIT;
GO

