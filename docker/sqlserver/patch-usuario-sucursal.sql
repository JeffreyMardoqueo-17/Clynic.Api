USE [ClynicDB];
GO

IF COL_LENGTH('Usuario', 'IdSucursal') IS NULL
BEGIN
    ALTER TABLE Usuario ADD IdSucursal INT NULL;
END
GO

IF COL_LENGTH('Usuario', 'DebeCambiarClave') IS NULL
BEGIN
    ALTER TABLE Usuario ADD DebeCambiarClave BIT NOT NULL CONSTRAINT DF_Usuario_DebeCambiarClave DEFAULT (0);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_Usuario_Sucursal'
)
BEGIN
    ALTER TABLE Usuario
    ADD CONSTRAINT FK_Usuario_Sucursal
        FOREIGN KEY (IdSucursal) REFERENCES Sucursal(Id);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Usuario_IdSucursal'
      AND object_id = OBJECT_ID('Usuario')
)
BEGIN
    CREATE INDEX IX_Usuario_IdSucursal ON Usuario(IdSucursal);
END
GO
