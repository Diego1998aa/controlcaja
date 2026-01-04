-- Base de Datos para Sistema POS
-- SQLite Database Schema

-- Tabla de Usuarios
CREATE TABLE IF NOT EXISTS Usuarios (
    id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_usuario TEXT NOT NULL UNIQUE,
    contraseña TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    rol TEXT NOT NULL CHECK(rol IN ('Cajero', 'Supervisor')),
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    activo INTEGER DEFAULT 1,
    ultimo_acceso DATETIME
);

-- Tabla de Categorías
CREATE TABLE IF NOT EXISTS Categorias (
    id_categoria INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre_categoria TEXT NOT NULL UNIQUE,
    descripcion TEXT,
    activo INTEGER DEFAULT 1
);

-- Tabla de Productos
CREATE TABLE IF NOT EXISTS Productos (
    id_producto INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo_barras TEXT UNIQUE,
    sku TEXT UNIQUE,
    nombre TEXT NOT NULL,
    descripcion TEXT,
    precio_compra REAL NOT NULL DEFAULT 0,
    precio_venta REAL NOT NULL,
    stock_actual INTEGER NOT NULL DEFAULT 0,
    stock_minimo INTEGER DEFAULT 5,
    id_categoria INTEGER,
    activo INTEGER DEFAULT 1,
    fecha_creacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME,
    FOREIGN KEY (id_categoria) REFERENCES Categorias(id_categoria)
);

-- Tabla de Ventas
CREATE TABLE IF NOT EXISTS Ventas (
    id_venta INTEGER PRIMARY KEY AUTOINCREMENT,
    id_usuario_cajero INTEGER NOT NULL,
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    subtotal REAL NOT NULL,
    iva REAL DEFAULT 0,
    descuento REAL DEFAULT 0,
    total REAL NOT NULL,
    metodo_pago TEXT NOT NULL CHECK(metodo_pago IN ('Efectivo', 'Tarjeta', 'Transferencia', 'Mixto')),
    monto_recibido REAL,
    monto_cambio REAL,
    estado TEXT DEFAULT 'Completada' CHECK(estado IN ('Completada', 'Cancelada')),
    FOREIGN KEY (id_usuario_cajero) REFERENCES Usuarios(id_usuario)
);

-- Tabla de Detalle de Venta
CREATE TABLE IF NOT EXISTS Detalle_Venta (
    id_detalle INTEGER PRIMARY KEY AUTOINCREMENT,
    id_venta INTEGER NOT NULL,
    id_producto INTEGER NOT NULL,
    cantidad INTEGER NOT NULL,
    precio_unitario REAL NOT NULL,
    subtotal REAL NOT NULL,
    FOREIGN KEY (id_venta) REFERENCES Ventas(id_venta) ON DELETE CASCADE,
    FOREIGN KEY (id_producto) REFERENCES Productos(id_producto)
);

-- Tabla de Movimientos de Inventario
CREATE TABLE IF NOT EXISTS Movimientos_Inventario (
    id_movimiento INTEGER PRIMARY KEY AUTOINCREMENT,
    id_producto INTEGER NOT NULL,
    tipo_movimiento TEXT NOT NULL CHECK(tipo_movimiento IN ('Entrada', 'Salida', 'Ajuste', 'Venta')),
    cantidad INTEGER NOT NULL,
    stock_anterior INTEGER NOT NULL,
    stock_nuevo INTEGER NOT NULL,
    motivo TEXT,
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    id_usuario INTEGER NOT NULL,
    FOREIGN KEY (id_producto) REFERENCES Productos(id_producto),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);

-- Tabla de Proveedores (opcional pero útil)
CREATE TABLE IF NOT EXISTS Proveedores (
    id_proveedor INTEGER PRIMARY KEY AUTOINCREMENT,
    nombre TEXT NOT NULL,
    contacto TEXT,
    telefono TEXT,
    email TEXT,
    direccion TEXT,
    activo INTEGER DEFAULT 1
);

-- Tabla de Compras (para recepción de mercancía)
CREATE TABLE IF NOT EXISTS Compras (
    id_compra INTEGER PRIMARY KEY AUTOINCREMENT,
    id_proveedor INTEGER,
    fecha_hora DATETIME DEFAULT CURRENT_TIMESTAMP,
    total REAL NOT NULL,
    estado TEXT DEFAULT 'Recibida',
    id_usuario INTEGER NOT NULL,
    notas TEXT,
    FOREIGN KEY (id_proveedor) REFERENCES Proveedores(id_proveedor),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);

-- Tabla de Detalle de Compras
CREATE TABLE IF NOT EXISTS Detalle_Compra (
    id_detalle_compra INTEGER PRIMARY KEY AUTOINCREMENT,
    id_compra INTEGER NOT NULL,
    id_producto INTEGER NOT NULL,
    cantidad INTEGER NOT NULL,
    precio_compra REAL NOT NULL,
    subtotal REAL NOT NULL,
    FOREIGN KEY (id_compra) REFERENCES Compras(id_compra) ON DELETE CASCADE,
    FOREIGN KEY (id_producto) REFERENCES Productos(id_producto)
);

-- Tabla de Cortes de Caja
CREATE TABLE IF NOT EXISTS Cortes_Caja (
    id_corte INTEGER PRIMARY KEY AUTOINCREMENT,
    id_usuario INTEGER NOT NULL,
    fecha_hora_inicio DATETIME NOT NULL,
    fecha_hora_cierre DATETIME DEFAULT CURRENT_TIMESTAMP,
    total_efectivo REAL DEFAULT 0,
    total_tarjeta REAL DEFAULT 0,
    total_transferencia REAL DEFAULT 0,
    total_ventas REAL NOT NULL,
    numero_ventas INTEGER NOT NULL,
    fondo_inicial REAL DEFAULT 0,
    observaciones TEXT,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);

-- Índices para mejorar el rendimiento
CREATE INDEX idx_productos_codigo ON Productos(codigo_barras);
CREATE INDEX idx_productos_sku ON Productos(sku);
CREATE INDEX idx_productos_nombre ON Productos(nombre);
CREATE INDEX idx_ventas_fecha ON Ventas(fecha_hora);
CREATE INDEX idx_ventas_cajero ON Ventas(id_usuario_cajero);
CREATE INDEX idx_movimientos_fecha ON Movimientos_Inventario(fecha_hora);

-- Datos iniciales: Usuario administrador por defecto
-- Contraseña: "admin123" (debe cambiarse en producción)
INSERT INTO Usuarios (nombre_usuario, contraseña, nombre_completo, rol) 
VALUES ('admin', '$2a$11$xKZqKxLvfNvPvQgXvZ5V5.E8YvqJqVxKZlY3ZQ9qOm5fKpE7dKqLG', 'Administrador', 'Supervisor');

-- Categorías de ejemplo
INSERT INTO Categorias (nombre_categoria, descripcion) VALUES 
('Abarrotes', 'Productos de despensa y consumo básico'),
('Bebidas', 'Refrescos, jugos y bebidas'),
('Lácteos', 'Productos lácteos refrigerados'),
('Panadería', 'Pan y productos de panadería'),
('Limpieza', 'Productos de limpieza y hogar'),
('Botanas', 'Frituras y snacks'),
('Otros', 'Productos varios');