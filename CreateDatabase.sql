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
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Registry] (
        [registry_id] int NOT NULL IDENTITY,
        [jag_id] nvarchar(20) NOT NULL,
        [first_name] nvarchar(50) NOT NULL,
        [last_name] nvarchar(50) NOT NULL,
        [graduation_year] int NULL,
        [degree_program] nvarchar(100) NULL,
        [email_on_record] nvarchar(150) NULL,
        [account_created] bit NOT NULL,
        CONSTRAINT [PK__Alumni_R__EF8E9CE8B1C6B2D8] PRIMARY KEY ([registry_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [JagId] nvarchar(20) NOT NULL,
        [created_at] datetime NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Degree_Programs] (
        [degree_id] int NOT NULL IDENTITY,
        [institution] nvarchar(150) NOT NULL,
        [degree_type] nvarchar(20) NOT NULL,
        [major_field_of_study] nvarchar(100) NOT NULL,
        [department] nvarchar(100) NOT NULL,
        CONSTRAINT [PK__Degree_P__A1AFAEBBB780871C] PRIMARY KEY ([degree_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Employers] (
        [employer_id] int NOT NULL IDENTITY,
        [employer_name] nvarchar(150) NOT NULL,
        [location] nvarchar(150) NULL,
        [industry] nvarchar(100) NULL,
        CONSTRAINT [PK__Employer__365FA4E7DF9F3065] PRIMARY KEY ([employer_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Organization_Types] (
        [organization_type_id] int NOT NULL IDENTITY,
        [organization_name] nvarchar(150) NOT NULL,
        CONSTRAINT [PK__Organiza__466C7A244B987C0F] PRIMARY KEY ([organization_type_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni] (
        [alumni_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(450) NULL,
        [jag_id] nvarchar(20) NOT NULL,
        [prefix] nvarchar(10) NULL,
        [first_name] nvarchar(50) NOT NULL,
        [preferred_first_name] nvarchar(50) NULL,
        [last_name] nvarchar(50) NOT NULL,
        [gender] nvarchar(20) NULL,
        [date_of_birth] date NULL,
        [student_email] nvarchar(150) NULL,
        [permanent_email] nvarchar(150) NOT NULL,
        [phone] nvarchar(20) NULL,
        [address] nvarchar(255) NULL,
        [city] nvarchar(100) NULL,
        [state] nvarchar(50) NULL,
        [postcode] nvarchar(20) NULL,
        [country] nvarchar(50) NULL,
        [graduation_year] int NOT NULL,
        [solicitation_code] nvarchar(20) NULL,
        [social_media_account] nvarchar(255) NULL,
        [privacy] bit NOT NULL DEFAULT CAST(1 AS bit),
        [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
        [last_updated] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Alumni__BB1DF35C3C7BFE94] PRIMARY KEY ([alumni_id]),
        CONSTRAINT [FK_Alumni_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Messages] (
        [message_id] int NOT NULL IDENTITY,
        [title] nvarchar(150) NOT NULL,
        [message_body] nvarchar(max) NOT NULL,
        [message_type] nvarchar(50) NOT NULL,
        [created_by] nvarchar(450) NULL,
        [created_at] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Messages__0BBF6EE63BAB61CB] PRIMARY KEY ([message_id]),
        CONSTRAINT [FK_Messages_AspNetUsers_created_by] FOREIGN KEY ([created_by]) REFERENCES [AspNetUsers] ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Degrees] (
        [alumni_degree_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [degree_id] int NOT NULL,
        [date_conferred] date NOT NULL,
        [years_to_complete_degree] int NULL,
        [gpa] decimal(3,2) NULL,
        [employment_while_studying] nvarchar(50) NULL,
        [degree_specific_job] bit NULL,
        [participated_in_research] bit NULL,
        [job_secured_upon_graduation] bit NULL,
        [attended_or_plans_grad_school] bit NULL,
        CONSTRAINT [PK__Alumni_D__F0FA85104CA70CF5] PRIMARY KEY ([alumni_degree_id]),
        CONSTRAINT [fk_ad_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
        CONSTRAINT [fk_ad_degree] FOREIGN KEY ([degree_id]) REFERENCES [Degree_Programs] ([degree_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Employment] (
        [alumni_employment_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [employer_id] int NOT NULL,
        [job_title] nvarchar(100) NOT NULL,
        [start_date] date NOT NULL,
        [end_date] date NULL,
        [salary_range] nvarchar(50) NULL,
        CONSTRAINT [PK__Alumni_E__22E422E8BDA6C801] PRIMARY KEY ([alumni_employment_id]),
        CONSTRAINT [fk_ae_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
        CONSTRAINT [fk_ae_employer] FOREIGN KEY ([employer_id]) REFERENCES [Employers] ([employer_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Internships] (
        [alumni_internship_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [employer_id] int NOT NULL,
        [internship_type] nvarchar(50) NOT NULL,
        [title] nvarchar(100) NOT NULL,
        [start_date] date NOT NULL,
        [end_date] date NOT NULL,
        CONSTRAINT [PK__Alumni_I__FFE040B307B79B52] PRIMARY KEY ([alumni_internship_id]),
        CONSTRAINT [fk_ai_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
        CONSTRAINT [fk_ai_employer] FOREIGN KEY ([employer_id]) REFERENCES [Employers] ([employer_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Organizations] (
        [alumni_organization_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [organization_type_id] int NOT NULL,
        [officer_roles] nvarchar(150) NULL,
        CONSTRAINT [PK__Alumni_O__8366569D1933C852] PRIMARY KEY ([alumni_organization_id]),
        CONSTRAINT [fk_ao_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
        CONSTRAINT [fk_ao_org] FOREIGN KEY ([organization_type_id]) REFERENCES [Organization_Types] ([organization_type_id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE TABLE [Alumni_Messages] (
        [alumni_message_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [message_id] int NOT NULL,
        [sent_at] datetime NOT NULL DEFAULT ((getdate())),
        CONSTRAINT [PK__Alumni_M__E6B241004CC7DEC9] PRIMARY KEY ([alumni_message_id]),
        CONSTRAINT [fk_am_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
        CONSTRAINT [fk_am_message] FOREIGN KEY ([message_id]) REFERENCES [Messages] ([message_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Alumni_jag_id] ON [Alumni] ([jag_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Alumni_user_id] ON [Alumni] ([user_id]) WHERE [user_id] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__Alumni__FBF400ED6AB71E0A] ON [Alumni] ([jag_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Degrees_alumni_id] ON [Alumni_Degrees] ([alumni_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Degrees_degree_id] ON [Alumni_Degrees] ([degree_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Employment_alumni_id] ON [Alumni_Employment] ([alumni_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Employment_employer_id] ON [Alumni_Employment] ([employer_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Internships_alumni_id] ON [Alumni_Internships] ([alumni_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Internships_employer_id] ON [Alumni_Internships] ([employer_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Messages_alumni_id] ON [Alumni_Messages] ([alumni_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Messages_message_id] ON [Alumni_Messages] ([message_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Organizations_alumni_id] ON [Alumni_Organizations] ([alumni_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Alumni_Organizations_organization_type_id] ON [Alumni_Organizations] ([organization_type_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [UQ__Alumni_R__FBF400ED39BA228D] ON [Alumni_Registry] ([jag_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Messages_created_by] ON [Messages] ([created_by]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260213234903_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260213234903_InitialCreate', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260215003554_ChangeSolicitationCodeToBool'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni]') AND [c].[name] = N'solicitation_code');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [Alumni] DROP CONSTRAINT [' + @var + '];');
    EXEC(N'UPDATE [Alumni] SET [solicitation_code] = CAST(0 AS bit) WHERE [solicitation_code] IS NULL');
    ALTER TABLE [Alumni] ALTER COLUMN [solicitation_code] bit NOT NULL;
    ALTER TABLE [Alumni] ADD DEFAULT CAST(0 AS bit) FOR [solicitation_code];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260215003554_ChangeSolicitationCodeToBool'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260215003554_ChangeSolicitationCodeToBool', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260215010000_AddIsFirstLoginToAppUser'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [is_first_login] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260215010000_AddIsFirstLoginToAppUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260215010000_AddIsFirstLoginToAppUser', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218014729_AddedDecorations'
)
BEGIN
    CREATE TABLE [Audit_Logs] (
        [audit_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(450) NOT NULL,
        [user_name] nvarchar(256) NOT NULL,
        [action] nvarchar(50) NOT NULL,
        [entity_name] nvarchar(100) NOT NULL,
        [entity_id] nvarchar(50) NULL,
        [details] nvarchar(max) NULL,
        [ip_address] nvarchar(45) NULL,
        [timestamp] datetime NOT NULL,
        CONSTRAINT [PK_Audit_Logs] PRIMARY KEY ([audit_id]),
        CONSTRAINT [FK_Audit_Logs_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218014729_AddedDecorations'
)
BEGIN
    CREATE INDEX [IX_Audit_Logs_user_id] ON [Audit_Logs] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260218014729_AddedDecorations'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260218014729_AddedDecorations', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    ALTER TABLE [Alumni] DROP CONSTRAINT [FK_Alumni_AspNetUsers_user_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DROP TABLE [Audit_Logs];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DROP INDEX [IX_Alumni_user_id] ON [Alumni];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni_Registry]') AND [c].[name] = N'degree_program');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Alumni_Registry] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Alumni_Registry] DROP COLUMN [degree_program];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni_Registry]') AND [c].[name] = N'email_on_record');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Alumni_Registry] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Alumni_Registry] DROP COLUMN [email_on_record];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni_Registry]') AND [c].[name] = N'graduation_year');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Alumni_Registry] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Alumni_Registry] DROP COLUMN [graduation_year];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni]') AND [c].[name] = N'date_of_birth');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Alumni] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Alumni] DROP COLUMN [date_of_birth];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni]') AND [c].[name] = N'user_id');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Alumni] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Alumni] DROP COLUMN [user_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    ALTER TABLE [Alumni] ADD [age_at_graduation] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD CONSTRAINT [AK_AspNetUsers_JagId] UNIQUE ([JagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    ALTER TABLE [Alumni] ADD CONSTRAINT [FK_Alumni_AspNetUsers_jag_id] FOREIGN KEY ([jag_id]) REFERENCES [AspNetUsers] ([JagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260318194851_AddAgeAtGraduationColumn'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260318194851_AddAgeAtGraduationColumn', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni_Organizations] DROP CONSTRAINT [fk_ao_org];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    DROP TABLE [Organization_Types];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Degree_Programs]') AND [c].[name] = N'department');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Degree_Programs] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Degree_Programs] DROP COLUMN [department];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni]') AND [c].[name] = N'age_at_graduation');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [Alumni] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [Alumni] DROP COLUMN [age_at_graduation];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    EXEC sp_rename N'[Alumni_Organizations].[organization_type_id]', N'organization_id', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    EXEC sp_rename N'[Alumni_Organizations].[IX_Alumni_Organizations_organization_type_id]', N'IX_Alumni_Organizations_organization_id', 'INDEX';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni] ADD [college_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Degree_Programs] ADD [department_id] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Degree_Programs] ADD [is_active] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni] ADD [date_of_birth] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni] ADD [middle_name] nvarchar(50) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni] ADD [suffix] nvarchar(10) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE TABLE [Colleges] (
        [college_id] int NOT NULL IDENTITY,
        [college_name] nvarchar(150) NOT NULL,
        [is_internal] bit NOT NULL,
        [is_active] bit NOT NULL,
        CONSTRAINT [PK_Colleges] PRIMARY KEY ([college_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE TABLE [Departments] (
        [department_id] int NOT NULL IDENTITY,
        [college_id] int NOT NULL,
        [department_name] nvarchar(150) NOT NULL,
        [is_active] bit NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([department_id]),
        CONSTRAINT [fk_dept_college] FOREIGN KEY ([college_id]) REFERENCES [Colleges] ([college_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE TABLE [Student_Organizations] (
        [organization_id] int NOT NULL IDENTITY,
        [organization_name] nvarchar(150) NOT NULL,
        [college_id] int NOT NULL,
        [department_id] int NULL,
        [is_active] bit NOT NULL,
        CONSTRAINT [PK_Student_Organizations] PRIMARY KEY ([organization_id]),
        CONSTRAINT [fk_studorg_college] FOREIGN KEY ([college_id]) REFERENCES [Colleges] ([college_id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_studorg_department] FOREIGN KEY ([department_id]) REFERENCES [Departments] ([department_id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN

    INSERT INTO Colleges (college_name, is_internal, is_active) VALUES (N'School of Computing', 1, 1);
    DECLARE @DefaultCollegeId INT = SCOPE_IDENTITY();
    INSERT INTO Departments (college_id, department_name, is_active) VALUES (@DefaultCollegeId, N'General', 1);
    DECLARE @DefaultDepartmentId INT = SCOPE_IDENTITY();
    UPDATE Degree_Programs SET department_id = @DefaultDepartmentId WHERE department_id IS NULL;

END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Degree_Programs]') AND [c].[name] = N'department_id');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Degree_Programs] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Degree_Programs] ALTER COLUMN [department_id] int NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE INDEX [IX_Degree_Programs_department_id] ON [Degree_Programs] ([department_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE INDEX [IX_Alumni_college_id] ON [Alumni] ([college_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE INDEX [IX_Departments_college_id] ON [Departments] ([college_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE INDEX [IX_Student_Organizations_college_id] ON [Student_Organizations] ([college_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    CREATE INDEX [IX_Student_Organizations_department_id] ON [Student_Organizations] ([department_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni] ADD CONSTRAINT [fk_alumni_college] FOREIGN KEY ([college_id]) REFERENCES [Colleges] ([college_id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Alumni_Organizations] ADD CONSTRAINT [fk_ao_org] FOREIGN KEY ([organization_id]) REFERENCES [Student_Organizations] ([organization_id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    ALTER TABLE [Degree_Programs] ADD CONSTRAINT [fk_degree_department] FOREIGN KEY ([department_id]) REFERENCES [Departments] ([department_id]) ON DELETE NO ACTION;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905220925_AddCollegesDepartmentsAndStudentOrgs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905220925_AddCollegesDepartmentsAndStudentOrgs', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] ADD [assigned_at] datetime NOT NULL DEFAULT ((getdate()));
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] ADD [assigned_by] nvarchar(max) NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] ADD [is_active] bit NOT NULL DEFAULT CAST(1 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    ALTER TABLE [AspNetUserRoles] ADD [scope_mode] nvarchar(20) NOT NULL DEFAULT N'System';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    CREATE TABLE [User_Access_Scopes] (
        [user_access_scope_id] int NOT NULL IDENTITY,
        [user_id] nvarchar(450) NOT NULL,
        [role_id] nvarchar(450) NOT NULL,
        [college_id] int NULL,
        [department_id] int NULL,
        [access_level] nvarchar(20) NOT NULL,
        [is_active] bit NOT NULL,
        CONSTRAINT [PK_User_Access_Scopes] PRIMARY KEY ([user_access_scope_id]),
        CONSTRAINT [fk_scope_college] FOREIGN KEY ([college_id]) REFERENCES [Colleges] ([college_id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_scope_department] FOREIGN KEY ([department_id]) REFERENCES [Departments] ([department_id]) ON DELETE NO ACTION,
        CONSTRAINT [fk_scope_role] FOREIGN KEY ([role_id]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [fk_scope_user] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    CREATE INDEX [IX_User_Access_Scopes_college_id] ON [User_Access_Scopes] ([college_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    CREATE INDEX [IX_User_Access_Scopes_department_id] ON [User_Access_Scopes] ([department_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    CREATE INDEX [IX_User_Access_Scopes_role_id] ON [User_Access_Scopes] ([role_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    CREATE INDEX [IX_User_Access_Scopes_user_id] ON [User_Access_Scopes] ([user_id]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905224440_AddUserRolesAndAccessScopes'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905224440_AddUserRolesAndAccessScopes', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Degrees_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_degree_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Degrees_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Employment_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_employment_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Employment_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Internships_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_internship_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Internships_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Messages_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_message_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Messages_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Organizations_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [alumni_organization_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Organizations_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Alumni_Registry_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [registry_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Alumni_Registry_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Colleges_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [college_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Colleges_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Degree_Programs_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [degree_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Degree_Programs_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Departments_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [department_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Departments_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Employers_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [employer_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Employers_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Messages_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [message_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Messages_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [Student_Organizations_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [organization_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_Student_Organizations_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    CREATE TABLE [User_Access_Scopes_Audit] (
        [audit_id] int NOT NULL IDENTITY,
        [user_access_scope_id] int NOT NULL,
        [action_type] nvarchar(20) NOT NULL,
        [changed_by] nvarchar(256) NULL,
        [changed_at] datetime NOT NULL,
        [old_values] nvarchar(max) NULL,
        [new_values] nvarchar(max) NULL,
        CONSTRAINT [PK_User_Access_Scopes_Audit] PRIMARY KEY ([audit_id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260905225001_AddAuditTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260905225001_AddAuditTables', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907071729_RemoveAlumniAppUserForeignKey'
)
BEGIN
    ALTER TABLE [Alumni] DROP CONSTRAINT [FK_Alumni_AspNetUsers_jag_id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907071729_RemoveAlumniAppUserForeignKey'
)
BEGIN
    ALTER TABLE [AspNetUsers] DROP CONSTRAINT [AK_AspNetUsers_JagId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907071729_RemoveAlumniAppUserForeignKey'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907071729_RemoveAlumniAppUserForeignKey', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907192853_AddMustChangePassword'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [must_change_password] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260907192853_AddMustChangePassword'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260907192853_AddMustChangePassword', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909165403_AddUniqueIndexOnAppUserJagId'
)
BEGIN
    CREATE UNIQUE INDEX [UQ_AspNetUsers_JagId] ON [AspNetUsers] ([JagId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260909165403_AddUniqueIndexOnAppUserJagId'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260909165403_AddUniqueIndexOnAppUserJagId', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004195830_MakeInternshipEndDateOptional'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni_Internships]') AND [c].[name] = N'end_date');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [Alumni_Internships] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [Alumni_Internships] ALTER COLUMN [end_date] date NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261004195830_MakeInternshipEndDateOptional'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261004195830_MakeInternshipEndDateOptional', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005120000_MarkExistingAlumniActive'
)
BEGIN
    UPDATE [Alumni] SET [is_active] = 1 WHERE [is_active] = 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261005120000_MarkExistingAlumniActive'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261005120000_MarkExistingAlumniActive', N'9.0.12');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006191448_ReplaceDateOfBirthWithAgeAtGraduation'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Alumni]') AND [c].[name] = N'date_of_birth');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Alumni] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Alumni] DROP COLUMN [date_of_birth];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006191448_ReplaceDateOfBirthWithAgeAtGraduation'
)
BEGIN
    ALTER TABLE [Alumni] ADD [age_at_graduation] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261006191448_ReplaceDateOfBirthWithAgeAtGraduation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261006191448_ReplaceDateOfBirthWithAgeAtGraduation', N'9.0.12');
END;

COMMIT;
GO

