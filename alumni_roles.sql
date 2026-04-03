CREATE TABLE [Alumni_Registry] (
    [registry_id] int NOT NULL IDENTITY,
    [jag_id] nvarchar(20) NOT NULL,
    [first_name] nvarchar(50) NOT NULL,
    [last_name] nvarchar(50) NOT NULL,
    [account_created] bit NOT NULL,
    CONSTRAINT [PK__Alumni_R__EF8E9CE8B1C6B2D8] PRIMARY KEY ([registry_id])
);
GO


CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [JagId] nvarchar(20) NULL,
    [created_at] datetime NOT NULL,
    [is_first_login] bit NOT NULL,
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
GO


CREATE TABLE [Degree_Programs] (
    [degree_id] int NOT NULL IDENTITY,
    [institution] nvarchar(150) NOT NULL,
    [degree_type] nvarchar(20) NOT NULL,
    [major_field_of_study] nvarchar(100) NOT NULL,
    [department] nvarchar(100) NOT NULL,
    CONSTRAINT [PK__Degree_P__A1AFAEBBB780871C] PRIMARY KEY ([degree_id])
);
GO


CREATE TABLE [Employers] (
    [employer_id] int NOT NULL IDENTITY,
    [employer_name] nvarchar(150) NOT NULL,
    [location] nvarchar(150) NULL,
    [industry] nvarchar(100) NULL,
    CONSTRAINT [PK__Employer__365FA4E7DF9F3065] PRIMARY KEY ([employer_id])
);
GO


CREATE TABLE [Organization_Types] (
    [organization_type_id] int NOT NULL IDENTITY,
    [organization_name] nvarchar(150) NOT NULL,
    CONSTRAINT [PK__Organiza__466C7A244B987C0F] PRIMARY KEY ([organization_type_id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Alumni] (
    [alumni_id] int NOT NULL IDENTITY,
    [jag_id] nvarchar(20) NOT NULL,
    [user_id] nvarchar(450) NULL,
    [prefix] nvarchar(10) NULL,
    [first_name] nvarchar(50) NOT NULL,
    [preferred_first_name] nvarchar(50) NULL,
    [last_name] nvarchar(50) NOT NULL,
    [gender] nvarchar(20) NULL,
    [age_at_graduation] int NULL,
    [student_email] nvarchar(150) NULL,
    [permanent_email] nvarchar(150) NOT NULL,
    [phone] nvarchar(20) NULL,
    [address] nvarchar(255) NULL,
    [city] nvarchar(100) NULL,
    [state] nvarchar(50) NULL,
    [postcode] nvarchar(20) NULL,
    [country] nvarchar(50) NULL,
    [graduation_year] int NOT NULL,
    [solicitation_code] bit NOT NULL,
    [social_media_account] nvarchar(255) NULL,
    [privacy] bit NOT NULL DEFAULT CAST(1 AS bit),
    [is_active] bit NOT NULL DEFAULT CAST(1 AS bit),
    [last_updated] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Alumni__BB1DF35C3C7BFE94] PRIMARY KEY ([alumni_id]),
    CONSTRAINT [FK_Alumni_AspNetUsers_user_id] FOREIGN KEY ([user_id]) REFERENCES [AspNetUsers] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


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
GO


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
GO


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
GO


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
GO


CREATE TABLE [Alumni_Organizations] (
    [alumni_organization_id] int NOT NULL IDENTITY,
    [alumni_id] int NOT NULL,
    [organization_type_id] int NOT NULL,
    [officer_roles] nvarchar(150) NULL,
    CONSTRAINT [PK__Alumni_O__8366569D1933C852] PRIMARY KEY ([alumni_organization_id]),
    CONSTRAINT [fk_ao_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
    CONSTRAINT [fk_ao_org] FOREIGN KEY ([organization_type_id]) REFERENCES [Organization_Types] ([organization_type_id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Alumni_Messages] (
    [alumni_message_id] int NOT NULL IDENTITY,
    [alumni_id] int NOT NULL,
    [message_id] int NOT NULL,
    [sent_at] datetime NOT NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Alumni_M__E6B241004CC7DEC9] PRIMARY KEY ([alumni_message_id]),
    CONSTRAINT [fk_am_alumni] FOREIGN KEY ([alumni_id]) REFERENCES [Alumni] ([alumni_id]) ON DELETE CASCADE,
    CONSTRAINT [fk_am_message] FOREIGN KEY ([message_id]) REFERENCES [Messages] ([message_id])
);
GO


CREATE UNIQUE INDEX [IX_Alumni_jag_id] ON [Alumni] ([jag_id]);
GO


CREATE INDEX [IX_Alumni_user_id] ON [Alumni] ([user_id]);
GO


CREATE UNIQUE INDEX [UQ__Alumni__FBF400ED6AB71E0A] ON [Alumni] ([jag_id]);
GO


CREATE INDEX [IX_Alumni_Degrees_alumni_id] ON [Alumni_Degrees] ([alumni_id]);
GO


CREATE INDEX [IX_Alumni_Degrees_degree_id] ON [Alumni_Degrees] ([degree_id]);
GO


CREATE INDEX [IX_Alumni_Employment_alumni_id] ON [Alumni_Employment] ([alumni_id]);
GO


CREATE INDEX [IX_Alumni_Employment_employer_id] ON [Alumni_Employment] ([employer_id]);
GO


CREATE INDEX [IX_Alumni_Internships_alumni_id] ON [Alumni_Internships] ([alumni_id]);
GO


CREATE INDEX [IX_Alumni_Internships_employer_id] ON [Alumni_Internships] ([employer_id]);
GO


CREATE INDEX [IX_Alumni_Messages_alumni_id] ON [Alumni_Messages] ([alumni_id]);
GO


CREATE INDEX [IX_Alumni_Messages_message_id] ON [Alumni_Messages] ([message_id]);
GO


CREATE INDEX [IX_Alumni_Organizations_alumni_id] ON [Alumni_Organizations] ([alumni_id]);
GO


CREATE INDEX [IX_Alumni_Organizations_organization_type_id] ON [Alumni_Organizations] ([organization_type_id]);
GO


CREATE UNIQUE INDEX [UQ__Alumni_R__FBF400ED39BA228D] ON [Alumni_Registry] ([jag_id]);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE INDEX [IX_AspNetUsers_JagId] ON [AspNetUsers] ([JagId]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_Messages_created_by] ON [Messages] ([created_by]);
GO


