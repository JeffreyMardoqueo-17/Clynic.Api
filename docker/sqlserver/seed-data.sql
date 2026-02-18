USE SistemaCitasClinicasSaaS;
GO

PRINT 'Insertando datos semilla...';
GO

/* Clinica */
INSERT INTO Clinicas (Nombre, Telefono, Direccion)
VALUES ('Clínica Demo', '7777-7777', 'San Salvador');
GO

/* Sucursal */
INSERT INTO Sucursales (IdClinica, Nombre, Direccion)
VALUES (1, 'Sucursal Central', 'Centro');
GO

/* Usuario Admin */
INSERT INTO Usuarios (IdClinica, NombreCompleto, Correo, ClaveHash, Rol)
VALUES (1, 'Admin General', 'admin@demo.com', 'admin@demo.com', 'ADMIN');
GO

/* Servicios */
INSERT INTO Servicios (IdClinica, NombreServicio, DuracionMin, PrecioBase)
VALUES 
(1, 'Consulta General', 30, 25.00),
(1, 'Limpieza Dental', 45, 40.00);
GO

PRINT 'Datos semilla insertados.';
GO
