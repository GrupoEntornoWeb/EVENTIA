/* =====================================================================
   EVENTIA - Script Completo de Base de Datos
   Proyecto Final DSW I - Cibertec
   Estudiante: Gustavo Daniel Vila Leyva
   Docente: Luis Salvatierra Aquino
   
   Servidor: CHUNAPIOLAS\SQLEXPRESS
   Base de datos: BD_Eventia
   
   Este script recrea la BD desde cero con todas las tablas, restricciones
   e inserts semilla. Ejecutar en SQL Server Management Studio (SSMS).
   ===================================================================== */

USE master;
GO

-- =====================================================================
-- 1. CREAR BASE DE DATOS
-- =====================================================================
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'BD_Eventia')
BEGIN
    ALTER DATABASE BD_Eventia SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BD_Eventia;
END
GO

CREATE DATABASE BD_Eventia;
GO

USE BD_Eventia;
GO

-- =====================================================================
-- 2. CREAR TABLAS
-- =====================================================================

-- 2.1 Usuario
CREATE TABLE Usuario (
    UsuarioId       INT             NOT NULL,
    Nombre          NVARCHAR(100)   NOT NULL,
    Correo          NVARCHAR(100)   NOT NULL,
    ContrasenaHash  NVARCHAR(256)   NOT NULL,
    Telefono        NVARCHAR(20)    NULL,
    TipoPerfil      NVARCHAR(20)    NOT NULL,
    FechaRegistro   DATETIME2       NOT NULL DEFAULT GETDATE(),

    CONSTRAINT PK_Usuario PRIMARY KEY (UsuarioId),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo)
);
GO

-- 2.2 Categoria (solo CategoriaId + Nombre, sin Descripcion)
CREATE TABLE Categoria (
    CategoriaId     INT             NOT NULL IDENTITY(1,1),
    Nombre          NVARCHAR(50)    NOT NULL,

    CONSTRAINT PK_Categoria PRIMARY KEY (CategoriaId)
);
GO

-- 2.3 Cliente (FK 1:1 con Usuario via ClienteId = UsuarioId)
CREATE TABLE Cliente (
    ClienteId       INT             NOT NULL,
    Direccion       NVARCHAR(200)   NULL,

    CONSTRAINT PK_Cliente PRIMARY KEY (ClienteId),
    CONSTRAINT FK_Cliente_Usuario FOREIGN KEY (ClienteId)
        REFERENCES Usuario(UsuarioId)
);
GO

-- 2.4 Proveedor (FK 1:1 con Usuario via ProveedorId = UsuarioId)
CREATE TABLE Proveedor (
    ProveedorId     INT             NOT NULL,
    NombreNegocio   NVARCHAR(100)   NOT NULL,
    Direccion       NVARCHAR(200)   NULL,

    CONSTRAINT PK_Proveedor PRIMARY KEY (ProveedorId),
    CONSTRAINT FK_Proveedor_Usuario FOREIGN KEY (ProveedorId)
        REFERENCES Usuario(UsuarioId)
);
GO

-- 2.5 Articulo
CREATE TABLE Articulo (
    ArticuloId      INT             NOT NULL IDENTITY(1,1),
    ProveedorId     INT             NOT NULL,
    CategoriaId     INT             NOT NULL,
    Nombre          NVARCHAR(100)   NOT NULL,
    Descripcion     NVARCHAR(500)   NULL,
    Precio          DECIMAL(10,2)   NOT NULL,
    CantidadTotal   INT             NOT NULL,
    ImagenUrl       NVARCHAR(300)   NULL,
    Activo          BIT             NOT NULL DEFAULT 1,

    CONSTRAINT PK_Articulo PRIMARY KEY (ArticuloId),
    CONSTRAINT FK_Articulo_Proveedor FOREIGN KEY (ProveedorId)
        REFERENCES Proveedor(ProveedorId),
    CONSTRAINT FK_Articulo_Categoria FOREIGN KEY (CategoriaId)
        REFERENCES Categoria(CategoriaId)
);
GO

-- 2.6 Pedido
CREATE TABLE Pedido (
    PedidoId        INT             NOT NULL IDENTITY(1,1),
    ClienteId       INT             NOT NULL,
    ProveedorId     INT             NOT NULL,
    FechaPedido     DATETIME2       NOT NULL DEFAULT GETDATE(),
    FechaEvento     DATE            NOT NULL,
    Estado          NVARCHAR(20)    NOT NULL DEFAULT 'Reservado',
    MontoTotal      DECIMAL(10,2)   NOT NULL DEFAULT 0,
    DireccionEntrega NVARCHAR(200)  NULL,
    Observaciones   NVARCHAR(500)   NULL,

    CONSTRAINT PK_Pedido PRIMARY KEY (PedidoId),
    CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (ClienteId)
        REFERENCES Cliente(ClienteId),
    CONSTRAINT FK_Pedido_Proveedor FOREIGN KEY (ProveedorId)
        REFERENCES Proveedor(ProveedorId)
);
GO

-- 2.7 DetallePedido
CREATE TABLE DetallePedido (
    DetallePedidoId INT             NOT NULL IDENTITY(1,1),
    PedidoId        INT             NOT NULL,
    ArticuloId      INT             NOT NULL,
    Cantidad        INT             NOT NULL,
    PrecioUnitario  DECIMAL(10,2)   NOT NULL,
    Subtotal        DECIMAL(10,2)   NOT NULL,

    CONSTRAINT PK_DetallePedido PRIMARY KEY (DetallePedidoId),
    CONSTRAINT FK_DetallePedido_Pedido FOREIGN KEY (PedidoId)
        REFERENCES Pedido(PedidoId),
    CONSTRAINT FK_DetallePedido_Articulo FOREIGN KEY (ArticuloId)
        REFERENCES Articulo(ArticuloId)
);
GO

-- 2.8 Pago (un pago por pedido - constraint UNIQUE en PedidoId)
CREATE TABLE Pago (
    PagoId          INT             NOT NULL IDENTITY(1,1),
    PedidoId        INT             NOT NULL,
    MetodoPago      NVARCHAR(30)    NOT NULL,
    MontoPagado     DECIMAL(10,2)   NOT NULL,
    FechaPago       DATETIME2       NOT NULL DEFAULT GETDATE(),
    Estado          NVARCHAR(20)    NOT NULL,

    CONSTRAINT PK_Pago PRIMARY KEY (PagoId),
    CONSTRAINT UQ_Pago_Pedido UNIQUE (PedidoId),
    CONSTRAINT FK_Pago_Pedido FOREIGN KEY (PedidoId)
        REFERENCES Pedido(PedidoId)
);
GO

-- 2.9 Disponibilidad (stock por articulo por fecha, unique en ArticuloId+Fecha)
CREATE TABLE Disponibilidad (
    DisponibilidadId    INT         NOT NULL IDENTITY(1,1),
    ArticuloId          INT         NOT NULL,
    Fecha               DATE        NOT NULL,
    CantidadDisponible  INT         NOT NULL,
    CantidadReservada   INT         NOT NULL DEFAULT 0,

    CONSTRAINT PK_Disponibilidad PRIMARY KEY (DisponibilidadId),
    CONSTRAINT UQ_Disponibilidad_Articulo_Fecha UNIQUE (ArticuloId, Fecha),
    CONSTRAINT FK_Disponibilidad_Articulo FOREIGN KEY (ArticuloId)
        REFERENCES Articulo(ArticuloId)
);
GO

-- =====================================================================
-- 3. INSERTS SEMILLA
-- =====================================================================

-- =====================================================================
-- 3.1 USUARIOS (4)
-- Todos los passwords: Password123!
-- Hash BCrypt: $2a$11$NEZbf2zqFn.9fCBgcuo/HeioR/4DOk/MvhkJzXCAl/0XRXNIDmDhi
-- =====================================================================
SET IDENTITY_INSERT Usuario ON;
GO

INSERT INTO Usuario (UsuarioId, Nombre, Correo, ContrasenaHash, Telefono, TipoPerfil, FechaRegistro) VALUES
(1, 'Carlos Mendoza',  'carlos.mendoza@mail.com',  '$2a$11$NEZbf2zqFn.9fCBgcuo/HeioR/4DOk/MvhkJzXCAl/0XRXNIDmDhi', '999111222', 'Cliente',   '2026-08-19 02:36:09.040'),
(2, 'Ana Garcia',      'ana.garcia@mail.com',      '$2a$11$NEZbf2zqFn.9fCBgcuo/HeioR/4DOk/MvhkJzXCAl/0XRXNIDmDhi', '999333444', 'Cliente',   '2026-08-19 02:36:09.040'),
(3, 'Martin Alonso',   'martin.alonso@mail.com',   '$2a$11$NEZbf2zqFn.9fCBgcuo/HeioR/4DOk/MvhkJzXCAl/0XRXNIDmDhi', '988111222', 'Proveedor', '2026-08-19 02:36:09.040'),
(4, 'Carmen Reyes',    'carmen.reyes@mail.com',    '$2a$11$NEZbf2zqFn.9fCBgcuo/HeioR/4DOk/MvhkJzXCAl/0XRXNIDmDhi', '988777888', 'Proveedor', '2026-08-19 02:36:09.040');

SET IDENTITY_INSERT Usuario OFF;
GO

-- =====================================================================
-- 3.2 CATEGORIAS (8)
-- =====================================================================
SET IDENTITY_INSERT Categoria ON;
GO

INSERT INTO Categoria (CategoriaId, Nombre) VALUES
(1, 'Sillas'),
(2, 'Mesas'),
(3, 'Sonido'),
(4, 'Iluminacion'),
(5, 'Toldos'),
(6, 'Decoracion'),
(7, 'Catering'),
(8, 'Mobiliario');

SET IDENTITY_INSERT Categoria OFF;
GO

-- =====================================================================
-- 3.3 CLIENTES (2)
-- =====================================================================
INSERT INTO Cliente (ClienteId, Direccion) VALUES
(1, 'Av. Las Palmas 123'),
(2, 'Jr. Los Olivos 456');
GO

-- =====================================================================
-- 3.4 PROVEEDORES (2)
-- =====================================================================
INSERT INTO Proveedor (ProveedorId, NombreNegocio, Direccion) VALUES
(3, 'Eventos Martin', 'Av. Industrial 100'),
(4, 'Sonido Carmen',  'Av. Argentina 400');
GO

-- =====================================================================
-- 3.5 ARTICULOS (19)
-- Martin (ProveedorId=3):  1, 3, 7, 9, 11, 12, 14, 16, 18
-- Carmen (ProveedorId=4):  2, 4, 5, 6, 8, 10, 13, 15, 17, 19
-- =====================================================================
SET IDENTITY_INSERT Articulo ON;
GO

INSERT INTO Articulo (ArticuloId, ProveedorId, CategoriaId, Nombre, Descripcion, Precio, CantidadTotal, ImagenUrl, Activo) VALUES
-- === MARTIN (ProveedorId=3) ===
( 1, 3, 1, 'Silla Plegable Blanca',
  'Silla plastico blanco, plegable, resistente hasta 120kg',
  8.00, 200, 'https://placehold.co/400x300/E8E0D4/1A1A1A?text=Silla+Plegable', 1),

( 3, 3, 2, 'Mesa Redonda 1.20m',
  'Mesa redonda 120cm diametro, 8 personas, plegable',
  22.00, 60, 'https://placehold.co/400x300/D1E7DD/1A1A1A?text=Mesa+Redonda', 0),

( 7, 3, 3, 'Microfono Inalambrico',
  'Mic dinamico inalambrico, alcance 50m, bateria 8h',
  35.00, 12, 'https://placehold.co/400x300/6c757d/FFFFFF?text=Mic+Inalambrico', 1),

( 9, 3, 4, 'Guirnalda LED 10m',
  'Cadena LED 10 metros, 100 luces, color warm',
  15.00, 25, 'https://placehold.co/400x300/FFC107/1A1A1A?text=Guirnalda', 1),

(11, 3, 5, 'Toldo Marquesina 3x6m',
  'Estructura metalica con lona blanca impermeable',
  85.00, 8, 'https://placehold.co/400x300/0D6EFD/FFFFFF?text=Marquesina', 1),

(12, 3, 5, 'Carpa 6x6m',
  'Carpa tipo arana, 6x6 metros, lona blanca',
  150.00, 4, 'https://placehold.co/400x300/20C997/FFFFFF?text=Carpa+6x6', 1),

(14, 3, 6, 'Arco de Globos',
  'Arco con 100 globos, colores a elegir, montaje incluido',
  60.00, 10, 'https://placehold.co/400x300/E8752A/FFFFFF?text=Arco+Globos', 1),

(16, 3, 7, 'Cafetera Industrial 30mm',
  'Cafetera de infermer, capacidad 30 tazas',
  45.00, 4, 'https://placehold.co/400x300/6F42C1/FFFFFF?text=Cafetera', 1),

(18, 3, 8, 'Barra Alta 2.40m',
  'Barra para cocteleria, 2.40m largo, plegable',
  70.00, 6, 'https://placehold.co/400x300/1A1A1A/E8752A?text=Barra+Alta', 0),

-- === CARMEN (ProveedorId=4) ===
( 2, 4, 1, 'Silla Tiffany Dorada',
  'Silla estilo tiffany, estructura metal dorado, cojin blanco',
  18.00, 80, 'https://placehold.co/400x300/FFF3CD/1A1A1A?text=Silla+Tiffany', 1),

( 4, 4, 2, 'Mesa Coctelera Alta',
  'Mesa alta para cocteleria, 110cm alto, base metal',
  15.00, 30, 'https://placehold.co/400x300/F8D7DA/1A1A1A?text=Mesa+Coctelera', 1),

( 5, 4, 3, 'Parlante JBL EON 715',
  'Parlante activo 15 pulgadas, 1000W, bluetooth',
  120.00, 10, 'https://placehold.co/400x300/1A1A1A/FFFFFF?text=Parlante+JBL', 1),

( 6, 4, 3, 'Mesa de Mezclas Yamaha',
  'Mesa 16 canales, efectos, entradas XLR',
  95.00, 5, 'https://placehold.co/400x300/333333/FFFFFF?text=Mesa+Mezclas', 1),

( 8, 4, 4, 'LED Par 64 RGB',
  'Foco LED PAR64, RGB, control DMX, 54W',
  40.00, 20, 'https://placehold.co/400x300/E8752A/FFFFFF?text=LED+Par64', 1),

(10, 4, 4, 'Tornillo LED Disco',
  'Efecto luces movil, 120W, bluetooth, control app',
  65.00, 6, 'https://placehold.co/400x300/8B5CF6/FFFFFF?text=Tornillo+LED', 1),

(13, 4, 6, 'Centro de Mesa Floral',
  'Arreglo floral artificial, base dorada, 30cm alto',
  25.00, 40, 'https://placehold.co/400x300/F8D7DA/1A1A1A?text=Centro+Mesa', 1),

(15, 4, 6, 'Cortina de luces 3x2m',
  'Telon de luces LED para foto, 3x2 metros',
  30.00, 8, 'https://placehold.co/400x300/FFC107/1A1A1A?text=Cortina+Luces', 1),

(17, 4, 7, 'Cooler para Bebidas',
  'Cooler 60L, acero inoxidable, tapa abatible',
  20.00, 8, 'https://placehold.co/400x300/17A2B8/FFFFFF?text=Cooler', 1),

(19, 4, 8, 'Sofa Lounge 2 puestos',
  'Sofa modular tapizado, color gris, desmontable',
  55.00, 4, 'https://placehold.co/400x300/ADB5BD/1A1A1A?text=Sofa+Lounge', 1);

SET IDENTITY_INSERT Articulo OFF;
GO

-- =====================================================================
-- 3.6 PEDIDOS (11 pedidos representativos del ciclo de vida)
-- Estados: Reservado, Confirmado, Entregado, Devuelto, Cancelado
-- =====================================================================
SET IDENTITY_INSERT Pedido ON;
GO

INSERT INTO Pedido (PedidoId, ClienteId, ProveedorId, FechaPedido, FechaEvento, Estado, MontoTotal) VALUES
-- Reservados (esperando pago)
( 3, 1, 4, '2026-08-01 10:00:00', '2026-09-15', 'Reservado',  120.00),
( 7, 2, 4, '2026-08-03 14:30:00', '2026-09-20', 'Reservado',   95.00),
(10, 1, 4, '2026-08-05 09:15:00', '2026-10-05', 'Reservado',  300.00),

-- Confirmados (pago registrado, pendiente de entrega)
( 2, 2, 4, '2026-07-20 11:00:00', '2026-08-25', 'Confirmado', 240.00),
( 6, 1, 4, '2026-07-25 16:45:00', '2026-09-01', 'Confirmado', 180.00),
( 9, 2, 3, '2026-08-01 08:20:00', '2026-10-01', 'Confirmado', 170.00),

-- Entregados (proveedor marco entrega)
( 4, 2, 3, '2026-07-15 13:10:00', '2026-09-05', 'Entregado',  350.00),

-- Devueltos (equipo regresado)
( 1, 1, 3, '2026-07-10 10:45:00', '2026-08-15', 'Devuelto',   160.00),
( 8, 1, 3, '2026-07-28 12:00:00', '2026-09-25', 'Devuelto',   210.00),

-- Cancelados (cancelados antes de entrega)
( 5, 2, 3, '2026-07-18 15:30:00', '2026-09-10', 'Cancelado',   85.00),
(11, 1, 3, '2026-08-05 09:30:00', '2026-10-10', 'Cancelado',  250.00);

SET IDENTITY_INSERT Pedido OFF;
GO

-- =====================================================================
-- 3.7 DETALLE PEDIDO (items de cada pedido)
-- =====================================================================
SET IDENTITY_INSERT DetallePedido ON;
GO

INSERT INTO DetallePedido (DetallePedidoId, PedidoId, ArticuloId, Cantidad, PrecioUnitario, Subtotal) VALUES
-- Pedido 1 (Devuelto): 20 sillas plegables
( 1,  1,  1, 20,   8.00,  160.00),
-- Pedido 2 (Confirmado): 10 sillas tiffany + 1 parlante JBL
( 2,  2,  2, 10,  18.00,  180.00),
( 3,  2,  5,  1,  60.00,   60.00),
-- Pedido 3 (Reservado): 15 sillas plegables
( 4,  3,  1, 15,   8.00,  120.00),
-- Pedido 4 (Entregado): 1 carpa + 10 guirnaldas
( 5,  4, 12,  1, 150.00,  150.00),
( 6,  4,  9, 10,  20.00,  200.00),
-- Pedido 5 (Cancelado): 1 toldo marquesina
( 7,  5, 11,  1,  85.00,   85.00),
-- Pedido 6 (Confirmado): 10 sillas tiffany
( 8,  6,  2, 10,  18.00,  180.00),
-- Pedido 7 (Reservado): 1 mesa de mezclas
( 9,  7,  6,  1,  95.00,   95.00),
-- Pedido 8 (Devuelto): 1 barra alta + 2 arcos de globos
(10,  8, 18,  1,  70.00,   70.00),
(11,  8, 14,  2,  70.00,  140.00),
-- Pedido 9 (Confirmado): 5 sillas plegables + 3 LED par
(12,  9,  1,  5,   8.00,   40.00),
(13,  9,  8,  3,  43.33,  130.00),
-- Pedido 10 (Reservado): 10 sillas tiffany + 4 cortinas de luces
(14, 10,  2, 10,  18.00,  180.00),
(15, 10, 15,  4,  30.00,  120.00),
-- Pedido 11 (Cancelado): 1 carpa + 1 toldo
(16, 11, 12,  1, 150.00,  150.00),
(17, 11, 11,  1, 100.00,  100.00);

SET IDENTITY_INSERT DetallePedido OFF;
GO

-- =====================================================================
-- 3.8 PAGOS (solo para pedidos Confirmados, Entregados y Devueltos)
-- Reservado = sin pago, Cancelado = sin pago
-- =====================================================================
SET IDENTITY_INSERT Pago ON;
GO

INSERT INTO Pago (PagoId, PedidoId, MetodoPago, MontoPagado, FechaPago, Estado) VALUES
-- Devueltos
( 1,  1, 'Efectivo',      160.00, '2026-07-10 10:50:00', 'Pagado'),
( 8,  8, 'Efectivo',      210.00, '2026-07-28 12:05:00', 'Pagado'),
-- Confirmados
( 2,  2, 'Yape',          240.00, '2026-07-20 11:05:00', 'Pagado'),
( 6,  6, 'Transferencia', 180.00, '2026-07-25 16:50:00', 'Pagado'),
( 9,  9, 'Yape',          170.00, '2026-08-01 08:25:00', 'Pagado'),
-- Entregados
( 4,  4, 'Efectivo',      350.00, '2026-07-15 13:15:00', 'Pagado');

SET IDENTITY_INSERT Pago OFF;
GO

-- =====================================================================
-- 3.9 DISPONIBILIDAD (stock por articulo y fecha)
-- Stock variado para demostrar los 4 estados visuales del calendario:
--   Disponible (verde):  Disponible > 0, Reservada < Disponible
--   Parcial (amarillo):  Reservada > 0 pero hay stock restante
--   Sin stock (gris):    Disponible = 0, Reservada = 0
--   Bloqueado (rojo):    Disponible = 0, Reservada > 0
-- =====================================================================
SET IDENTITY_INSERT Disponibilidad ON;
GO

-- --- Articulo 1: Silla Plegable Blanca (total 200) ---
-- Septiembre: variado para mostrar todos los colores del calendario
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(  1, 1, '2026-09-01', 200,   0),   -- disponible
(  2, 1, '2026-09-02', 200,   0),   -- disponible
(  3, 1, '2026-09-03', 200,   0),   -- disponible
(  4, 1, '2026-09-04', 200,  50),   -- parcial
(  5, 1, '2026-09-05', 200,   0),   -- disponible
(  6, 1, '2026-09-06', 200,   0),   -- disponible
(  7, 1, '2026-09-07', 200,   0),   -- disponible
(  8, 1, '2026-09-08', 200,   0),   -- disponible
(  9, 1, '2026-09-09', 200, 195),   -- parcial (quedan 5)
( 10, 1, '2026-09-10',   0,   1),   -- bloqueado
( 11, 1, '2026-09-11', 200,  30),   -- parcial
( 12, 1, '2026-09-13', 200,   0),   -- disponible
( 13, 1, '2026-09-14', 200,   0),   -- disponible
( 14, 1, '2026-09-15',   0,   0),   -- sin stock
( 15, 1, '2026-09-17',   0,   1),   -- bloqueado
( 16, 1, '2026-09-18', 200,  80),   -- parcial
( 17, 1, '2026-09-20', 200,   0),   -- disponible
( 18, 1, '2026-09-21', 200,   0),   -- disponible
( 19, 1, '2026-09-22', 200, 150),   -- parcial
( 20, 1, '2026-09-23', 200, 198),   -- parcial (quedan 2)
( 21, 1, '2026-09-24',   0,   0),   -- sin stock
( 22, 1, '2026-09-26', 200,  40),   -- parcial
( 23, 1, '2026-09-27', 200,   0),   -- disponible
( 24, 1, '2026-09-28', 200,   0),   -- disponible
( 25, 1, '2026-09-29', 200,   0),   -- disponible
-- Octubre
( 26, 1, '2026-10-01', 200,   0),   -- disponible
( 27, 1, '2026-10-02', 200,   0),   -- disponible
( 28, 1, '2026-10-03', 200,  60),   -- parcial
( 29, 1, '2026-10-04',   0,   1),   -- bloqueado
( 30, 1, '2026-10-05', 200,   0),   -- disponible
( 31, 1, '2026-10-06', 200,   0),   -- disponible
( 32, 1, '2026-10-07', 200, 100),   -- parcial
( 33, 1, '2026-10-08', 200,  25),   -- parcial
( 34, 1, '2026-10-09',   0,   0),   -- sin stock
( 35, 1, '2026-10-10',   0,   1),   -- bloqueado
( 36, 1, '2026-10-11',   0,   1),   -- bloqueado
( 37, 1, '2026-10-12', 200,   0),   -- disponible
( 38, 1, '2026-10-13', 200,   0),   -- disponible
( 39, 1, '2026-10-14', 200, 140),   -- parcial
( 40, 1, '2026-10-15', 200, 120),   -- parcial
( 41, 1, '2026-10-16', 200, 197),   -- parcial (quedan 3)
( 42, 1, '2026-10-19', 200,   0),   -- disponible
( 43, 1, '2026-10-20', 200,   0),   -- disponible
( 44, 1, '2026-10-21', 200,  55),   -- parcial
( 45, 1, '2026-10-22', 200, 180),   -- parcial
( 46, 1, '2026-10-23',   0,   0),   -- sin stock
( 47, 1, '2026-10-24', 200, 199),   -- parcial (queda 1)
( 48, 1, '2026-10-25',   0,   1),   -- bloqueado
( 49, 1, '2026-10-26', 200,   0),   -- disponible
( 50, 1, '2026-10-27', 200,   0),   -- disponible
( 51, 1, '2026-10-28', 200,  70),   -- parcial
( 52, 1, '2026-10-29', 200,  35),   -- parcial
( 53, 1, '2026-10-30',   0,   0),   -- sin stock
( 54, 1, '2026-10-31',   0,   1),   -- bloqueado
-- Noviembre (stock reducido)
( 55, 1, '2026-11-01', 150,   0),   -- disponible
( 56, 1, '2026-11-15', 100,  10),   -- parcial
( 57, 1, '2026-11-20', 150,  20);   -- parcial

-- --- Articulo 2: Silla Tiffany Dorada (total 80) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 58, 2, '2026-10-01', 80, 0), ( 59, 2, '2026-10-05', 80, 0), ( 60, 2, '2026-10-10', 80, 0),
( 61, 2, '2026-10-15', 80, 0), ( 62, 2, '2026-10-20', 80, 0),
( 63, 2, '2026-11-01', 80, 0), ( 64, 2, '2026-11-15', 80, 0);

-- --- Articulo 3: Mesa Redonda (total 60, desactivada) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 65, 3, '2026-08-20', 60, 0), ( 66, 3, '2026-08-22', 60, 0), ( 67, 3, '2026-08-25', 60, 0),
( 68, 3, '2026-08-28', 60, 0), ( 69, 3, '2026-08-30', 60, 0),
( 70, 3, '2026-11-01', 60, 0), ( 71, 3, '2026-11-15', 60, 0);

-- --- Articulo 4: Mesa Coctelera (total 30) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 72, 4, '2026-10-01', 30, 0), ( 73, 4, '2026-10-05', 30, 0), ( 74, 4, '2026-10-10', 30, 0),
( 75, 4, '2026-10-15', 30, 0), ( 76, 4, '2026-10-20', 30, 0),
( 77, 4, '2026-11-01', 30, 0), ( 78, 4, '2026-11-15', 30, 0);

-- --- Articulo 5: Parlante JBL (total 10) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 79, 5, '2026-10-01', 10, 10),  -- bloqueado (todo reservado)
( 80, 5, '2026-10-05', 10,  0), ( 81, 5, '2026-10-10', 10, 0),
( 82, 5, '2026-10-15', 10,  0), ( 83, 5, '2026-10-20', 10, 0),
( 84, 5, '2026-11-01', 10,  0), ( 85, 5, '2026-11-15', 10, 0);

-- --- Articulo 6: Mesa de Mezclas (total 5) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 86, 6, '2026-10-01', 5, 0), ( 87, 6, '2026-10-05', 5, 0), ( 88, 6, '2026-10-10', 5, 0),
( 89, 6, '2026-10-15', 5, 0), ( 90, 6, '2026-10-20', 5, 0),
( 91, 6, '2026-11-01', 5, 0), ( 92, 6, '2026-11-15', 5, 0);

-- --- Articulo 7: Microfono (total 12) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
( 93, 7, '2026-08-20',  0, 1),  -- bloqueado
( 94, 7, '2026-08-22', 12, 0), ( 95, 7, '2026-08-25', 12, 0),
( 96, 7, '2026-08-28', 12, 0), ( 97, 7, '2026-08-30', 12, 0),
( 98, 7, '2026-11-01', 12, 0), ( 99, 7, '2026-11-15', 12, 0);

-- --- Articulo 8: LED Par 64 (total 20) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(100, 8, '2026-10-01', 20, 0), (101, 8, '2026-10-05', 20, 0), (102, 8, '2026-10-10', 20, 0),
(103, 8, '2026-10-15', 20, 0), (104, 8, '2026-10-20', 20, 0),
(105, 8, '2026-11-01', 20, 0), (106, 8, '2026-11-15', 20, 0);

-- --- Articulo 9: Guirnalda LED (total 25) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(107, 9, '2026-08-20', 25, 0), (108, 9, '2026-08-22', 25, 0), (109, 9, '2026-08-25', 25, 0),
(110, 9, '2026-08-28', 25, 0), (111, 9, '2026-08-30', 25, 0),
(112, 9, '2026-11-01', 25, 0), (113, 9, '2026-11-15', 25, 0),
(114, 9, '2026-12-01',100, 0);

-- --- Articulo 10: Tornillo LED (total 6) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(115, 10, '2026-10-01', 6, 0), (116, 10, '2026-10-05', 6, 0), (117, 10, '2026-10-10', 6, 0),
(118, 10, '2026-10-15', 6, 0), (119, 10, '2026-10-20', 6, 0),
(120, 10, '2026-11-01', 6, 0), (121, 10, '2026-11-15', 6, 0);

-- --- Articulo 11: Toldo Marquesina (total 8) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(122, 11, '2026-08-20', 8, 0), (123, 11, '2026-08-22', 8, 0), (124, 11, '2026-08-25', 8, 0),
(125, 11, '2026-08-28', 8, 0), (126, 11, '2026-08-30', 8, 0),
(127, 11, '2026-11-01', 8, 0), (128, 11, '2026-11-15', 8, 0),
(129, 11, '2026-11-20',100, 1);  -- parcial

-- --- Articulo 12: Carpa 6x6m (total 4) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(130, 12, '2026-08-20', 4, 1),   -- parcial
(131, 12, '2026-08-22', 4, 0), (132, 12, '2026-08-25', 0, 0),  -- sin stock
(133, 12, '2026-08-28', 4, 0), (134, 12, '2026-08-30', 4, 0),
(135, 12, '2026-11-01', 4, 0), (136, 12, '2026-11-15', 4, 0),
(137, 12, '2026-12-10',100, 0);

-- --- Articulo 13: Centro de Mesa Floral (total 40) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(138, 13, '2026-10-01', 40, 0), (139, 13, '2026-10-05', 40, 0), (140, 13, '2026-10-10', 40, 0),
(141, 13, '2026-10-15', 40, 0), (142, 13, '2026-10-20', 40, 0),
(143, 13, '2026-11-01', 40, 0), (144, 13, '2026-11-15', 40, 0);

-- --- Articulo 14: Arco de Globos (total 10) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(145, 14, '2026-08-20',  0, 0),  -- sin stock
(146, 14, '2026-08-22',  0, 0),  -- sin stock
(147, 14, '2026-08-25', 10, 0), (148, 14, '2026-08-28', 10, 0), (149, 14, '2026-08-30', 10, 0),
(150, 14, '2026-11-01', 10, 0), (151, 14, '2026-11-15', 10, 0);

-- --- Articulo 15: Cortina de luces (total 8) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(152, 15, '2026-10-01', 8, 0), (153, 15, '2026-10-05', 8, 0), (154, 15, '2026-10-10', 8, 0),
(155, 15, '2026-10-15', 8, 0), (156, 15, '2026-10-20', 8, 0),
(157, 15, '2026-11-01', 8, 0), (158, 15, '2026-11-15', 8, 0);

-- --- Articulo 16: Cafetera Industrial (total 4) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(159, 16, '2026-08-20', 4, 0), (160, 16, '2026-08-22', 4, 0), (161, 16, '2026-08-25', 4, 1),  -- parcial
(162, 16, '2026-08-28', 4, 0), (163, 16, '2026-08-30', 4, 0),
(164, 16, '2026-11-01', 4, 0), (165, 16, '2026-11-15', 4, 0);

-- --- Articulo 17: Cooler para Bebidas (total 8) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(166, 17, '2026-10-01', 8, 0), (167, 17, '2026-10-05', 8, 0), (168, 17, '2026-10-10', 8, 0),
(169, 17, '2026-10-15', 8, 0), (170, 17, '2026-10-20', 8, 0),
(171, 17, '2026-11-01', 8, 0), (172, 17, '2026-11-15', 8, 0);

-- --- Articulo 18: Barra Alta (total 6, desactivada) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(173, 18, '2026-08-20', 6, 0), (174, 18, '2026-08-22', 6, 0), (175, 18, '2026-08-25', 6, 0),
(176, 18, '2026-08-28', 6, 0), (177, 18, '2026-08-30', 6, 0),
(178, 18, '2026-11-01', 6, 0), (179, 18, '2026-11-15', 6, 0);

-- --- Articulo 19: Sofa Lounge (total 4) ---
INSERT INTO Disponibilidad (DisponibilidadId, ArticuloId, Fecha, CantidadDisponible, CantidadReservada) VALUES
(180, 19, '2026-10-01', 4, 0), (181, 19, '2026-10-05', 4, 0), (182, 19, '2026-10-10', 4, 0),
(183, 19, '2026-10-15', 4, 0), (184, 19, '2026-10-20', 4, 0),
(185, 19, '2026-11-01', 4, 0), (186, 19, '2026-11-15', 4, 0);

SET IDENTITY_INSERT Disponibilidad OFF;
GO

-- =====================================================================
-- 4. VERIFICACION FINAL
-- =====================================================================
PRINT '================================================================='
PRINT '  EVENTIA - BD_Eventia creada exitosamente'
PRINT '================================================================='
PRINT ''

PRINT 'Tablas creadas:'
SELECT TABLE_NAME,
       (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = t.TABLE_NAME) AS Columnas
FROM INFORMATION_SCHEMA.TABLES t
WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME != 'sysdiagrams'
ORDER BY TABLE_NAME;

PRINT ''
PRINT 'Registros por tabla:'
SELECT 'Usuario' AS Tabla, COUNT(*) AS Registros FROM Usuario
UNION ALL SELECT 'Categoria',    COUNT(*) FROM Categoria
UNION ALL SELECT 'Cliente',      COUNT(*) FROM Cliente
UNION ALL SELECT 'Proveedor',    COUNT(*) FROM Proveedor
UNION ALL SELECT 'Articulo',     COUNT(*) FROM Articulo
UNION ALL SELECT 'Pedido',       COUNT(*) FROM Pedido
UNION ALL SELECT 'DetallePedido', COUNT(*) FROM DetallePedido
UNION ALL SELECT 'Pago',         COUNT(*) FROM Pago
UNION ALL SELECT 'Disponibilidad', COUNT(*) FROM Disponibilidad;

PRINT ''
PRINT 'Credenciales de prueba:'
PRINT '  Cliente:   carlos.mendoza@mail.com  / Password123!'
PRINT '  Cliente:   ana.garcia@mail.com      / Password123!'
PRINT '  Proveedor: martin.alonso@mail.com   / Password123!'
PRINT '  Proveedor: carmen.reyes@mail.com    / Password123!'
PRINT ''
PRINT 'Articulos desactivados (Activo=0): Mesa Redonda (3), Barra Alta (18)'
PRINT ''
PRINT 'Pedidos por estado:'
SELECT Estado, COUNT(*) AS Cantidad FROM Pedido GROUP BY Estado ORDER BY Estado;
PRINT ''
PRINT 'Connection string para appsettings.json:'
PRINT '  Server=CHUNAPIOLAS\SQLEXPRESS;Database=BD_Eventia;Trusted_Connection=True;TrustServerCertificate=True;'
GO
