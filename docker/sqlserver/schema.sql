---------AQUI VAN TODAS LAS TABLAS DE LA BASE DE DATOS
USE SistemaCitasClinicasSaaS;
GO

/* =========================================
   1. CLINICAS
========================================= */
CREATE TABLE Clinicas (
    IdClinica INT IDENTITY PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Telefono NVARCHAR(50),
    Direccion NVARCHAR(250),
    Activa BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE()
);
GO

/* =========================================
   2. SUCURSALES
========================================= */
CREATE TABLE Sucursales (
    IdSucursal INT IDENTITY PRIMARY KEY,
    IdClinica INT NOT NULL,
    Nombre NVARCHAR(150),
    Direccion NVARCHAR(250),
    Activa BIT DEFAULT 1,

    FOREIGN KEY (IdClinica)
        REFERENCES Clinicas(IdClinica)
);
GO

/* =========================================
   3. USUARIOS
========================================= */
CREATE TABLE Usuarios (
    IdUsuario INT IDENTITY PRIMARY KEY,
    IdClinica INT NOT NULL,

    NombreCompleto NVARCHAR(150),
    Correo NVARCHAR(150),
    ClaveHash NVARCHAR(300),

    Rol NVARCHAR(50),

    Activo BIT DEFAULT 1,
    FechaCreacion DATETIME DEFAULT GETDATE(),

    FOREIGN KEY (IdClinica)
        REFERENCES Clinicas(IdClinica)
);
GO

/* =========================================
   4. PACIENTES
========================================= */
CREATE TABLE Pacientes (
    IdPaciente INT IDENTITY PRIMARY KEY,
    NombreCompleto NVARCHAR(150),
    Telefono NVARCHAR(50),
    FechaRegistro DATETIME DEFAULT GETDATE()
);
GO

/* =========================================
   5. SERVICIOS
========================================= */
CREATE TABLE Servicios (
    IdServicio INT IDENTITY PRIMARY KEY,
    IdClinica INT NOT NULL,

    NombreServicio NVARCHAR(150),
    DuracionMin INT NOT NULL,
    PrecioBase DECIMAL(10,2),

    Activo BIT DEFAULT 1,

    FOREIGN KEY (IdClinica)
        REFERENCES Clinicas(IdClinica)
);
GO

/* =========================================
   6. HORARIOS
========================================= */
CREATE TABLE HorariosSucursal (
    IdHorario INT IDENTITY PRIMARY KEY,
    IdSucursal INT NOT NULL,

    DiaSemana INT,
    HoraInicio TIME,
    HoraFin TIME,

    FOREIGN KEY (IdSucursal)
        REFERENCES Sucursales(IdSucursal)
);
GO

/* =========================================
   7. CITAS
========================================= */
CREATE TABLE Citas (
    IdCita INT IDENTITY PRIMARY KEY,

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

    FOREIGN KEY (IdClinica) REFERENCES Clinicas(IdClinica),
    FOREIGN KEY (IdSucursal) REFERENCES Sucursales(IdSucursal),
    FOREIGN KEY (IdPaciente) REFERENCES Pacientes(IdPaciente),
    FOREIGN KEY (IdDoctor) REFERENCES Usuarios(IdUsuario)
);
GO

/* =========================================
   8. DETALLE SERVICIOS
========================================= */
CREATE TABLE CitasServicios (
    IdDetalle INT IDENTITY PRIMARY KEY,
    IdCita INT NOT NULL,
    IdServicio INT NOT NULL,

    DuracionMin INT,
    Precio DECIMAL(10,2),

    FOREIGN KEY (IdCita)
        REFERENCES Citas(IdCita),

    FOREIGN KEY (IdServicio)
        REFERENCES Servicios(IdServicio)
);
GO
