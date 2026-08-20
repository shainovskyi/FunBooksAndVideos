-- one time setup script for the database schema and seed data for FunBooksAndVideos application


IF DB_ID('FunBooksAndVideos') IS NULL
    CREATE DATABASE FunBooksAndVideos;
GO

USE FunBooksAndVideos;
GO

DECLARE @createSchema BIT = CASE WHEN OBJECT_ID('dbo.Customers', 'U') IS NULL THEN 1 ELSE 0 END;

IF @createSchema = 0 RETURN;

CREATE TABLE dbo.Customers
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_Customers PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL
);

CREATE TABLE dbo.CustomerMemberships
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_CustomerMemberships PRIMARY KEY,
    CustomerId INT NOT NULL CONSTRAINT FK_CustomerMemberships_Customers REFERENCES dbo.Customers(Id),
    Membership VARCHAR(50) NOT NULL
);

CREATE TABLE dbo.Products
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_Products PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Type VARCHAR(50) NOT NULL CONSTRAINT CK_Products_Type CHECK (Type IN ('Book', 'Video'))
);

CREATE TABLE dbo.PurchaseOrders
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_PurchaseOrders PRIMARY KEY,
    CustomerId INT NOT NULL CONSTRAINT FK_PurchaseOrders_Customers REFERENCES dbo.Customers(Id),
    TotalPrice DECIMAL(18, 2) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL
);

CREATE TABLE dbo.LineItems
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_LineItems PRIMARY KEY,
    PurchaseOrderId INT NOT NULL CONSTRAINT FK_LineItems_PurchaseOrders REFERENCES dbo.PurchaseOrders(Id),
    Type VARCHAR(50) NOT NULL,
    ProductId INT NULL CONSTRAINT FK_LineItems_Products REFERENCES dbo.Products(Id),
    MembershipType VARCHAR(50) NULL,
    Description VARCHAR(300) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL
);

CREATE TABLE dbo.ShippingSlips
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_ShippingSlips PRIMARY KEY,
    PurchaseOrderId INT NOT NULL CONSTRAINT FK_ShippingSlips_PurchaseOrders REFERENCES dbo.PurchaseOrders(Id),
    CustomerId INT NOT NULL CONSTRAINT FK_ShippingSlips_Customers REFERENCES dbo.Customers(Id),
    CreatedAtUtc DATETIME2 NOT NULL
);

CREATE TABLE dbo.ShippingSlipItems
(
    Id INT IDENTITY(1, 1) CONSTRAINT PK_ShippingSlipItems PRIMARY KEY,
    ShippingSlipId INT NOT NULL CONSTRAINT FK_ShippingSlipItems_ShippingSlips REFERENCES dbo.ShippingSlips(Id),
    ItemName VARCHAR(100) NOT NULL
);

CREATE TABLE dbo.MembershipPrices
(
    MembershipType VARCHAR(50) NOT NULL CONSTRAINT PK_MembershipPrices PRIMARY KEY,
    Price DECIMAL(18, 2) NOT NULL CONSTRAINT CK_MembershipPrices_Price CHECK (Price >= 0)
);

GO

-----------------------------------------------------------------------------
-- SEED DATA
-----------------------------------------------------------------------------

DECLARE @seedData BIT = CASE WHEN EXISTS (SELECT TOP 1 1 FROM dbo.Customers) THEN 0 ELSE 1 END;

IF @seedData = 0 RETURN;

INSERT INTO dbo.Customers (Name, Email)
SELECT
    Name  = CONCAT('Customer ', value),
    Email = CONCAT('customer', value, '@mail.com')
FROM GENERATE_SERIES(1, 10);

INSERT INTO dbo.Products (Name, Price, Type)
SELECT
    Name  = CONCAT(t.Type, ' ', value),
    Price = CAST((5.00 + (value % 4 * 10) + (value * RAND())) AS DECIMAL(18,2)),
    Type  = t.Type
FROM GENERATE_SERIES(1, 20)
CROSS APPLY (SELECT Type = CASE WHEN value % 3 = 0 THEN 'Video' ELSE 'Book' END) t;

INSERT INTO dbo.MembershipPrices (MembershipType, Price)
VALUES ('BookClub', 12.99), ('VideoClub', 8.01), ('Premium', 15.50);

GO

-----------------------------------------------------------------------------
-- CLEANUP DATA
-----------------------------------------------------------------------------
/*
DELETE FROM dbo.ShippingSlipItems;
DELETE FROM dbo.ShippingSlips;
DELETE FROM dbo.LineItems;
DELETE FROM dbo.PurchaseOrders;
DELETE FROM dbo.Products;
DELETE FROM dbo.CustomerMemberships;
DELETE FROM dbo.Customers;
DELETE FROM dbo.MembershipPrices;
*/

-----------------------------------------------------------------------------
-- DROP ALL TABLES
-----------------------------------------------------------------------------
/*
DROP TABLE IF EXISTS dbo.ShippingSlipItems;
DROP TABLE IF EXISTS dbo.ShippingSlips;
DROP TABLE IF EXISTS dbo.LineItems;
DROP TABLE IF EXISTS dbo.PurchaseOrders;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.CustomerMemberships;
DROP TABLE IF EXISTS dbo.Customers;
DROP TABLE IF EXISTS dbo.MembershipPrices;
*/