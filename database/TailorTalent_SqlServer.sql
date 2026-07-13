/* =============================================================================
   TailorTalent (AIJobHunter) — SQL Server database script
   Creates the database, application login, all tables, indexes,
   and CRUD stored procedures for every entity.

   Matches the EF Core model in src/TailorTalent.Api/Models.
   Run with: sqlcmd -S <server> -i TailorTalent_SqlServer.sql
   ============================================================================= */

-- ============================================================
-- 1. DATABASE
-- ============================================================
IF DB_ID(N'TailorTalent') IS NULL
BEGIN
    CREATE DATABASE TailorTalent;
END
GO

USE TailorTalent;
GO

-- ============================================================
-- 2. APPLICATION LOGIN / USER (edit the password before running in prod!)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'tailortalent_app')
BEGIN
    CREATE LOGIN tailortalent_app WITH PASSWORD = N'ChangeMe!Strong#Passw0rd', CHECK_POLICY = ON;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'tailortalent_app')
BEGIN
    CREATE USER tailortalent_app FOR LOGIN tailortalent_app;
    ALTER ROLE db_datareader ADD MEMBER tailortalent_app;
    ALTER ROLE db_datawriter ADD MEMBER tailortalent_app;
    GRANT EXECUTE TO tailortalent_app;
END
GO

-- ============================================================
-- 3. TABLES
-- ============================================================

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        Id            UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY DEFAULT NEWID(),
        Email         NVARCHAR(128)    NOT NULL,
        PasswordHash  NVARCHAR(256)    NOT NULL,
        FullName      NVARCHAR(128)    NOT NULL CONSTRAINT DF_Users_FullName DEFAULT N'',
        CreatedAt     DATETIME2(7)     NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt     DATETIME2(7)     NOT NULL CONSTRAINT DF_Users_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
    CREATE UNIQUE INDEX IX_Users_Email ON dbo.Users (Email);
END
GO

IF OBJECT_ID(N'dbo.Resumes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Resumes (
        Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Resumes PRIMARY KEY DEFAULT NEWID(),
        UserId             NVARCHAR(128)    NOT NULL,
        Title              NVARCHAR(256)    NOT NULL,
        RawContent         NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_Resumes_RawContent DEFAULT N'',
        ParsedSectionsJson NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_Resumes_Parsed DEFAULT N'{}',
        CreatedAt          DATETIME2(7)     NOT NULL CONSTRAINT DF_Resumes_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt          DATETIME2(7)     NOT NULL CONSTRAINT DF_Resumes_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_Resumes_UserId ON dbo.Resumes (UserId);
    CREATE INDEX IX_Resumes_CreatedAt ON dbo.Resumes (CreatedAt);
END
GO

IF OBJECT_ID(N'dbo.JobDescriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobDescriptions (
        Id                     UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_JobDescriptions PRIMARY KEY DEFAULT NEWID(),
        UserId                 NVARCHAR(128)    NOT NULL,
        Title                  NVARCHAR(256)    NOT NULL,
        Company                NVARCHAR(256)    NOT NULL CONSTRAINT DF_JD_Company DEFAULT N'',
        RawContent             NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_JD_RawContent DEFAULT N'',
        ParsedRequirementsJson NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_JD_Parsed DEFAULT N'{}',
        CreatedAt              DATETIME2(7)     NOT NULL CONSTRAINT DF_JD_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt              DATETIME2(7)     NOT NULL CONSTRAINT DF_JD_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_JobDescriptions_UserId ON dbo.JobDescriptions (UserId);
    CREATE INDEX IX_JobDescriptions_CreatedAt ON dbo.JobDescriptions (CreatedAt);
END
GO

IF OBJECT_ID(N'dbo.TailoringSessions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TailoringSessions (
        Id               UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TailoringSessions PRIMARY KEY DEFAULT NEWID(),
        UserId           NVARCHAR(128)    NOT NULL,
        ResumeId         UNIQUEIDENTIFIER NOT NULL,
        JobDescriptionId UNIQUEIDENTIFIER NOT NULL,
        TailoredContent  NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_TS_Content DEFAULT N'',
        CoverLetter      NVARCHAR(MAX)    NOT NULL CONSTRAINT DF_TS_Cover DEFAULT N'',
        AtsScore         INT              NULL,
        Status           INT              NOT NULL CONSTRAINT DF_TS_Status DEFAULT 0, -- 0=Draft
        CreatedAt        DATETIME2(7)     NOT NULL CONSTRAINT DF_TS_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt        DATETIME2(7)     NOT NULL CONSTRAINT DF_TS_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_TailoringSessions_Resumes FOREIGN KEY (ResumeId)
            REFERENCES dbo.Resumes (Id) ON DELETE CASCADE,
        CONSTRAINT FK_TailoringSessions_JobDescriptions FOREIGN KEY (JobDescriptionId)
            REFERENCES dbo.JobDescriptions (Id) ON DELETE NO ACTION -- avoid multiple cascade paths; app deletes sessions first
    );
    CREATE INDEX IX_TailoringSessions_UserId ON dbo.TailoringSessions (UserId);
    CREATE INDEX IX_TailoringSessions_ResumeId ON dbo.TailoringSessions (ResumeId);
    CREATE INDEX IX_TailoringSessions_JobDescriptionId ON dbo.TailoringSessions (JobDescriptionId);
    CREATE INDEX IX_TailoringSessions_Status ON dbo.TailoringSessions (Status);
    CREATE INDEX IX_TailoringSessions_CreatedAt ON dbo.TailoringSessions (CreatedAt);
END
GO

IF OBJECT_ID(N'dbo.UserSubscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserSubscriptions (
        Id        UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserSubscriptions PRIMARY KEY DEFAULT NEWID(),
        UserId    NVARCHAR(128)    NOT NULL,
        [Plan]    INT              NOT NULL CONSTRAINT DF_US_Plan DEFAULT 0, -- 0=Free, 1=Premium, 2=PayPerTailor
        StartDate DATETIME2(7)     NOT NULL CONSTRAINT DF_US_Start DEFAULT SYSUTCDATETIME(),
        EndDate   DATETIME2(7)     NULL,
        IsActive  BIT              NOT NULL CONSTRAINT DF_US_Active DEFAULT 1,
        CreatedAt DATETIME2(7)     NOT NULL CONSTRAINT DF_US_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(7)     NOT NULL CONSTRAINT DF_US_UpdatedAt DEFAULT SYSUTCDATETIME()
    );
    CREATE UNIQUE INDEX IX_UserSubscriptions_UserId ON dbo.UserSubscriptions (UserId);
END
GO

IF OBJECT_ID(N'dbo.UserCredits', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.UserCredits (
        Id                    UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_UserCredits PRIMARY KEY DEFAULT NEWID(),
        UserId                NVARCHAR(128)    NOT NULL,
        CreditsRemaining      INT              NOT NULL CONSTRAINT DF_UC_Remaining DEFAULT 0,
        TotalCreditsPurchased INT              NOT NULL CONSTRAINT DF_UC_Purchased DEFAULT 0,
        CreatedAt             DATETIME2(7)     NOT NULL CONSTRAINT DF_UC_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt             DATETIME2(7)     NOT NULL CONSTRAINT DF_UC_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_UserCredits_NonNegative CHECK (CreditsRemaining >= 0)
    );
    CREATE UNIQUE INDEX IX_UserCredits_UserId ON dbo.UserCredits (UserId);
END
GO

IF OBJECT_ID(N'dbo.CreditTransactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CreditTransactions (
        Id                 UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CreditTransactions PRIMARY KEY DEFAULT NEWID(),
        UserId             NVARCHAR(128)    NOT NULL,
        Amount             INT              NOT NULL,
        Description        NVARCHAR(512)    NOT NULL CONSTRAINT DF_CT_Desc DEFAULT N'',
        TailoringSessionId UNIQUEIDENTIFIER NULL,
        CreatedAt          DATETIME2(7)     NOT NULL CONSTRAINT DF_CT_CreatedAt DEFAULT SYSUTCDATETIME()
    );
    CREATE INDEX IX_CreditTransactions_UserId ON dbo.CreditTransactions (UserId);
    CREATE INDEX IX_CreditTransactions_CreatedAt ON dbo.CreditTransactions (CreatedAt);
END
GO

-- ============================================================
-- 4. CRUD STORED PROCEDURES
-- ============================================================

-- ----------------------- Users -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_Users_Create
    @Id UNIQUEIDENTIFIER = NULL,
    @Email NVARCHAR(128),
    @PasswordHash NVARCHAR(256),
    @FullName NVARCHAR(128) = N''
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = ISNULL(@Id, NEWID());
    INSERT INTO dbo.Users (Id, Email, PasswordHash, FullName)
    VALUES (@Id, @Email, @PasswordHash, @FullName);
    SELECT * FROM dbo.Users WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Users WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_GetByEmail
    @Email NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Users WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_Update
    @Id UNIQUEIDENTIFIER,
    @Email NVARCHAR(128) = NULL,
    @PasswordHash NVARCHAR(256) = NULL,
    @FullName NVARCHAR(128) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Users SET
        Email        = COALESCE(@Email, Email),
        PasswordHash = COALESCE(@PasswordHash, PasswordHash),
        FullName     = COALESCE(@FullName, FullName),
        UpdatedAt    = SYSUTCDATETIME()
    WHERE Id = @Id;
    SELECT * FROM dbo.Users WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Users_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Users WHERE Id = @Id;
    SELECT @@ROWCOUNT AS RowsDeleted;
END
GO

-- ----------------------- Resumes -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_Resumes_Create
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(128),
    @Title NVARCHAR(256),
    @RawContent NVARCHAR(MAX) = N'',
    @ParsedSectionsJson NVARCHAR(MAX) = N'{}'
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = ISNULL(@Id, NEWID());
    INSERT INTO dbo.Resumes (Id, UserId, Title, RawContent, ParsedSectionsJson)
    VALUES (@Id, @UserId, @Title, @RawContent, @ParsedSectionsJson);
    SELECT * FROM dbo.Resumes WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Resumes_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Resumes WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Resumes_GetAllByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.Resumes WHERE UserId = @UserId ORDER BY UpdatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Resumes_Update
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(256) = NULL,
    @RawContent NVARCHAR(MAX) = NULL,
    @ParsedSectionsJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Resumes SET
        Title              = COALESCE(@Title, Title),
        RawContent         = COALESCE(@RawContent, RawContent),
        ParsedSectionsJson = COALESCE(@ParsedSectionsJson, ParsedSectionsJson),
        UpdatedAt          = SYSUTCDATETIME()
    WHERE Id = @Id;
    SELECT * FROM dbo.Resumes WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_Resumes_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.Resumes WHERE Id = @Id; -- sessions cascade
    SELECT @@ROWCOUNT AS RowsDeleted;
END
GO

-- ----------------------- JobDescriptions -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_JobDescriptions_Create
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(128),
    @Title NVARCHAR(256),
    @Company NVARCHAR(256) = N'',
    @RawContent NVARCHAR(MAX) = N'',
    @ParsedRequirementsJson NVARCHAR(MAX) = N'{}'
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = ISNULL(@Id, NEWID());
    INSERT INTO dbo.JobDescriptions (Id, UserId, Title, Company, RawContent, ParsedRequirementsJson)
    VALUES (@Id, @UserId, @Title, @Company, @RawContent, @ParsedRequirementsJson);
    SELECT * FROM dbo.JobDescriptions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_JobDescriptions_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.JobDescriptions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_JobDescriptions_GetAllByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.JobDescriptions WHERE UserId = @UserId ORDER BY UpdatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_JobDescriptions_Update
    @Id UNIQUEIDENTIFIER,
    @Title NVARCHAR(256) = NULL,
    @Company NVARCHAR(256) = NULL,
    @RawContent NVARCHAR(MAX) = NULL,
    @ParsedRequirementsJson NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.JobDescriptions SET
        Title                  = COALESCE(@Title, Title),
        Company                = COALESCE(@Company, Company),
        RawContent             = COALESCE(@RawContent, RawContent),
        ParsedRequirementsJson = COALESCE(@ParsedRequirementsJson, ParsedRequirementsJson),
        UpdatedAt              = SYSUTCDATETIME()
    WHERE Id = @Id;
    SELECT * FROM dbo.JobDescriptions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_JobDescriptions_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    -- FK is NO ACTION: remove dependent sessions first
    DELETE FROM dbo.TailoringSessions WHERE JobDescriptionId = @Id;
    DELETE FROM dbo.JobDescriptions WHERE Id = @Id;
    SELECT @@ROWCOUNT AS RowsDeleted;
END
GO

-- ----------------------- TailoringSessions -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_TailoringSessions_Create
    @Id UNIQUEIDENTIFIER = NULL,
    @UserId NVARCHAR(128),
    @ResumeId UNIQUEIDENTIFIER,
    @JobDescriptionId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET @Id = ISNULL(@Id, NEWID());
    INSERT INTO dbo.TailoringSessions (Id, UserId, ResumeId, JobDescriptionId)
    VALUES (@Id, @UserId, @ResumeId, @JobDescriptionId);
    SELECT * FROM dbo.TailoringSessions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TailoringSessions_GetById
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TailoringSessions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TailoringSessions_GetAllByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.TailoringSessions WHERE UserId = @UserId ORDER BY UpdatedAt DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TailoringSessions_Update
    @Id UNIQUEIDENTIFIER,
    @TailoredContent NVARCHAR(MAX) = NULL,
    @CoverLetter NVARCHAR(MAX) = NULL,
    @AtsScore INT = NULL,
    @Status INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.TailoringSessions SET
        TailoredContent = COALESCE(@TailoredContent, TailoredContent),
        CoverLetter     = COALESCE(@CoverLetter, CoverLetter),
        AtsScore        = COALESCE(@AtsScore, AtsScore),
        Status          = COALESCE(@Status, Status),
        UpdatedAt       = SYSUTCDATETIME()
    WHERE Id = @Id;
    SELECT * FROM dbo.TailoringSessions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TailoringSessions_Delete
    @Id UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.TailoringSessions WHERE Id = @Id;
    SELECT @@ROWCOUNT AS RowsDeleted;
END
GO

-- ----------------------- UserSubscriptions -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_UserSubscriptions_Upsert
    @UserId NVARCHAR(128),
    @Plan INT,
    @MonthsValid INT = 1
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.UserSubscriptions WHERE UserId = @UserId)
        UPDATE dbo.UserSubscriptions SET
            [Plan]    = @Plan,
            IsActive  = 1,
            StartDate = SYSUTCDATETIME(),
            EndDate   = DATEADD(MONTH, @MonthsValid, SYSUTCDATETIME()),
            UpdatedAt = SYSUTCDATETIME()
        WHERE UserId = @UserId;
    ELSE
        INSERT INTO dbo.UserSubscriptions (UserId, [Plan], EndDate)
        VALUES (@UserId, @Plan, DATEADD(MONTH, @MonthsValid, SYSUTCDATETIME()));

    SELECT * FROM dbo.UserSubscriptions WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UserSubscriptions_GetByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.UserSubscriptions WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UserSubscriptions_Deactivate
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.UserSubscriptions
    SET IsActive = 0, UpdatedAt = SYSUTCDATETIME()
    WHERE UserId = @UserId;
    SELECT @@ROWCOUNT AS RowsUpdated;
END
GO

-- ----------------------- UserCredits -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_UserCredits_Initialize
    @UserId NVARCHAR(128),
    @WelcomeCredits INT = 3
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.UserCredits WHERE UserId = @UserId)
    BEGIN
        INSERT INTO dbo.UserCredits (UserId, CreditsRemaining) VALUES (@UserId, @WelcomeCredits);
        INSERT INTO dbo.CreditTransactions (UserId, Amount, Description)
        VALUES (@UserId, @WelcomeCredits, N'Free tier welcome credits');

        IF NOT EXISTS (SELECT 1 FROM dbo.UserSubscriptions WHERE UserId = @UserId)
            INSERT INTO dbo.UserSubscriptions (UserId, [Plan]) VALUES (@UserId, 0); -- Free
    END
    SELECT * FROM dbo.UserCredits WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UserCredits_GetByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.UserCredits WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UserCredits_Add
    @UserId NVARCHAR(128),
    @Amount INT,
    @Description NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;
    IF @Amount <= 0
    BEGIN
        RAISERROR(N'Amount must be positive.', 16, 1);
        RETURN;
    END

    UPDATE dbo.UserCredits SET
        CreditsRemaining      = CreditsRemaining + @Amount,
        TotalCreditsPurchased = TotalCreditsPurchased + @Amount,
        UpdatedAt             = SYSUTCDATETIME()
    WHERE UserId = @UserId;

    IF @@ROWCOUNT = 0
    BEGIN
        EXEC dbo.usp_UserCredits_Initialize @UserId = @UserId, @WelcomeCredits = 0;
        UPDATE dbo.UserCredits SET
            CreditsRemaining      = CreditsRemaining + @Amount,
            TotalCreditsPurchased = TotalCreditsPurchased + @Amount,
            UpdatedAt             = SYSUTCDATETIME()
        WHERE UserId = @UserId;
    END

    INSERT INTO dbo.CreditTransactions (UserId, Amount, Description)
    VALUES (@UserId, @Amount, @Description);

    SELECT * FROM dbo.UserCredits WHERE UserId = @UserId;
END
GO

-- Atomic deduction: returns Success = 1 when a credit was consumed, 0 when balance was empty.
-- Premium users (active Plan = 1) succeed without consuming a credit.
CREATE OR ALTER PROCEDURE dbo.usp_UserCredits_Deduct
    @UserId NVARCHAR(128),
    @Description NVARCHAR(512),
    @TailoringSessionId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM dbo.UserSubscriptions
               WHERE UserId = @UserId AND [Plan] = 1 AND IsActive = 1)
    BEGIN
        SELECT CAST(1 AS BIT) AS Success; -- unlimited plan, nothing deducted
        RETURN;
    END

    UPDATE dbo.UserCredits SET
        CreditsRemaining = CreditsRemaining - 1,
        UpdatedAt        = SYSUTCDATETIME()
    WHERE UserId = @UserId AND CreditsRemaining > 0;

    IF @@ROWCOUNT = 1
    BEGIN
        INSERT INTO dbo.CreditTransactions (UserId, Amount, Description, TailoringSessionId)
        VALUES (@UserId, -1, @Description, @TailoringSessionId);
        SELECT CAST(1 AS BIT) AS Success;
    END
    ELSE
        SELECT CAST(0 AS BIT) AS Success;
END
GO

-- ----------------------- CreditTransactions -----------------------
CREATE OR ALTER PROCEDURE dbo.usp_CreditTransactions_Create
    @UserId NVARCHAR(128),
    @Amount INT,
    @Description NVARCHAR(512) = N'',
    @TailoringSessionId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO dbo.CreditTransactions (Id, UserId, Amount, Description, TailoringSessionId)
    VALUES (@Id, @UserId, @Amount, @Description, @TailoringSessionId);
    SELECT * FROM dbo.CreditTransactions WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreditTransactions_GetAllByUser
    @UserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM dbo.CreditTransactions WHERE UserId = @UserId ORDER BY CreatedAt DESC;
END
GO

PRINT N'TailorTalent SQL Server schema and CRUD procedures created successfully.';
GO
