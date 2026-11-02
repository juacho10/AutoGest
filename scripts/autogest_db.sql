-- ============================================
-- Base de datos: Autogest_db
-- ============================================

SET FOREIGN_KEY_CHECKS = 0;

-- --------------------------------------------
-- Tabla: __efmigrationshistory
-- --------------------------------------------
DROP TABLE IF EXISTS `__efmigrationshistory`;
CREATE TABLE `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `__efmigrationshistory` VALUES ('20261027215020_InitialCreate','8.0.8');

-- --------------------------------------------
-- Tabla: clientes
-- --------------------------------------------
DROP TABLE IF EXISTS `clientes`;
CREATE TABLE `clientes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Dni` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Nombre` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Apellido` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Telefono` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Direccion` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Clientes_Dni` (`Dni`),
  UNIQUE KEY `IX_Clientes_Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `clientes` VALUES
(1,'30123456','Juan','Pérez','1151234567','juan.perez@email.com','Av. Rivadavia 1234','2026-10-27 18:55:03',NULL,1),
(2,'30234567','María','Gómez','1152345678','maria.gomez@email.com','Belgrano 567','2026-10-27 18:55:03',NULL,1),
(3,'30345678','Carlos','López','1153456789','carlos.lopez@email.com','San Martín 890','2026-10-27 18:55:03',NULL,1);

-- --------------------------------------------
-- Tabla: usuarios
-- --------------------------------------------
DROP TABLE IF EXISTS `usuarios`;
CREATE TABLE `usuarios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Apellido` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Email` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Avatar` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Rol` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Usuarios_Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `usuarios` VALUES
(1,'Admin','Sistema','admin@autogest.com','$2a$11$RQEYDf3wyMWeM3MM.W7a3eMc4.3r9hpzS79S0bHR6Pfubtqq8kbDa','','Administrador','2026-10-27 18:55:00',NULL,1),
(2,'Pedro','Mecánico','pedro@autogest.com','$2a$11$mk9ufe9CGZG1he5cpawntOnHwts0QtPAbE2N4cI6SCpWFlEilxcAm','','Empleado','2026-10-27 18:55:02',NULL,1);

-- --------------------------------------------
-- Tabla: repuestos
-- --------------------------------------------
DROP TABLE IF EXISTS `repuestos`;
CREATE TABLE `repuestos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Codigo` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Descripcion` varchar(200) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Stock` int NOT NULL,
  `Precio` decimal(18,2) NOT NULL,
  `Proveedor` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Repuestos_Codigo` (`Codigo`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `repuestos` VALUES
(1,'FIL-001','Filtro de aceite',25,3500.00,'Bosch','2026-10-27 18:55:04',NULL,1),
(2,'FIL-002','Filtro de aire',15,4200.00,'Bosch','2026-10-27 18:55:04',NULL,1),
(3,'ACE-001','Aceite 5W30 (1L)',40,8500.00,'YPF','2026-10-27 18:55:04',NULL,1),
(4,'BAT-001','Batería 12V 60Ah',8,85000.00,'Willard','2026-10-27 18:55:04',NULL,1),
(5,'PAS-001','Pastillas de freno delanteras',12,25000.00,'Brembo','2026-10-27 18:55:04',NULL,1),
(6,'BOM-001','Bomba de agua',5,45000.00,'SKF','2026-10-27 18:55:04',NULL,1),
(7,'COR-001','Correa de distribución',10,32000.00,'Gates','2026-10-27 18:55:04',NULL,1),
(8,'Buj-001','Bujía de encendido',50,2800.00,'NGK','2026-10-27 18:55:04',NULL,1);

-- --------------------------------------------
-- Tabla: vehiculos
-- --------------------------------------------
DROP TABLE IF EXISTS `vehiculos`;
CREATE TABLE `vehiculos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Patente` varchar(10) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Marca` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Modelo` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Anio` int NOT NULL,
  `Kilometraje` int NOT NULL,
  `Color` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `Imagen` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `ClienteId` int NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Vehiculos_Patente` (`Patente`),
  KEY `IX_Vehiculos_ClienteId` (`ClienteId`),
  CONSTRAINT `FK_Vehiculos_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `clientes` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO `vehiculos` VALUES
(1,'AB123CD','Ford','Focus',2018,85000,'Gris',NULL,1,'2026-10-27 18:55:04',NULL,1),
(2,'EF456GH','Chevrolet','Onix',2020,45000,'Blanco',NULL,2,'2026-10-27 18:55:04',NULL,1),
(3,'IJ789KL','Volkswagen','Amarok',2019,120000,'Negro',NULL,3,'2026-10-27 18:55:04',NULL,1);

-- --------------------------------------------
-- Tabla: ordenes
-- --------------------------------------------
DROP TABLE IF EXISTS `ordenes`;
CREATE TABLE `ordenes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `VehiculoId` int NOT NULL,
  `MecanicoId` int NOT NULL,
  `FechaIngreso` datetime NOT NULL,
  `FechaEgreso` datetime DEFAULT NULL,
  `Descripcion` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Estado` varchar(20) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ManoDeObra` decimal(18,2) NOT NULL,
  `MontoTotal` decimal(18,2) NOT NULL,
  `UsuarioCreacionId` int DEFAULT NULL,
  `UsuarioModificacionId` int DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  PRIMARY KEY (`Id`),
  KEY `IX_Ordenes_MecanicoId` (`MecanicoId`),
  KEY `IX_Ordenes_UsuarioCreacionId` (`UsuarioCreacionId`),
  KEY `IX_Ordenes_UsuarioModificacionId` (`UsuarioModificacionId`),
  KEY `IX_Ordenes_VehiculoId` (`VehiculoId`),
  CONSTRAINT `FK_Ordenes_Usuarios_MecanicoId` FOREIGN KEY (`MecanicoId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Ordenes_Usuarios_UsuarioCreacionId` FOREIGN KEY (`UsuarioCreacionId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Ordenes_Usuarios_UsuarioModificacionId` FOREIGN KEY (`UsuarioModificacionId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Ordenes_Vehiculos_VehiculoId` FOREIGN KEY (`VehiculoId`) REFERENCES `vehiculos` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;



-- --------------------------------------------
-- Tabla: detalles
-- --------------------------------------------
DROP TABLE IF EXISTS `detalles`;
CREATE TABLE `detalles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `OrdenId` int NOT NULL,
  `RepuestoId` int NOT NULL,
  `Cantidad` int NOT NULL,
  `PrecioUnitario` decimal(18,2) NOT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Detalles_OrdenId` (`OrdenId`),
  KEY `IX_Detalles_RepuestoId` (`RepuestoId`),
  CONSTRAINT `FK_Detalles_Ordenes_OrdenId` FOREIGN KEY (`OrdenId`) REFERENCES `ordenes` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Detalles_Repuestos_RepuestoId` FOREIGN KEY (`RepuestoId`) REFERENCES `repuestos` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;



-- --------------------------------------------
-- Tabla: pagos
-- --------------------------------------------
DROP TABLE IF EXISTS `pagos`;
CREATE TABLE `pagos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `NumeroPago` int NOT NULL,
  `FechaPago` datetime NOT NULL,
  `Importe` decimal(18,2) NOT NULL,
  `Concepto` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Anulado` tinyint(1) NOT NULL DEFAULT '0',
  `OrdenId` int NOT NULL,
  `UsuarioCreacionId` int NOT NULL,
  `UsuarioAnulacionId` int DEFAULT NULL,
  `FechaCreacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `FechaModificacion` datetime DEFAULT NULL,
  `Activo` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Pagos_OrdenId` (`OrdenId`),
  KEY `IX_Pagos_UsuarioAnulacionId` (`UsuarioAnulacionId`),
  KEY `IX_Pagos_UsuarioCreacionId` (`UsuarioCreacionId`),
  CONSTRAINT `FK_Pagos_Ordenes_OrdenId` FOREIGN KEY (`OrdenId`) REFERENCES `ordenes` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Pagos_Usuarios_UsuarioAnulacionId` FOREIGN KEY (`UsuarioAnulacionId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT,
  CONSTRAINT `FK_Pagos_Usuarios_UsuarioCreacionId` FOREIGN KEY (`UsuarioCreacionId`) REFERENCES `usuarios` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;



SET FOREIGN_KEY_CHECKS = 1;

