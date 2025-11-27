CREATE TABLE StorageRooms (
    StorageRoomId INT IDENTITY(1,1) PRIMARY KEY,
    Number NVARCHAR(10) NOT NULL,
    Area FLOAT,
    OwnerId INT,
    CONSTRAINT FK_StorageRooms_Residents FOREIGN KEY (OwnerId) REFERENCES Residents(ResidentId)
);

UPDATE StorageRooms
SET Number = CONCAT(CAST(StorageRoomId AS NVARCHAR), 'Н');

DECLARE @startStor INT = 1;
DECLARE @endStor INT = 90;

WHILE @startStor <= @endStor BEGIN
    DECLARE @storNum NVARCHAR(10) = CONCAT(CAST(@startStor AS NVARCHAR), N'Н'); -- Название кладовой (1Н, 2Н и т.д.)
    DECLARE @area DECIMAL(10,2) = ROUND((RAND() * 5) + 2, 2); -- Генератор случайной площади от 2 до 7 м²

    INSERT INTO StorageRooms (Number, Area)
    VALUES (@storNum, @area);

    SET @startStor += 1;
END


select * from StorageRooms
---- parkings
alter table ParkingRooms add Area FLOAT NOT NULL

UPDATE ParkingRooms
SET Number = CONCAT(CAST(ParkingRoomId AS NVARCHAR), 'Н');

DECLARE @startStor INT = 91;
DECLARE @endStor INT = 136;

WHILE @startStor <= @endStor BEGIN
    DECLARE @storNum NVARCHAR(10) = CONCAT(CAST(@startStor AS NVARCHAR), N'Н'); -- Название кладовой (1Н, 2Н и т.д.)
    DECLARE @area DECIMAL(10,2) = ROUND((RAND() * 12) + 20, 2); -- Генератор случайной площади от 2 до 7 м²

    INSERT INTO ParkingRooms (Number, Area)
    VALUES (@storNum, @area);

    SET @startStor += 1;
END

SELECT * FROM ParkingRooms