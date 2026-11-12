/* =====================================================================
   BiblioTech - Script de base de datos
   Ejecutar completo en SSMS (F5). Borra y recrea la base de datos.
   ===================================================================== */

-------------------------------------------------------------------------
-- 0. CREAR LA BASE DE DATOS (si existe, se elimina y se crea de nuevo)
--    (En Azure SQL esta sección NO se ejecuta: ver BiblioTech.Azure.sql)
-------------------------------------------------------------------------
USE master;
GO

IF DB_ID('BiblioTech') IS NOT NULL
BEGIN
    -- Cierra conexiones abiertas para poder borrarla
    ALTER DATABASE BiblioTech SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BiblioTech;
END
GO

CREATE DATABASE BiblioTech;
GO

USE BiblioTech;
GO

-------------------------------------------------------------------------
-- 1. TABLAS
-------------------------------------------------------------------------

-- 1.1 AUTORES: tabla simple
CREATE TABLE dbo.Autores
(
    Id            INT IDENTITY(1,1) NOT NULL,
    Nombre        NVARCHAR(100)     NOT NULL,
    Nacionalidad  NVARCHAR(60)      NULL,

    CONSTRAINT PK_Autores PRIMARY KEY (Id)
);
GO

-- 1.2 LIBROS: con reglas de integridad sobre los ejemplares
CREATE TABLE dbo.Libros
(
    Id                     INT IDENTITY(1,1) NOT NULL,
    Titulo                 NVARCHAR(200)     NOT NULL,
    ISBN                   NVARCHAR(20)      NOT NULL,
    AnioPublicacion        INT               NULL,
    EjemplaresTotales      INT               NOT NULL,
    EjemplaresDisponibles  INT               NOT NULL,

    CONSTRAINT PK_Libros PRIMARY KEY (Id),
    CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
    CONSTRAINT CK_Libros_Totales CHECK (EjemplaresTotales > 0),
    -- Nunca negativos ni más disponibles que los que existen
    CONSTRAINT CK_Libros_Disponibles CHECK (EjemplaresDisponibles BETWEEN 0 AND EjemplaresTotales),
    CONSTRAINT CK_Libros_Anio CHECK (AnioPublicacion IS NULL OR AnioPublicacion BETWEEN 1450 AND 2100)
);
GO

-- 1.3 LIBROAUTOR: tabla puente para la relación N:N
CREATE TABLE dbo.LibroAutor
(
    LibroId  INT NOT NULL,
    AutorId  INT NOT NULL,

    -- PK compuesta: la misma pareja no se puede repetir
    CONSTRAINT PK_LibroAutor PRIMARY KEY (LibroId, AutorId),
    CONSTRAINT FK_LibroAutor_Libros  FOREIGN KEY (LibroId) REFERENCES dbo.Libros(Id)  ON DELETE CASCADE,
    CONSTRAINT FK_LibroAutor_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores(Id)
);
GO

-- 1.4 SOCIOS: personas inscritas, con borrado lógico (Activo)
CREATE TABLE dbo.Socios
(
    Id             INT IDENTITY(1,1) NOT NULL,
    Documento      NVARCHAR(20)      NOT NULL,
    Nombre         NVARCHAR(120)     NOT NULL,
    Email          NVARCHAR(150)     NOT NULL,
    FechaRegistro  DATETIME2         NOT NULL CONSTRAINT DF_Socios_FechaRegistro DEFAULT SYSDATETIME(),
    Activo         BIT               NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT 1,

    CONSTRAINT PK_Socios PRIMARY KEY (Id),
    CONSTRAINT UQ_Socios_Documento UNIQUE (Documento),
    CONSTRAINT UQ_Socios_Email UNIQUE (Email),
    CONSTRAINT CK_Socios_Email CHECK (Email LIKE '%_@_%._%')
);
GO

-- 1.5 PRESTAMOS: relaciona un libro con un socio (1:N con cada uno)
CREATE TABLE dbo.Prestamos
(
    Id                       INT IDENTITY(1,1) NOT NULL,
    LibroId                  INT               NOT NULL,
    SocioId                  INT               NOT NULL,
    FechaPrestamo            DATETIME2         NOT NULL CONSTRAINT DF_Prestamos_FechaPrestamo DEFAULT SYSDATETIME(),
    FechaDevolucionEsperada  DATETIME2         NOT NULL,
    FechaDevolucionReal      DATETIME2         NULL,      -- NULL = todavía no se devuelve
    Estado                   NVARCHAR(10)      NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT 'Activo',

    CONSTRAINT PK_Prestamos PRIMARY KEY (Id),
    CONSTRAINT FK_Prestamos_Libros FOREIGN KEY (LibroId) REFERENCES dbo.Libros(Id),
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios(Id),

    -- Solo se guardan estos dos estados; "Vencido" se CALCULA
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN ('Activo', 'Devuelto')),
    CONSTRAINT CK_Prestamos_FechaEsperada CHECK (FechaDevolucionEsperada > FechaPrestamo),
    CONSTRAINT CK_Prestamos_FechaReal CHECK (FechaDevolucionReal IS NULL OR FechaDevolucionReal >= FechaPrestamo),
    -- Coherencia: Activo sin fecha real; Devuelto con fecha real
    CONSTRAINT CK_Prestamos_Coherencia CHECK (
        (Estado = 'Activo'   AND FechaDevolucionReal IS NULL) OR
        (Estado = 'Devuelto' AND FechaDevolucionReal IS NOT NULL)
    )
);
GO

-- 1.6 USUARIOS: login del bibliotecario (JWT, día 3)
CREATE TABLE dbo.Usuarios
(
    Id            INT IDENTITY(1,1) NOT NULL,
    Email         NVARCHAR(150)     NOT NULL,
    PasswordHash  NVARCHAR(255)     NOT NULL,   -- nunca la contraseña en texto plano
    Rol           NVARCHAR(20)      NOT NULL CONSTRAINT DF_Usuarios_Rol DEFAULT 'Bibliotecario',

    CONSTRAINT PK_Usuarios PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuarios_Email UNIQUE (Email)
);
GO

-------------------------------------------------------------------------
-- 2. ÍNDICES (las FK no crean índice automáticamente en SQL Server)
-------------------------------------------------------------------------
CREATE INDEX IX_Prestamos_SocioId_Estado ON dbo.Prestamos (SocioId, Estado); -- reglas R2 y R3
CREATE INDEX IX_Prestamos_LibroId        ON dbo.Prestamos (LibroId);
CREATE INDEX IX_LibroAutor_AutorId       ON dbo.LibroAutor (AutorId);         -- la PK ya cubre LibroId
CREATE INDEX IX_Libros_Titulo            ON dbo.Libros (Titulo);              -- búsqueda
GO

-------------------------------------------------------------------------
-- 3. VISTA: préstamos vencidos (estado calculado)
-------------------------------------------------------------------------
CREATE VIEW dbo.vw_PrestamosVencidos
AS
SELECT
    p.Id                        AS PrestamoId,
    l.Titulo,
    s.Nombre                    AS Socio,
    s.Email,
    p.FechaPrestamo,
    p.FechaDevolucionEsperada,
    DATEDIFF(DAY, p.FechaDevolucionEsperada, SYSDATETIME()) AS DiasRetraso
FROM dbo.Prestamos p
INNER JOIN dbo.Libros  l ON l.Id = p.LibroId
INNER JOIN dbo.Socios  s ON s.Id = p.SocioId
WHERE p.Estado = 'Activo'
  AND p.FechaDevolucionEsperada < SYSDATETIME();
GO

-------------------------------------------------------------------------
-- 4. PROCEDIMIENTO ALMACENADO: resumen para el dashboard
-------------------------------------------------------------------------
CREATE PROCEDURE dbo.sp_ResumenDashboard
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*)                              FROM dbo.Libros)                            AS TotalLibros,
        (SELECT ISNULL(SUM(EjemplaresDisponibles), 0) FROM dbo.Libros)                            AS EjemplaresDisponibles,
        (SELECT COUNT(*)                              FROM dbo.Prestamos WHERE Estado = 'Activo') AS PrestamosActivos,
        (SELECT COUNT(*)                              FROM dbo.vw_PrestamosVencidos)              AS PrestamosVencidos,
        (SELECT COUNT(*)                              FROM dbo.Socios WHERE Activo = 1)           AS SociosActivos;
END
GO

-------------------------------------------------------------------------
-- 5. DATOS SEMILLA
-------------------------------------------------------------------------

-- 5.1 Autores (Id 1 a 6)
INSERT INTO dbo.Autores (Nombre, Nacionalidad) VALUES
('Gabriel García Márquez', 'Colombiana'),     -- 1
('Isabel Allende',         'Chilena'),        -- 2
('Mario Vargas Llosa',     'Peruana'),        -- 3
('Robert C. Martin',       'Estadounidense'), -- 4
('Martin Fowler',          'Británica'),      -- 5
('Kent Beck',              'Estadounidense'); -- 6

-- 5.2 Libros (Id 1 a 10). Disponibles = Totales - préstamos activos
INSERT INTO dbo.Libros (Titulo, ISBN, AnioPublicacion, EjemplaresTotales, EjemplaresDisponibles) VALUES
('Cien años de soledad',              '978-0307474728', 1967, 3, 2), -- 1 (1 activo)
('El amor en los tiempos del cólera', '978-0307387264', 1985, 2, 2), -- 2
('La casa de los espíritus',          '978-1501117015', 1982, 2, 1), -- 3 (1 activo)
('La ciudad y los perros',            '978-8420412146', 1963, 1, 1), -- 4
('Clean Code',                        '978-0132350884', 2008, 3, 2), -- 5 (1 activo)
('Refactoring',                       '978-0134757599', 2018, 2, 1), -- 6 (1 activo)
('Test Driven Development',           '978-0321146533', 2002, 1, 0), -- 7 (0 disponibles -> R1)
('Clean Architecture',                '978-0134494166', 2017, 2, 2), -- 8
('Crónica de una muerte anunciada',   '978-1400034710', 1981, 2, 2), -- 9
('Paula',                             '978-0060927219', 1994, 1, 0); -- 10 (1 activo)

-- 5.3 Relación N:N libro-autor
INSERT INTO dbo.LibroAutor (LibroId, AutorId) VALUES
(1, 1), (2, 1), (9, 1),       -- García Márquez
(3, 2), (10, 2),              -- Allende
(4, 3),                       -- Vargas Llosa
(5, 4), (8, 4),               -- Robert C. Martin
(6, 5), (6, 6),               -- Refactoring: DOS autores (Fowler y Beck)
(7, 6);                       -- Kent Beck

-- 5.4 Socios (Id 1 a 5)
INSERT INTO dbo.Socios (Documento, Nombre, Email, Activo) VALUES
('1010101010', 'Ana Gómez',      'ana.gomez@ejemplo.com',      1), -- 1: 3 activos (R2)
('2020202020', 'Carlos Ruiz',    'carlos.ruiz@ejemplo.com',    1), -- 2: tiene vencido (R3)
('3030303030', 'Laura Martínez', 'laura.martinez@ejemplo.com', 1), -- 3: 1 activo al día
('4040404040', 'Pedro Sánchez',  'pedro.sanchez@ejemplo.com',  1), -- 4: sin préstamos (camino feliz)
('5050505050', 'Sofía Herrera',  'sofia.herrera@ejemplo.com',  0); -- 5: inactiva (R6)

-- 5.5 Préstamos (fechas relativas a HOY)
DECLARE @Hoy DATETIME2 = SYSDATETIME();

INSERT INTO dbo.Prestamos (LibroId, SocioId, FechaPrestamo, FechaDevolucionEsperada, FechaDevolucionReal, Estado) VALUES
-- Activos al día
(5,  1, DATEADD(DAY,  -2, @Hoy), DATEADD(DAY,  13, @Hoy), NULL, 'Activo'),   -- Ana
(6,  1, DATEADD(DAY,  -3, @Hoy), DATEADD(DAY,  12, @Hoy), NULL, 'Activo'),   -- Ana
(1,  1, DATEADD(DAY,  -5, @Hoy), DATEADD(DAY,  10, @Hoy), NULL, 'Activo'),   -- Ana (ya tiene 3)
(3,  3, DATEADD(DAY,  -1, @Hoy), DATEADD(DAY,  14, @Hoy), NULL, 'Activo'),   -- Laura
-- Activos VENCIDOS (aparecen en la vista)
(7,  2, DATEADD(DAY, -20, @Hoy), DATEADD(DAY,  -5, @Hoy), NULL, 'Activo'),   -- Carlos, 5 días de retraso
(10, 5, DATEADD(DAY, -25, @Hoy), DATEADD(DAY, -10, @Hoy), NULL, 'Activo'),   -- Sofía, 10 días de retraso
-- Historial devuelto
(1,  3, DATEADD(DAY, -40, @Hoy), DATEADD(DAY, -25, @Hoy), DATEADD(DAY, -30, @Hoy), 'Devuelto'),
(4,  4, DATEADD(DAY, -30, @Hoy), DATEADD(DAY, -15, @Hoy), DATEADD(DAY, -20, @Hoy), 'Devuelto'),
(2,  2, DATEADD(DAY, -60, @Hoy), DATEADD(DAY, -45, @Hoy), DATEADD(DAY, -40, @Hoy), 'Devuelto');
GO

-- 5.6 Usuarios: VACÍA a propósito.
-- El usuario bibliotecario se crea el día 3 desde la API, porque el hash
-- de la contraseña lo genera BCrypt en C#.

-------------------------------------------------------------------------
-- 6. CONSULTAS DE VERIFICACIÓN (para explicar en clase)
-------------------------------------------------------------------------

-- Libros con sus autores (JOIN a través de la tabla puente)
SELECT l.Titulo, a.Nombre AS Autor
FROM dbo.Libros l
INNER JOIN dbo.LibroAutor la ON la.LibroId = l.Id
INNER JOIN dbo.Autores    a  ON a.Id = la.AutorId
ORDER BY l.Titulo;

-- Préstamos activos por socio (base de la regla R2)
SELECT s.Nombre, COUNT(p.Id) AS PrestamosActivos
FROM dbo.Socios s
LEFT JOIN dbo.Prestamos p ON p.SocioId = s.Id AND p.Estado = 'Activo'
GROUP BY s.Nombre;

-- La vista de vencidos
SELECT * FROM dbo.vw_PrestamosVencidos;

-- El procedimiento del dashboard
EXEC dbo.sp_ResumenDashboard;
GO

/* =====================================================================
   7. PRUEBAS QUE DEBEN FALLAR (descomentar UNA a la vez en clase)
   ===================================================================== */
-- ISBN duplicado -> viola UQ_Libros_ISBN
-- INSERT INTO dbo.Libros (Titulo, ISBN, EjemplaresTotales, EjemplaresDisponibles) VALUES ('Copia', '978-0132350884', 1, 1);

-- Disponibles negativos -> viola CK_Libros_Disponibles
-- UPDATE dbo.Libros SET EjemplaresDisponibles = -1 WHERE Id = 1;

-- Estado inválido -> viola CK_Prestamos_Estado
-- UPDATE dbo.Prestamos SET Estado = 'Perdido' WHERE Id = 1;

-- Devuelto sin fecha real -> viola CK_Prestamos_Coherencia
-- UPDATE dbo.Prestamos SET Estado = 'Devuelto' WHERE Id = 1;

-- Borrar un libro con préstamos -> viola FK_Prestamos_Libros
-- DELETE FROM dbo.Libros WHERE Id = 5;

-- Préstamo de un socio que no existe -> viola FK_Prestamos_Socios
-- INSERT INTO dbo.Prestamos (LibroId, SocioId, FechaDevolucionEsperada) VALUES (1, 999, DATEADD(DAY, 15, SYSDATETIME()));
