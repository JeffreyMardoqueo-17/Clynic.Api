USE [ClynicDB];
GO

IF OBJECT_ID(N'LandingPageConfig', N'U') IS NULL
BEGIN
    CREATE TABLE LandingPageConfig (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdClinica INT NOT NULL,
        NombreLanding NVARCHAR(160) NOT NULL,
        HeroTitulo NVARCHAR(180) NOT NULL,
        HeroSubtitulo NVARCHAR(220) NOT NULL,
        DescripcionGeneral NVARCHAR(1200) NULL,
        TelefonoContacto NVARCHAR(80) NULL,
        CorreoContacto NVARCHAR(180) NULL,
        DireccionContacto NVARCHAR(280) NULL,
        WhatsappContacto NVARCHAR(80) NULL,
        CtaPrincipalTexto NVARCHAR(80) NULL,
        CtaPrincipalUrl NVARCHAR(350) NULL,
        ServiciosJson NVARCHAR(MAX) NOT NULL CONSTRAINT DF_LandingPageConfig_ServiciosJson DEFAULT N'[]',
        MetaTitulo NVARCHAR(180) NULL,
        MetaDescripcion NVARCHAR(300) NULL,
        DominioBase NVARCHAR(160) NULL,
        MostrarHorariosSucursal BIT NOT NULL CONSTRAINT DF_LandingPageConfig_MostrarHorarios DEFAULT 1,
        Publicada BIT NOT NULL CONSTRAINT DF_LandingPageConfig_Publicada DEFAULT 0,
        FechaCreacion DATETIME NOT NULL CONSTRAINT DF_LandingPageConfig_FechaCreacion DEFAULT GETDATE(),
        FechaActualizacion DATETIME NOT NULL CONSTRAINT DF_LandingPageConfig_FechaActualizacion DEFAULT GETDATE(),
        CONSTRAINT FK_LandingPageConfig_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id) ON DELETE CASCADE,
        CONSTRAINT UX_LandingPageConfig_Clinica UNIQUE (IdClinica)
    );
END
GO
