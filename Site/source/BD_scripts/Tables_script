USE Cursovaya;
GO

-- Таблица жители
CREATE TABLE Residents (
    ResidentId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(25),
    Email NVARCHAR(100)
);

-- Таблица назначение ролей жителям
CREATE TABLE ResidentRoles (
    ResidentId INT,
    RoleId INT,
    PRIMARY KEY (ResidentId, RoleId),
    CONSTRAINT FK_ResidentRoles_Residents FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId),
    CONSTRAINT FK_ResidentRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);

-- Таблица квартиры
CREATE TABLE Apartments (
    ApartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Number NVARCHAR(10) NOT NULL,
    Floor INT NOT NULL,
    Area FLOAT,
    OwnerId INT,
    CONSTRAINT FK_Apartments_Residents FOREIGN KEY (OwnerId) REFERENCES Residents(ResidentId)
);

-- Таблица кладовые
CREATE TABLE StorageRooms (
    StorageRoomId INT IDENTITY(1,1) PRIMARY KEY,
    Number NVARCHAR(10) NOT NULL,
    Area FLOAT,
    OwnerId INT,
    CONSTRAINT FK_StorageRooms_Residents FOREIGN KEY (OwnerId) REFERENCES Residents(ResidentId)
);

-- Таблица паркинги
CREATE TABLE ParkingRooms (
    ParkingRoomId INT IDENTITY(1,1) PRIMARY KEY,
    OwnerId INT,
    Number NVARCHAR(10) NOT NULL,
    CONSTRAINT FK_ParkingRooms_Residents FOREIGN KEY (OwnerId) REFERENCES Residents(ResidentId)
);

-- История владения (в будущем менять)
CREATE TABLE OwnershipHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    ObjectType NVARCHAR(50) NOT NULL,
    ObjectId INT NOT NULL,
    PreviousOwnerId INT,
    CurrentOwnerId INT,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME
);

-- Лог изменений жильцов (если потребуется аудит)
CREATE TABLE ResidentHistory (
    HistoryId INT IDENTITY(1,1) PRIMARY KEY,
    ResidentId INT,
    OldData NVARCHAR(MAX),
    NewData NVARCHAR(MAX),
    UpdateDateTime DATETIME DEFAULT GETDATE()
);

-- Триггер для регистрации изменений
CREATE TRIGGER TR_UpdateResidentLog ON Residents FOR UPDATE AS
BEGIN
    INSERT INTO ResidentHistory (ResidentId, OldData, NewData, UpdateDateTime)
    SELECT i.ResidentId, d.Name + ' (' + ISNULL(d.Phone,'') + ')', i.Name + ' (' + ISNULL(i.Phone,'') + ')', GETDATE() FROM inserted i JOIN deleted d ON i.ResidentId = d.ResidentId;
END;