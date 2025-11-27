-- Initial schema seed for local development
USE master;
GO

IF DB_ID(N'GreenQuarter') IS NULL
BEGIN
    CREATE DATABASE GreenQuarter;
END
GO

USE GreenQuarter;
GO

-- Sample owners
INSERT INTO Owners (Id, FullName, Email, Phone, CreatedAt)
VALUES
    (NEWID(), N'Irina Petrova', 'irina.petrova@example.com', '+7 999 111-22-33', SYSUTCDATETIME()),
    (NEWID(), N'Maksim Volkov', 'maksim.volkov@example.com', '+7 999 444-55-66', SYSUTCDATETIME());

-- Sample apartments (schema assumed to exist via EF migrations)
INSERT INTO Apartments (Id, Number, Building, Entrance, Floor, Area, Status, CreatedAt)
VALUES
    (NEWID(), '12', 'A', '1', 3, 74.5, 'Occupied', SYSUTCDATETIME()),
    (NEWID(), '45', 'B', '2', 7, 90.2, 'Vacant', SYSUTCDATETIME());

-- Owner-asset links (using placeholder GUIDs; replace with actual IDs after migrations)
-- INSERT ... SELECT statements should map actual IDs.

