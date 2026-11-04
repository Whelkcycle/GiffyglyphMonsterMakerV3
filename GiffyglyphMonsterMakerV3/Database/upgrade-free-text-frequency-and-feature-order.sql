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
