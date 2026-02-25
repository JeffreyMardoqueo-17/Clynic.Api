USE [ClynicDB];
GO

PRINT 'Insertando datos semilla...';
GO

/* Insertar Clínicas */
INSERT INTO Clinica (Nombre, Telefono, Direccion, Activa, FechaCreacion)
VALUES 
    ('Clínica Central', '123-456-7890', 'Av. Principal 100', 1, GETDATE()),
    ('Clínica Norte', '123-456-7891', 'Calle Norte 200', 1, GETDATE()),
    ('Clínica Sur', '123-456-7892', 'Av. Sur 300', 1, GETDATE());
GO

/* Insertar Sucursales */
INSERT INTO Sucursal (IdClinica, Nombre, Direccion, Activa)
VALUES 
    (1, 'Sucursal Centro', 'Centro Comercial 1', 1),
    (1, 'Sucursal Este', 'Zona Este', 1),
    (2, 'Sucursal Principal', 'Av. Principal 200', 1);
GO

-- /* Insertar Usuarios */
-- INSERT INTO Usuario (IdClinica, NombreCompleto, Correo, ClaveHash, Rol, Activo, FechaCreacion)
-- VALUES 
--     (1, 'Administrador General', 'admin@clinica1.com', 'hashed_password_123', 'ADMIN', 1, GETDATE()),
--     (1, 'Dr. Juan Pérez', 'juan@clinica1.com', 'hashed_password_456', 'DOCTOR', 1, GETDATE()),
--     (2, 'Administrador Clínica 2', 'admin@clinica2.com', 'hashed_password_789', 'ADMIN', 1, GETDATE());
-- GO

PRINT '✅ Datos semilla insertados correctamente.';
GO

