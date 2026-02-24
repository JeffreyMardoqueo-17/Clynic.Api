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

    NombreCompleto NVARCHAR(150),
    Correo NVARCHAR(150),
    ClaveHash NVARCHAR(300),
    Rol NVARCHAR(50),

    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),

    CONSTRAINT FK_Usuario_Clinica
        FOREIGN KEY (IdClinica)
        REFERENCES Clinica(Id)
);
GO

/* =========================================
   4. PACIENTE
========================================= */
CREATE TABLE Paciente (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    NombreCompleto NVARCHAR(150),
    Telefono NVARCHAR(50),
    FechaRegistro DATETIME DEFAULT GETDATE()
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
   9. CODIGO VERIFICACION
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
   10. INDICES PARA CODIGO VERIFICACION
========================================= */
CREATE INDEX IX_CodigoVerificacion_IdUsuario ON CodigoVerificacion(IdUsuario);
CREATE INDEX IX_CodigoVerificacion_Codigo ON CodigoVerificacion(Codigo);
GO
