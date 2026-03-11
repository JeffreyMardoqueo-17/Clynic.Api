USE [ClynicDB];
GO

PRINT 'Aplicando patch: usuario-rol-especialidad...';
GO

IF OBJECT_ID(N'Rol', N'U') IS NULL
BEGIN
    CREATE TABLE Rol (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdClinica INT NULL,
        IdSucursal INT NULL,
        Nombre NVARCHAR(80) NOT NULL,
        Descripcion NVARCHAR(250) NULL,
        Activo BIT NOT NULL DEFAULT 1
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Rol_Clinica_Sucursal_Nombre' AND object_id = OBJECT_ID('Rol'))
    BEGIN
        CREATE UNIQUE INDEX UX_Rol_Clinica_Sucursal_Nombre ON Rol(IdClinica, IdSucursal, Nombre);
    END
END
GO

IF OBJECT_ID(N'Especialidad', N'U') IS NULL
BEGIN
    CREATE TABLE Especialidad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdClinica INT NULL,
        Nombre NVARCHAR(100) NOT NULL,
        Descripcion NVARCHAR(400) NULL,
        Activa BIT NOT NULL DEFAULT 1
    );

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Especialidad_Clinica_Nombre' AND object_id = OBJECT_ID('Especialidad'))
    BEGIN
        CREATE UNIQUE INDEX UX_Especialidad_Clinica_Nombre ON Especialidad(IdClinica, Nombre);
    END
END
GO

IF OBJECT_ID(N'Rol', N'U') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE [name] = 'UX_Rol_Nombre'
          AND [type] = 'UQ'
          AND parent_object_id = OBJECT_ID('Rol')
    )
    BEGIN
        ALTER TABLE Rol DROP CONSTRAINT UX_Rol_Nombre;
    END

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Rol_Nombre' AND object_id = OBJECT_ID('Rol'))
    BEGIN
        DROP INDEX UX_Rol_Nombre ON Rol;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Rol_Clinica_Sucursal_Nombre' AND object_id = OBJECT_ID('Rol'))
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM Rol
            GROUP BY IdClinica, IdSucursal, Nombre
            HAVING COUNT(*) > 1
        )
        BEGIN
            CREATE UNIQUE INDEX UX_Rol_Clinica_Sucursal_Nombre ON Rol(IdClinica, IdSucursal, Nombre);
        END
    END
END
GO

IF OBJECT_ID(N'Especialidad', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Especialidad', 'IdClinica') IS NULL
    BEGIN
        ALTER TABLE Especialidad ADD IdClinica INT NULL;
    END

    IF EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE [name] = 'UX_Especialidad_Nombre'
          AND [type] = 'UQ'
          AND parent_object_id = OBJECT_ID('Especialidad')
    )
    BEGIN
        ALTER TABLE Especialidad DROP CONSTRAINT UX_Especialidad_Nombre;
    END

    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Especialidad_Nombre' AND object_id = OBJECT_ID('Especialidad'))
    BEGIN
        DROP INDEX UX_Especialidad_Nombre ON Especialidad;
    END

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Especialidad_Clinica_Nombre' AND object_id = OBJECT_ID('Especialidad'))
    BEGIN
        IF NOT EXISTS (
            SELECT 1
            FROM Especialidad
            GROUP BY IdClinica, Nombre
            HAVING COUNT(*) > 1
        )
        BEGIN
            CREATE UNIQUE INDEX UX_Especialidad_Clinica_Nombre ON Especialidad(IdClinica, Nombre);
        END
    END
END
GO

IF OBJECT_ID(N'RolEspecialidad', N'U') IS NULL
BEGIN
    CREATE TABLE RolEspecialidad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdRol INT NOT NULL,
        IdEspecialidad INT NOT NULL,
        Activa BIT NOT NULL DEFAULT 1,
        CONSTRAINT UX_RolEspecialidad_Rol_Especialidad UNIQUE (IdRol, IdEspecialidad),
        CONSTRAINT FK_RolEspecialidad_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolEspecialidad_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'SucursalEspecialidad', N'U') IS NULL
BEGIN
    CREATE TABLE SucursalEspecialidad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdSucursal INT NOT NULL,
        IdEspecialidad INT NOT NULL,
        CitasMaximasPorDia INT NOT NULL DEFAULT 0,
        Activa BIT NOT NULL DEFAULT 1,
        CONSTRAINT UX_SucursalEspecialidad_Sucursal_Especialidad UNIQUE (IdSucursal, IdEspecialidad),
        CONSTRAINT FK_SucursalEspecialidad_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id) ON DELETE CASCADE,
        CONSTRAINT FK_SucursalEspecialidad_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id)
    );
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Usuario', 'IdRol') IS NULL
    BEGIN
        ALTER TABLE Usuario ADD IdRol INT NULL;
    END

    IF COL_LENGTH('Usuario', 'IdEspecialidad') IS NULL
    BEGIN
        ALTER TABLE Usuario ADD IdEspecialidad INT NULL;
    END
END
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE LOWER(Nombre) = 'admin')
BEGIN
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    VALUES (NULL, NULL, 'Admin', 'Usuario global administrador', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE LOWER(Nombre) = 'doctor')
BEGIN
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    VALUES (NULL, NULL, 'Doctor', 'Rol profesional médico', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM Rol WHERE LOWER(Nombre) = 'recepcionista')
BEGIN
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    VALUES (NULL, NULL, 'Recepcionista', 'Rol operativo recepción', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM Especialidad WHERE LOWER(Nombre) = 'encargado global')
BEGIN
    INSERT INTO Especialidad (IdClinica, Nombre, Descripcion, Activa)
    VALUES (NULL, 'Encargado Global', 'Especialidad global para administradores', 1);
END
GO

IF NOT EXISTS (SELECT 1 FROM Especialidad WHERE LOWER(Nombre) = 'atencion al cliente')
BEGIN
    INSERT INTO Especialidad (IdClinica, Nombre, Descripcion, Activa)
    VALUES (NULL, 'Atencion al Cliente', 'Especialidad por defecto para recepcionistas', 1);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND COL_LENGTH('Usuario', 'Rol') IS NOT NULL
BEGIN
    ;WITH RolesFuente AS (
        SELECT DISTINCT LTRIM(RTRIM(Rol)) AS NombreRol
        FROM Usuario
        WHERE Rol IS NOT NULL AND LTRIM(RTRIM(Rol)) <> ''
    )
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    SELECT NULL, NULL, rf.NombreRol, CONCAT('Migrado desde columna Usuario.Rol: ', rf.NombreRol), 1
    FROM RolesFuente rf
    WHERE NOT EXISTS (
        SELECT 1
        FROM Rol r
        WHERE LOWER(r.Nombre) = LOWER(rf.NombreRol)
    );

    UPDATE u
    SET u.IdRol = r.Id
    FROM Usuario u
    INNER JOIN Rol r ON LOWER(r.Nombre) = LOWER(LTRIM(RTRIM(u.Rol)))
    WHERE u.IdRol IS NULL
      AND u.Rol IS NOT NULL
      AND LTRIM(RTRIM(u.Rol)) <> '';
END
GO

DECLARE @AdminRolId INT;
SELECT TOP 1 @AdminRolId = Id FROM Rol WHERE LOWER(Nombre) = 'admin';

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL AND @AdminRolId IS NOT NULL
BEGIN
    UPDATE Usuario
    SET IdRol = @AdminRolId
    WHERE IdRol IS NULL;
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND COL_LENGTH('Usuario', 'IdRol') IS NOT NULL
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.columns
        WHERE object_id = OBJECT_ID('Usuario')
          AND name = 'IdRol'
          AND is_nullable = 1
    )
    BEGIN
        ALTER TABLE Usuario ALTER COLUMN IdRol INT NOT NULL;
    END
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE name = 'FK_Usuario_Rol'
         AND parent_object_id = OBJECT_ID('Usuario')
   )
BEGIN
    ALTER TABLE Usuario WITH NOCHECK
    ADD CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND COL_LENGTH('Usuario', 'IdEspecialidad') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.foreign_keys
       WHERE name = 'FK_Usuario_Especialidad'
         AND parent_object_id = OBJECT_ID('Usuario')
   )
BEGIN
    ALTER TABLE Usuario WITH NOCHECK
    ADD CONSTRAINT FK_Usuario_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'IX_Usuario_IdRol'
         AND object_id = OBJECT_ID('Usuario')
   )
BEGIN
    CREATE INDEX IX_Usuario_IdRol ON Usuario(IdRol);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND COL_LENGTH('Usuario', 'IdEspecialidad') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'IX_Usuario_IdEspecialidad'
         AND object_id = OBJECT_ID('Usuario')
   )
BEGIN
    CREATE INDEX IX_Usuario_IdEspecialidad ON Usuario(IdEspecialidad);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'UX_Usuario_Clinica_Correo'
         AND object_id = OBJECT_ID('Usuario')
   )
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM Usuario
        GROUP BY IdClinica, Correo
        HAVING COUNT(*) > 1
    )
    BEGIN
        CREATE UNIQUE INDEX UX_Usuario_Clinica_Correo ON Usuario(IdClinica, Correo);
    END
    ELSE
    BEGIN
        PRINT 'Aviso: no se creó UX_Usuario_Clinica_Correo porque existen correos duplicados por clínica.';
    END
END
GO

PRINT 'Patch usuario-rol-especialidad aplicado.';
GO
