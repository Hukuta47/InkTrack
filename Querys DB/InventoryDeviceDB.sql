-- Создание базы данных

CREATE DATABASE [InventoryDeviceDB]
GO
USE [InventoryDeviceDB];
GO

-- Таблица Cabinet
CREATE TABLE [Cabinet] (
    [Cabinet ID] INT IDENTITY(1,1) NOT NULL,
    [Cabinet Name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Cabinet] PRIMARY KEY([Cabinet ID])
);
GO



-- Таблица FIO
CREATE TABLE [FIO] (
    [Fio ID] INT IDENTITY(1,1) NOT NULL,
    [Name] NVARCHAR(50) NOT NULL,
    [Surname] NVARCHAR(50) NOT NULL,
    [Patronymic] NVARCHAR(50),
    CONSTRAINT [PK_FIO] PRIMARY KEY([Fio ID]),
	
);
GO

-- Таблица Employee
CREATE TABLE [Employee] (
    [Employee ID] INT IDENTITY(1,1) NOT NULL,
    [Fio ID] INT NOT NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY([Employee ID]),
	CONSTRAINT [FK_Employee_FIO] FOREIGN KEY ([Fio ID]) REFERENCES [FIO]([Fio ID])
);
GO
-- Таблица Cartridge
CREATE TABLE [Cartridge] (
    [Cartridge ID] INT IDENTITY(1,1) NOT NULL,
    CONSTRAINT [PK_Cartridge] PRIMARY KEY([Cartridge ID])
);
GO

-- Таблица Printer
CREATE TABLE [Printer] (
    [Printer ID] INT IDENTITY(1,1) NOT NULL,
    [Cartridge ID] INT NOT NULL,
    CONSTRAINT [PK_Printer] PRIMARY KEY([Printer ID]),
	CONSTRAINT [FK_Printer_Cartridge] FOREIGN KEY ([Cartridge ID]) REFERENCES [Cartridge]([Cartridge ID])
);
GO

-- Таблица Cabinet - Employee (связь многие ко многим)
CREATE TABLE [dbo].[Cabinet_Employee] (
    [Cabinet ID] INT NOT NULL,
    [Employee ID] INT NOT NULL,
    CONSTRAINT [PK_Cabinet_Employee] PRIMARY KEY([Cabinet ID], [Employee ID]),

	CONSTRAINT [FK_Cabinet_Employee_Cabinet] FOREIGN KEY ([Cabinet ID]) REFERENCES [Cabinet]([Cabinet ID]),
	CONSTRAINT [FK_Cabinet_Employee_Employee] FOREIGN KEY ([Employee ID]) REFERENCES [dbo].[Employee]([Employee ID])
);
GO

-- Таблица Cabinet - Printer (связь многие ко многим)
CREATE TABLE [dbo].[Cabinet_Printer] (
    [Cabinet ID] INT NOT NULL,
    [Printer ID] INT NOT NULL,
    CONSTRAINT [PK_Cabinet_Printer] PRIMARY KEY([Cabinet ID], [Printer ID]),

	CONSTRAINT [FK_Cabinet_Printer_Cabinet] FOREIGN KEY ([Cabinet ID]) REFERENCES [dbo].[Cabinet]([Cabinet ID]),
    CONSTRAINT [FK_Cabinet_Printer_Printer] FOREIGN KEY ([Printer ID]) REFERENCES [dbo].[Printer]([Printer ID])
);

USE master