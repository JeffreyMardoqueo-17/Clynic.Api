USE [ClynicDB];
GO

/* =========================================
   1. CLINICA
========================================= */
CREATE TABLE Clinica (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(50) NULL,
    Direccion NVARCHAR(250) NULL,
    Activa BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE()
);
GO

/* =========================================
   2. SUCURSAL
========================================= */
CREATE TABLE Sucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Direccion NVARCHAR(250) NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Sucursal_Clinica
        FOREIGN KEY (IdClinica) REFERENCES Clinica(Id)
);
GO

/* =========================================
   3. ROL
========================================= */
CREATE TABLE Rol (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT  NULL, -- Permite NULL para roles globales (ej. Admin)
   IdSucursal INT NULL,
    Nombre NVARCHAR(80) NOT NULL,
    Descripcion NVARCHAR(250) NULL,
    Activo BIT NOT NULL DEFAULT 1,
   CONSTRAINT UX_Rol_Nombre UNIQUE (Nombre),
   CONSTRAINT FK_Rol_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),
   CONSTRAINT FK_Rol_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id)
);
GO

/* =========================================
   4. ESPECIALIDAD
========================================= */
CREATE TABLE Especialidad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(400) NULL,
    Activa BIT NOT NULL DEFAULT 1,
   CONSTRAINT UX_Especialidad_Nombre UNIQUE (Nombre)
);
GO

/* =========================================
   4.1. ROL ESPECIALIDAD
========================================= */
CREATE TABLE RolEspecialidad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdRol INT NOT NULL,
    IdEspecialidad INT NOT NULL,
    Activa BIT NOT NULL DEFAULT 1,
    CONSTRAINT UX_RolEspecialidad_Rol_Especialidad UNIQUE (IdRol, IdEspecialidad),
    CONSTRAINT FK_RolEspecialidad_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolEspecialidad_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id) ON DELETE CASCADE
);
GO

/* =========================================
   4.2. SUCURSAL ROL ESPECIALIDAD
========================================= */
CREATE TABLE SucursalRolEspecialidad (
   Id INT IDENTITY(1,1) PRIMARY KEY,
   IdSucursal INT NOT NULL,
   IdRol INT NOT NULL,
   IdEspecialidad INT NOT NULL,
   Activa BIT NOT NULL DEFAULT 1,
   CONSTRAINT UX_SucursalRolEspecialidad UNIQUE (IdSucursal, IdRol, IdEspecialidad),
   CONSTRAINT FK_SucursalRolEspecialidad_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id) ON DELETE CASCADE,
   CONSTRAINT FK_SucursalRolEspecialidad_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id),
   CONSTRAINT FK_SucursalRolEspecialidad_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id)
);
GO

/* =========================================
   5. SUCURSAL ESPECIALIDAD
========================================= */
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
GO

/* =========================================
   6. USUARIO
========================================= */
CREATE TABLE Usuario (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    IdSucursal INT NULL,
    IdRol INT NOT NULL,
    IdEspecialidad INT NULL,
    NombreCompleto NVARCHAR(150) NOT NULL,
    Correo NVARCHAR(150) NOT NULL,
    ClaveHash NVARCHAR(300) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    DebeCambiarClave BIT NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Usuario_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),
    CONSTRAINT FK_Usuario_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (IdRol) REFERENCES Rol(Id),
    CONSTRAINT FK_Usuario_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id)
);
GO

/* =========================================
   7. PACIENTE
========================================= */
CREATE TABLE Paciente (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    IdEspecialidad INT NULL,
    Nombres NVARCHAR(150) NOT NULL,
    Apellidos NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(50) NULL,
    Correo NVARCHAR(150) NULL,
    FechaNacimiento DATE NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Paciente_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),
    CONSTRAINT FK_Paciente_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id)
);
GO

/* =========================================
   8. SERVICIO
========================================= */
CREATE TABLE Servicio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    NombreServicio NVARCHAR(150) NOT NULL,
    DuracionMin INT NOT NULL,
    PrecioBase DECIMAL(10,2) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Servicio_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id)
);
GO

/* =========================================
   9. HORARIO SUCURSAL
========================================= */
CREATE TABLE HorarioSucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdSucursal INT NOT NULL,
    DiaSemana INT NULL,
    HoraInicio TIME NULL,
    HoraFin TIME NULL,

    CONSTRAINT FK_HorarioSucursal_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id)
);
GO

/* =========================================
   10. CITA
========================================= */
CREATE TABLE Cita (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdClinica INT NOT NULL,
    IdSucursal INT NOT NULL,
    IdPaciente INT NOT NULL,
    IdEspecialidad INT NOT NULL,
    IdDoctor INT NULL,
    FechaHoraInicioPlan DATETIME NOT NULL,
    FechaHoraFinPlan DATETIME NOT NULL,
    FechaHoraInicioReal DATETIME NULL,
    FechaHoraFinReal DATETIME NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
    Notas NVARCHAR(250) NULL,
    SubTotal DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalFinal DECIMAL(10,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Cita_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),
    CONSTRAINT FK_Cita_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id),
    CONSTRAINT FK_Cita_Paciente FOREIGN KEY (IdPaciente) REFERENCES Paciente(Id),
    CONSTRAINT FK_Cita_Especialidad FOREIGN KEY (IdEspecialidad) REFERENCES Especialidad(Id),
    CONSTRAINT FK_Cita_Doctor FOREIGN KEY (IdDoctor) REFERENCES Usuario(Id)
);
GO

/* =========================================
   11. CITA SERVICIO
========================================= */
CREATE TABLE CitaServicio (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCita INT NOT NULL,
    IdServicio INT NOT NULL,
    DuracionMin INT NULL,
    Precio DECIMAL(10,2) NULL,

    CONSTRAINT FK_CitaServicio_Cita FOREIGN KEY (IdCita) REFERENCES Cita(Id),
    CONSTRAINT FK_CitaServicio_Servicio FOREIGN KEY (IdServicio) REFERENCES Servicio(Id)
);
GO

/* =========================================
   12. HISTORIAL CLINICO
========================================= */
CREATE TABLE HistorialClinico (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdPaciente INT NOT NULL UNIQUE,
    EnfermedadesPrevias NVARCHAR(MAX) NULL,
    MedicamentosActuales NVARCHAR(MAX) NULL,
    Alergias NVARCHAR(MAX) NULL,
    AntecedentesFamiliares NVARCHAR(MAX) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaActualizacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_HistorialClinico_Paciente
        FOREIGN KEY (IdPaciente) REFERENCES Paciente(Id) ON DELETE CASCADE
);
GO

/* =========================================
   13. CONSULTA MEDICA
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
    FechaConsulta DATETIME NOT NULL DEFAULT GETDATE(),
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ConsultaMedica_Cita FOREIGN KEY (IdCita) REFERENCES Cita(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ConsultaMedica_Clinica FOREIGN KEY (IdClinica) REFERENCES Clinica(Id),
    CONSTRAINT FK_ConsultaMedica_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id),
    CONSTRAINT FK_ConsultaMedica_Paciente FOREIGN KEY (IdPaciente) REFERENCES Paciente(Id),
    CONSTRAINT FK_ConsultaMedica_Doctor FOREIGN KEY (IdDoctor) REFERENCES Usuario(Id)
);
GO

/* =========================================
   14. CODIGO VERIFICACION
========================================= */
CREATE TABLE CodigoVerificacion (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdUsuario INT NOT NULL,
    Codigo NVARCHAR(12) NOT NULL,
    Tipo NVARCHAR(50) NOT NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaExpiracion DATETIME NOT NULL,
    Usado BIT NOT NULL DEFAULT 0,
    FechaUso DATETIME NULL,
    CONSTRAINT FK_CodigoVerificacion_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario(Id) ON DELETE CASCADE
);
GO

/* =========================================
   15. ASUETO SUCURSAL
========================================= */
CREATE TABLE AsuetoSucursal (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdSucursal INT NOT NULL,
    Fecha DATE NOT NULL,
    Motivo NVARCHAR(200) NULL,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_AsuetoSucursal_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id) ON DELETE CASCADE
);
GO

/* =========================================
   16. INDICES
========================================= */
CREATE INDEX IX_Paciente_IdClinica ON Paciente(IdClinica);
CREATE INDEX IX_Paciente_IdClinica_Correo ON Paciente(IdClinica, Correo);
CREATE INDEX IX_Usuario_IdSucursal ON Usuario(IdSucursal);
CREATE INDEX IX_Usuario_IdRol ON Usuario(IdRol);
CREATE INDEX IX_Usuario_IdEspecialidad ON Usuario(IdEspecialidad);
CREATE UNIQUE INDEX UX_Usuario_Clinica_Correo ON Usuario(IdClinica, Correo);
CREATE INDEX IX_Cita_IdClinica_FechaHoraInicioPlan ON Cita(IdClinica, FechaHoraInicioPlan);
CREATE INDEX IX_Cita_IdSucursal_FechaHoraInicioPlan ON Cita(IdSucursal, FechaHoraInicioPlan);
CREATE INDEX IX_Cita_Especialidad_Fecha ON Cita(IdClinica, IdSucursal, IdEspecialidad, FechaHoraInicioPlan);
CREATE INDEX IX_ConsultaMedica_Clinica_Paciente_Fecha ON ConsultaMedica(IdClinica, IdPaciente, FechaConsulta);
CREATE INDEX IX_CodigoVerificacion_IdUsuario ON CodigoVerificacion(IdUsuario);
CREATE INDEX IX_CodigoVerificacion_Codigo ON CodigoVerificacion(Codigo);
CREATE UNIQUE INDEX UX_AsuetoSucursal_Sucursal_Fecha ON AsuetoSucursal(IdSucursal, Fecha);
GO
