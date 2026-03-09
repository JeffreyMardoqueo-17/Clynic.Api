USE [ClynicDB];
GO

PRINT 'Insertando datos semilla...';
GO

IF OBJECT_ID(N'Rol', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Rol WHERE LOWER(Nombre) = 'admin')
BEGIN
    INSERT INTO Rol (IdClinica, IdSucursal, Nombre, Descripcion, Activo)
    VALUES (NULL, NULL, 'Admin', 'Usuario global administrador', 1);
END
GO

IF OBJECT_ID(N'Especialidad', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM Especialidad WHERE LOWER(Nombre) = 'encargado global')
BEGIN
    INSERT INTO Especialidad (IdClinica, Nombre, Descripcion, Activa)
    VALUES (NULL, 'Encargado Global', 'Especialidad global para administradores', 1);
END
GO

DECLARE @IdRolAdmin INT;
DECLARE @IdEspecialidadAdmin INT;

IF OBJECT_ID(N'Rol', N'U') IS NOT NULL
BEGIN
    SELECT TOP 1 @IdRolAdmin = Id
    FROM Rol
    WHERE LOWER(Nombre) = 'admin';
END

IF OBJECT_ID(N'Especialidad', N'U') IS NOT NULL
BEGIN
    SELECT TOP 1 @IdEspecialidadAdmin = Id
    FROM Especialidad
    WHERE LOWER(Nombre) = 'encargado global';
END

IF @IdRolAdmin IS NOT NULL
   AND @IdEspecialidadAdmin IS NOT NULL
   AND OBJECT_ID(N'RolEspecialidad', N'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1
        FROM RolEspecialidad
        WHERE IdRol = @IdRolAdmin
          AND IdEspecialidad = @IdEspecialidadAdmin
   )
BEGIN
    INSERT INTO RolEspecialidad (IdRol, IdEspecialidad, Activa)
    VALUES (@IdRolAdmin, @IdEspecialidadAdmin, 1);
END
GO

PRINT 'Datos semilla listos: Rol Admin + Especialidad Encargado Global + RolEspecialidad.';
GO
