/*
    Upgrade for the custom-monster-maker branch.
    Run once against the SQL Server database used by this application before launching
    the updated app. Existing structured frequency values are retained in their legacy
    columns and remain available as a display fallback for old records.
*/
IF COL_LENGTH('dbo.Features', 'FrequencyText') IS NULL
BEGIN
    ALTER TABLE dbo.Features
        ADD FrequencyText nvarchar(max) NOT NULL
            CONSTRAINT DF_Features_FrequencyText DEFAULT (N'');
END;
GO

IF COL_LENGTH('dbo.Features', 'SortOrder') IS NULL
BEGIN
    ALTER TABLE dbo.Features
        ADD SortOrder int NOT NULL
            CONSTRAINT DF_Features_SortOrder DEFAULT (0);
END;
GO

/* Creature-template storage and creature skill proficiencies.
   EF Core's inheritance mapping can name this shared table after the root
   entity (Creature), rather than the derived Monster DbSet (Monsters). */
DECLARE @CreatureTable nvarchar(256);
IF OBJECT_ID(N'dbo.Creature', N'U') IS NOT NULL
    SET @CreatureTable = N'dbo.Creature';
ELSE IF OBJECT_ID(N'dbo.Creatures', N'U') IS NOT NULL
    SET @CreatureTable = N'dbo.Creatures';
ELSE IF OBJECT_ID(N'dbo.Monster', N'U') IS NOT NULL
    SET @CreatureTable = N'dbo.Monster';
ELSE IF OBJECT_ID(N'dbo.Monsters', N'U') IS NOT NULL
    SET @CreatureTable = N'dbo.Monsters';
ELSE
    THROW 51000, 'Could not find the creature table (expected dbo.Creature, dbo.Creatures, dbo.Monster, or dbo.Monsters).', 1;

IF COL_LENGTH(@CreatureTable, 'Skills') IS NULL
BEGIN
    DECLARE @SkillsSql nvarchar(max) =
        N'ALTER TABLE ' + @CreatureTable +
        N' ADD Skills nvarchar(max) NOT NULL CONSTRAINT DF_Creature_Skills DEFAULT (N''[]'');';
    EXEC sp_executesql @SkillsSql;
END;
GO

IF OBJECT_ID('dbo.CreatureTemplates', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.CreatureTemplates
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_CreatureTemplates PRIMARY KEY,
        CreateUserId nvarchar(450) NOT NULL,
        Name nvarchar(256) NOT NULL,
        Layer int NOT NULL,
        ParentTemplateId uniqueidentifier NULL,
        PayloadJson nvarchar(max) NOT NULL CONSTRAINT DF_CreatureTemplates_Payload DEFAULT (N'{}'),
        CONSTRAINT FK_CreatureTemplates_AspNetUsers_CreateUserId
            FOREIGN KEY (CreateUserId) REFERENCES dbo.AspNetUsers(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CreatureTemplates_Parent
            FOREIGN KEY (ParentTemplateId) REFERENCES dbo.CreatureTemplates(Id)
    );
    CREATE INDEX IX_CreatureTemplates_CreateUserId ON dbo.CreatureTemplates(CreateUserId);
    CREATE INDEX IX_CreatureTemplates_ParentTemplateId ON dbo.CreatureTemplates(ParentTemplateId);
END;
GO
