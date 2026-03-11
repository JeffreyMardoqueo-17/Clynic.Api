USE [ClynicDB];
GO

PRINT 'Aplicando patch: cita-especialidad...';
GO

IF OBJECT_ID(N'Cita', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Cita', 'IdEspecialidad') IS NULL
    BEGIN
        ALTER TABLE Cita ADD IdEspecialidad INT NULL;
        PRINT 'Se agregó columna Cita.IdEspecialidad';
    END
END
GO

IF OBJECT_ID(N'Cita', N'U') IS NOT NULL
   AND COL_LENGTH('Cita', 'IdEspecialidad') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Cita_Especialidad'
          AND parent_object_id = OBJECT_ID('Cita')
    )
    BEGIN
        ALTER TABLE Cita WITH NOCHECK
        ADD CONSTRAINT FK_Cita_Especialidad
            FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id);

        PRINT 'Se agregó FK_Cita_Especialidad';
    END
END
GO

IF OBJECT_ID(N'Cita', N'U') IS NOT NULL
   AND COL_LENGTH('Cita', 'IdEspecialidad') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Cita_IdEspecialidad'
          AND object_id = OBJECT_ID('Cita')
    )
    BEGIN
        CREATE INDEX IX_Cita_IdEspecialidad ON Cita(IdEspecialidad);
        PRINT 'Se agregó índice IX_Cita_IdEspecialidad';
    END
END
GO

PRINT 'Patch cita-especialidad aplicado.';
GO

IF OBJECT_ID(N'Paciente', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Paciente', 'IdEspecialidad') IS NULL
    BEGIN
        ALTER TABLE Paciente ADD IdEspecialidad INT NULL;
        PRINT 'Se agregó columna Paciente.IdEspecialidad';
    END
END
GO

IF OBJECT_ID(N'Paciente', N'U') IS NOT NULL
   AND COL_LENGTH('Paciente', 'IdEspecialidad') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Paciente_Especialidad'
          AND parent_object_id = OBJECT_ID('Paciente')
    )
    BEGIN
        ALTER TABLE Paciente WITH NOCHECK
        ADD CONSTRAINT FK_Paciente_Especialidad
            FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id);

        PRINT 'Se agregó FK_Paciente_Especialidad';
    END
END
GO

IF OBJECT_ID(N'Paciente', N'U') IS NOT NULL
   AND COL_LENGTH('Paciente', 'IdEspecialidad') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Paciente_IdEspecialidad'
          AND object_id = OBJECT_ID('Paciente')
    )
    BEGIN
        CREATE INDEX IX_Paciente_IdEspecialidad ON Paciente(IdEspecialidad);
        PRINT 'Se agregó índice IX_Paciente_IdEspecialidad';
    END
END
GO

PRINT 'Patch cita/paciente-especialidad aplicado.';
GO

IF OBJECT_ID(N'CitaActividad', N'U') IS NULL
BEGIN
    CREATE TABLE CitaActividad (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        IdCita INT NOT NULL,
        IdClinica INT NOT NULL,
        IdSucursal INT NOT NULL,
        IdUsuario INT NULL,
        RolUsuario NVARCHAR(80) NOT NULL,
        Accion NVARCHAR(80) NOT NULL,
        Detalle NVARCHAR(400) NOT NULL,
        FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_CitaActividad_Cita FOREIGN KEY (IdCita) REFERENCES Cita(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'CitaActividad', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM sys.indexes
       WHERE name = 'IX_CitaActividad_Clinica_Fecha'
         AND object_id = OBJECT_ID('CitaActividad')
   )
BEGIN
    CREATE INDEX IX_CitaActividad_Clinica_Fecha ON CitaActividad(IdClinica, FechaCreacion DESC);
END
GO

PRINT 'Patch cita-actividad aplicado.';
GO

IF OBJECT_ID(N'Servicio', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Servicio', 'IdEspecialidad') IS NULL
    BEGIN
        ALTER TABLE Servicio ADD IdEspecialidad INT NULL;
        PRINT 'Se agregó columna Servicio.IdEspecialidad';
    END
END
GO

IF OBJECT_ID(N'Servicio', N'U') IS NOT NULL
   AND COL_LENGTH('Servicio', 'IdEspecialidad') IS NOT NULL
BEGIN
    ;WITH ServicioConEspecialidad AS (
        SELECT
            s.Id AS IdServicio,
            MAX(c.IdEspecialidad) AS IdEspecialidad
        FROM Servicio s
        INNER JOIN CitaServicio cs ON cs.IdServicio = s.Id
        INNER JOIN Cita c ON c.Id = cs.IdCita
        GROUP BY s.Id
    )
    UPDATE s
    SET s.IdEspecialidad = sce.IdEspecialidad
    FROM Servicio s
    INNER JOIN ServicioConEspecialidad sce ON sce.IdServicio = s.Id
    WHERE s.IdEspecialidad IS NULL;

    UPDATE s
    SET s.IdEspecialidad = fallback.IdEspecialidad
    FROM Servicio s
    CROSS APPLY (
        SELECT TOP 1 e.Id AS IdEspecialidad
        FROM Especialidad e
        WHERE (e.IdClinica = s.IdClinica OR e.IdClinica IS NULL)
          AND e.Activa = 1
        ORDER BY CASE WHEN e.IdClinica = s.IdClinica THEN 0 ELSE 1 END, e.Id
    ) fallback
    WHERE s.IdEspecialidad IS NULL;

    IF EXISTS (SELECT 1 FROM Servicio WHERE IdEspecialidad IS NULL)
    BEGIN
        RAISERROR('No fue posible resolver IdEspecialidad para todos los servicios.', 16, 1);
    END
END
GO

IF OBJECT_ID(N'Servicio', N'U') IS NOT NULL
   AND COL_LENGTH('Servicio', 'IdEspecialidad') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Servicio_Especialidad'
          AND parent_object_id = OBJECT_ID('Servicio')
    )
    BEGIN
        ALTER TABLE Servicio WITH NOCHECK
        ADD CONSTRAINT FK_Servicio_Especialidad
            FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id);

        PRINT 'Se agregó FK_Servicio_Especialidad';
    END

    IF EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('Servicio')
          AND name = 'IdEspecialidad'
          AND is_nullable = 1
    )
    BEGIN
        ALTER TABLE Servicio ALTER COLUMN IdEspecialidad INT NOT NULL;
        PRINT 'Se marcó Servicio.IdEspecialidad como NOT NULL';
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'IX_Servicio_Clinica_Especialidad'
          AND object_id = OBJECT_ID('Servicio')
    )
    BEGIN
        CREATE INDEX IX_Servicio_Clinica_Especialidad ON Servicio(IdClinica, IdEspecialidad);
        PRINT 'Se agregó índice IX_Servicio_Clinica_Especialidad';
    END
END
GO
