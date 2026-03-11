USE [ClynicDB];
GO

PRINT 'Aplicando patch: usuario-sucursal...';
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Usuario', 'IdSucursal') IS NULL
    BEGIN
        ALTER TABLE Usuario ADD IdSucursal INT NULL;
    END
END
GO

IF OBJECT_ID(N'Sucursal', N'U') IS NOT NULL
   AND OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.foreign_keys
       WHERE name = 'FK_Usuario_Sucursal'
         AND parent_object_id = OBJECT_ID('Usuario')
   )
BEGIN
    ALTER TABLE Usuario WITH NOCHECK
    ADD CONSTRAINT FK_Usuario_Sucursal FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id);
END
GO

IF OBJECT_ID(N'Usuario', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1
       FROM sys.indexes
       WHERE name = 'IX_Usuario_IdSucursal'
         AND object_id = OBJECT_ID('Usuario')
   )
BEGIN
    CREATE INDEX IX_Usuario_IdSucursal ON Usuario(IdSucursal);
END
GO

PRINT 'Patch usuario-sucursal aplicado.';
GO
