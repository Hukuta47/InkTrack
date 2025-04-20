-- Создание базы данных
CREATE DATABASE InventoryDeviceDB;
GO

USE InventoryDeviceDB;
GO

-- Таблица с типами устройств
CREATE TABLE DeviceType (
    DeviceTypeID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

-- Таблица устройств
CREATE TABLE Device (
    DeviceID INT IDENTITY PRIMARY KEY,
    DeviceTypeID INT NOT NULL,
    Manufacturer NVARCHAR(50),
    Model NVARCHAR(50),
    Characteristics XML,
    InventoryNumber NCHAR(10),
    SerialNumber NVARCHAR(50),
    FOREIGN KEY (DeviceTypeID) REFERENCES DeviceType(DeviceTypeID)
);

-- Таблица принтеров (связь 1-к-1 с устройством)
CREATE TABLE Printer (
    PrinterID INT PRIMARY KEY,
    PrinterModelID INT,
    FOREIGN KEY (PrinterID) REFERENCES Device(DeviceID)
);

-- Таблица типов картриджей
CREATE TABLE CartridgeType (
    CartridgeTypeID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

-- Таблица моделей картриджей
CREATE TABLE CartridgeModel (
    CartridgeModelID INT PRIMARY KEY,
    ModelName NVARCHAR(50),
    Manufacturer NVARCHAR(50),
    CartridgeTypeID INT,
    FOREIGN KEY (CartridgeTypeID) REFERENCES CartridgeType(CartridgeTypeID)
);

-- Таблица статусов картриджей
CREATE TABLE CartridgeStatus (
    StatusID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(20)
);

-- Таблица картриджей
CREATE TABLE Cartridge (
    CartridgeID INT IDENTITY PRIMARY KEY,
    PrinterID INT,
    CartridgeModelID INT,
    CartridgeNumber NCHAR(10),
    StatusID INT,
    FOREIGN KEY (PrinterID) REFERENCES Printer(PrinterID),
    FOREIGN KEY (CartridgeModelID) REFERENCES CartridgeModel(CartridgeModelID),
    FOREIGN KEY (StatusID) REFERENCES CartridgeStatus(StatusID)
);

-- Совместимость моделей картриджей с принтерами
CREATE TABLE CartridgeCompatibility (
    CartridgeModelID INT,
    PrinterID INT,
    PRIMARY KEY (CartridgeModelID, PrinterID),
    FOREIGN KEY (CartridgeModelID) REFERENCES CartridgeModel(CartridgeModelID),
    FOREIGN KEY (PrinterID) REFERENCES Printer(PrinterID)
);

-- Таблица ФИО
CREATE TABLE FIO (
    FIOID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Surname NVARCHAR(50) NOT NULL,
    Patronymic NVARCHAR(50)
);

-- Таблица сотрудников
CREATE TABLE Employee (
    EmployeeID INT IDENTITY PRIMARY KEY,
    FIOID INT NOT NULL,
    FOREIGN KEY (FIOID) REFERENCES FIO(FIOID)
);

-- Таблица зданий
CREATE TABLE Bulding (
    BuldingID INT IDENTITY PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
);

-- Таблица кабинетов
CREATE TABLE Cabinet (
    CabinetID INT IDENTITY PRIMARY KEY,
    BuldingID INT NOT NULL,
    CabinetName NVARCHAR(50) NOT NULL,
    FOREIGN KEY (BuldingID) REFERENCES Bulding(BuldingID)
);

-- Связь "устройство - кабинет"
CREATE TABLE Cabinet_Device (
    CabinetID INT,
    DeviceID INT,
    PRIMARY KEY (CabinetID, DeviceID),
    FOREIGN KEY (CabinetID) REFERENCES Cabinet(CabinetID),
    FOREIGN KEY (DeviceID) REFERENCES Device(DeviceID)
);

-- Связь "сотрудник - кабинет"
CREATE TABLE Cabinet_Employee (
    CabinetID INT,
    EmployeeID INT,
    PRIMARY KEY (CabinetID, EmployeeID),
    FOREIGN KEY (CabinetID) REFERENCES Cabinet(CabinetID),
    FOREIGN KEY (EmployeeID) REFERENCES Employee(EmployeeID)
);

-- Вставка данных
SET IDENTITY_INSERT DeviceType ON;
INSERT INTO DeviceType (DeviceTypeID, Name) VALUES 
(1, N'Системный блок'),
(2, N'Принтер'),
(3, N'Проектор'),
(4, N'Интерактивная доска'),
(5, N'Телевизор'),
(6, N'Монитор'),
(7, N'Роутер'),
(8, N'Хаб');
SET IDENTITY_INSERT DeviceType OFF;

SET IDENTITY_INSERT Device ON;
INSERT INTO Device (DeviceID, DeviceTypeID, Manufacturer, Model, Characteristics, InventoryNumber, SerialNumber) VALUES 
(1, 6, N'Acer', N'', NULL, N'13600214', N'ETL510801453800AD4PK04'),
(3, 1, NULL, NULL, NULL, N'13600498', NULL),
(4, 2, N'Canon', N'MF211', NULL, N'13600677', N'WGT58408'),
(5, 2, N'Kyocera', N'Ecosys M2040dn', NULL, N'13600850', N'1419032212'),
(6, 2, N'Canon', NULL, NULL, N'13600615', N'f167300');
SET IDENTITY_INSERT Device OFF;

INSERT INTO Printer (PrinterID, PrinterModelID) VALUES 
(4, NULL),
(5, NULL),
(6, NULL);

SET IDENTITY_INSERT CartridgeType ON;
INSERT INTO CartridgeType (CartridgeTypeID, Name) VALUES 
(1, N'Цветной'),
(2, N'Черный');
SET IDENTITY_INSERT CartridgeType OFF;

SET IDENTITY_INSERT CartridgeStatus ON;
INSERT INTO CartridgeStatus (StatusID, Name) VALUES 
(1, N'Установлен'),
(2, N'На складе'),
(3, N'На заправке');
SET IDENTITY_INSERT CartridgeStatus OFF;

SET IDENTITY_INSERT FIO ON;
INSERT INTO FIO (FIOID, Name, Surname, Patronymic) VALUES 
(1, N'Никита', N'Зелтынь', N'Станиславович'),
(2, N'Светлана', N'Лапина', N'Николаевна');
SET IDENTITY_INSERT FIO OFF;

SET IDENTITY_INSERT Employee ON;
INSERT INTO Employee (EmployeeID, FIOID) VALUES 
(1, 1),
(2, 2);
SET IDENTITY_INSERT Employee OFF;

SET IDENTITY_INSERT Bulding ON;
INSERT INTO Bulding (BuldingID, Name) VALUES 
(1, N'Колледж'),
(2, N'Общежитие');
SET IDENTITY_INSERT Bulding OFF;

SET IDENTITY_INSERT Cabinet ON;
INSERT INTO Cabinet (CabinetID, BuldingID, CabinetName) VALUES 
(1, 1, N'101'), (2, 1, N'102'), (3, 1, N'103'), (4, 1, N'104'), (5, 1, N'105'),
(6, 1, N'106'), (7, 1, N'107'), (8, 1, N'108'), (9, 1, N'111'), (10, 1, N'112'),
(11, 1, N'112 А'), (12, 1, N'113'), (13, 1, N'114'), (14, 1, N'115'), (15, 1, N'117'),
(16, 1, N'117 А'), (17, 1, N'118'), (18, 1, N'119'), (19, 1, N'120'), (20, 1, N'121'),
(21, 1, N'122'), (22, 1, N'123'), (23, 1, N'124 А'), (24, 1, N'125'), (25, 1, N'126'),
(26, 1, N'202'), (27, 1, N'203'), (28, 1, N'204'), (29, 1, N'205'), (30, 1, N'206'),
(31, 1, N'208'), (32, 1, N'209'), (33, 1, N'210'), (34, 1, N'211'), (35, 1, N'212'),
(36, 1, N'213'), (37, 1, N'214'), (38, 1, N'215'), (39, 1, N'216'), (40, 1, N'219'),
(41, 1, N'221'), (42, 1, N'301'), (43, 1, N'302'), (44, 1, N'303'), (45, 1, N'305'),
(46, 1, N'306'), (47, 1, N'307'), (48, 1, N'308'), (49, 1, N'309'), (50, 1, N'311'),
(51, 1, N'312'), (52, 1, N'313'), (53, 1, N'314'), (54, 1, N'315'), (55, 1, N'316'),
(56, 1, N'317'), (57, 1, N'401'), (58, 1, N'403'), (59, 1, N'404'), (60, 1, N'406'),
(61, 1, N'407'), (62, 1, N'408'), (63, 1, N'410'), (64, 1, N'412'), (65, 1, N'413'),
(66, 1, N'415'), (67, 1, N'416'), (68, 1, N'417'), (69, 1, N'418'), (70, 1, N'419'),
(71, 1, N'420'), (72, 1, N'421'), (73, 1, N'422'), (74, 1, N'423');
SET IDENTITY_INSERT Cabinet OFF;

INSERT INTO Cabinet_Device (CabinetID, DeviceID) VALUES 
(1, 1), (1, 3), (2, 4), (2, 5), (3, 6);

INSERT INTO Cabinet_Employee (CabinetID, EmployeeID) VALUES 
(28, 2), (33, 1);