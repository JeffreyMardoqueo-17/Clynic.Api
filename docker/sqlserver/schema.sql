USE [ClynicDB];
GO

/* =========================================
   1. CLINICA
========================================= */
CREATE TABLE Clinica (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(50),
    Direccion NVARCHAR(250),
    Activa BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE()
);
GO

/* =========================================
   2. SUCURSAL
========================================= */
CREATE TABLE Sucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    Nombre NVARCHAR(150),
    Direccion NVARCHAR(250),
    Activa BIT DEFAULT 1,

    CONSTRAINT FK_Sucursal_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id)
);
GO

/* =========================================
   3. USUARIO
========================================= */
CREATE TABLE Usuario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    IdSucursal INT NULL,

    NombreCompleto NVARCHAR(150),
    Correo NVARCHAR(150),
    ClaveHash NVARCHAR(300),
    Rol NVARCHAR(50),

    Activo BIT DEFAULT 1,
    DebeCambiarClave BIT DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Usuario_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id),

    CONSTRAINT FK_Usuario_Sucursal
        FOREIGN KEY (IdSucursal)
        REFERENCES Sucursal(Id)
);
GO

/* =========================================
   4. PACIENTE
========================================= */
CREATE TABLE Paciente (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    Nombres NVARCHAR(150) NOT NULL,
    Apellidos NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(50),
    Correo NVARCHAR(150),
    FechaNacimiento DATE NULL,
    FechaRegistro DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Paciente_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id)
);
GO

/* =========================================
   5. SERVICIO
========================================= */
CREATE TABLE Servicio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,

    NombreServicio NVARCHAR(150),
    DuracionMin INT NOT NULL,
    PrecioBase DECIMAL(10,2),
    Activo BIT DEFAULT 1,

    CONSTRAINT FK_Servicio_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id)
);
GO

/* =========================================
   6. HORARIO SUCURSAL
========================================= */
CREATE TABLE HorarioSucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdSucursal INT NOT NULL,

    DiaSemana INT,
    HoraInicio TIME,
    HoraFin TIME,

    CONSTRAINT FK_HorarioSucursal_Sucursal
        FOREIGN KEY (IdSucursal)
        REFERENCES Sucursal(Id)
);
GO

/* =========================================
   7. CITA
========================================= */
CREATE TABLE Cita (
    Id INT IDENTITY(1,1) PRIMARY KEY,

    IdClinica INT NOT NULL,
    IdSucursal INT NOT NULL,
    IdPaciente INT NOT NULL,
    IdDoctor INT NULL,

    FechaHoraInicioPlan DATETIME NOT NULL,
    FechaHoraFinPlan DATETIME NOT NULL,
    FechaHoraInicioReal DATETIME NULL,
    FechaHoraFinReal DATETIME NULL,

    Estado NVARCHAR(50) DEFAULT 'Pendiente',
    Notas NVARCHAR(250),

    SubTotal DECIMAL(10,2) DEFAULT 0,
    TotalFinal DECIMAL(10,2) DEFAULT 0,
    FechaCreacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Cita_Clinica
        FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),

    CONSTRAINT FK_Cita_Sucursal
        FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id),

    CONSTRAINT FK_Cita_Paciente
        FOREIGN KEY (IdPaciente) REFERENCES Paciente(Id),

    CONSTRAINT FK_Cita_Doctor
        FOREIGN KEY (IdDoctor) REFERENCES Usuario(Id)
);
GO

/* =========================================
   8. CITA SERVICIO (DETALLE)
========================================= */
CREATE TABLE CitaServicio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCita INT NOT NULL,
    IdServicio INT NOT NULL,

    DuracionMin INT,
    Precio DECIMAL(10,2),

    CONSTRAINT FK_CitaServicio_Cita
        FOREIGN KEY (IdCita) REFERENCES Cita(Id),

    CONSTRAINT FK_CitaServicio_Servicio
        FOREIGN KEY (IdServicio) REFERENCES Servicio(Id)
);
GO

/* =========================================
   9. HISTORIAL CLINICO
========================================= */
CREATE TABLE HistorialClinico (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdPaciente INT NOT NULL UNIQUE,
    EnfermedadesPrevias NVARCHAR(MAX) NULL,
    MedicamentosActuales NVARCHAR(MAX) NULL,
    Alergias NVARCHAR(MAX) NULL,
    AntecedentesFamiliares NVARCHAR(MAX) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaActualizacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_HistorialClinico_Paciente
        FOREIGN KEY (IdPaciente)
        REFERENCES Paciente(Id)
        ON DELETE CASCADE
);
GO

/* =========================================
   10. CONSULTA MEDICA
========================================= */
CREATE TABLE ConsultaMedica (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCita INT NOT NULL UNIQUE,
    IdClinica INT NOT NULL,
    IdSucursal INT NOT NULL,
    IdPaciente INT NOT NULL,
    IdDoctor INT NULL,
    Diagnostico NVARCHAR(MAX) NULL,
    Tratamiento NVARCHAR(MAX) NULL,
    Receta NVARCHAR(MAX) NULL,
    ExamenesSolicitados NVARCHAR(MAX) NULL,
    NotasMedicas NVARCHAR(MAX) NULL,
    FechaConsulta DATETIME DEFAULT GETDATE(),
    FechaCreacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_ConsultaMedica_Cita
        FOREIGN KEY (IdCita)
        REFERENCES Cita(Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_ConsultaMedica_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id),

    CONSTRAINT FK_ConsultaMedica_Sucursal
        FOREIGN KEY (IdSucursal)
        REFERENCES Sucursal(Id),

    CONSTRAINT FK_ConsultaMedica_Paciente
        FOREIGN KEY (IdPaciente)
        REFERENCES Paciente(Id),

    CONSTRAINT FK_ConsultaMedica_Doctor
        FOREIGN KEY (IdDoctor)
        REFERENCES Usuario(Id)
);
GO

/* =========================================
   11. CODIGO VERIFICACION
========================================= */
CREATE TABLE CodigoVerificacion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,

    Codigo NVARCHAR(12) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,

    FechaCreacion DATETIME DEFAULT GETDATE(),
    FechaExpiracion DATETIME NOT NULL,
    
    Usado BIT DEFAULT 0,
    FechaUso DATETIME NULL,

    CONSTRAINT FK_CodigoVerificacion_Usuario
        FOREIGN KEY (IdUsuario)
        REFERENCES Usuario(Id)
        ON DELETE CASCADE
);
GO

/* =========================================
    12. INDICES BASE
========================================= */
CREATE INDEX IX_Paciente_IdClinica ON Paciente(IdClinica);
CREATE INDEX IX_Paciente_IdClinica_Correo ON Paciente(IdClinica, Correo);
CREATE INDEX IX_Cita_IdClinica_FechaHoraInicioPlan ON Cita(IdClinica, FechaHoraInicioPlan);
CREATE INDEX IX_Cita_IdSucursal_FechaHoraInicioPlan ON Cita(IdSucursal, FechaHoraInicioPlan);
CREATE INDEX IX_ConsultaMedica_Clinica_Paciente_Fecha ON ConsultaMedica(IdClinica, IdPaciente, FechaConsulta);

CREATE INDEX IX_CodigoVerificacion_IdUsuario ON CodigoVerificacion(IdUsuario);
CREATE INDEX IX_CodigoVerificacion_Codigo ON CodigoVerificacion(Codigo);
CREATE INDEX IX_Usuario_IdSucursal ON Usuario(IdSucursal);
GO

/* =========================================
    13. ASUETO SUCURSAL
========================================= */
CREATE TABLE AsuetoSucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdSucursal INT NOT NULL,
    Fecha DATE NOT NULL,
    Motivo NVARCHAR(200) NULL,
    FechaCreacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_AsuetoSucursal_Sucursal
        FOREIGN KEY (IdSucursal)
        REFERENCES Sucursal(Id)
        ON DELETE CASCADE
);
GO

CREATE UNIQUE INDEX UX_AsuetoSucursal_Sucursal_Fecha
    ON AsuetoSucursal(IdSucursal, Fecha);
GO
