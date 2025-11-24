GRANT alter on SCHEMA::dbo TO 

ALTER TABLE dbo.Residents DROP COLUMN Name;
ALTER TABLE Residents ADD FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Patronymic NVARCHAR(50);

INSERT INTO Residents (Phone, Email, FirstName, LastName, Patronymic) VALUES
('89009202127', 'edikyazikov@gmail.com', 'Эдуард', 'Язиков', 'Александрович');

SELECT * FROM Residents

ALTER DATABASE Cursovaya
COLLATE Cyrillic_General_100_CI_AS;

INSERT INTO Residents (Phone, Email, FirstName, LastName, Patronymic, Password) VALUES
('89009202113', 'edikyazikov1@gmail.com', N'Эдуард', N'Язиков', N'Александрович', '12345678');

delete from Residents WHERE ResidentId=2

ALTER TABLE ResidentHistory
ADD CONSTRAINT FK_ResidentHistory_Residents
FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId);

ALTER TABLE OwnershipHistory
ADD CONSTRAINT FK_OwnershipHistory_PreviousOwner
FOREIGN KEY (PreviousOwnerId) REFERENCES Residents(ResidentId);

ALTER TABLE OwnershipHistory
ADD CONSTRAINT FK_OwnershipHistory_CurrentOwner
FOREIGN KEY (CurrentOwnerId) REFERENCES Residents(ResidentId);

Alter TABLE Residents ADD Password NVARCHAR(50) NOT NULL;

INSERT INTO Residents (Phone, Email, FirstName, LastName, Patronymic, Password) VALUES
('89009202114', 'motylkova@gmail.com', N'Ирина', N'Мотылькова', N'Игоревна', 'admin123'), -- Админ
('89009202115', 'annayazykova@gmail.com', N'Анна', N'Язикова', N'Анатольевна', 'moderator123'), -- Модератор
('89009202116', 'alexandr.petrov@gmail.com', N'Александр', N'Петров', N'Владимирович', 'abc123'),
('89009202117', 'mariya.sidorenko@gmail.com', N'Марья', N'Сидоренко', N'Викторовна', 'passw0rd'),
('89009202118', 'sergey.ivanov@gmail.com', N'Сергей', N'Иванов', N'Андреевич', 'test123'),
('89009202119', 'anatoliy.popov@gmail.com', N'Анато́лий', N'По́пов', N'Семёнович', 'letmein'),
('89009202120', 'katya.kuznetsova@gmail.com', N'Екатерина', N'Кузнецова', N'Денисовна', 'qazwsxedc'),
('89009202121', 'vasilii.smirin@gmail.com', N'Василий', N'Смирин', N'Леонидович', 'secret123'),
('89009202122', 'darya.morozova@gmail.com', N'Дарья', N'Моро́зова', N'Георгиевна', 'password1');

INSERT into ResidentRoles (ResidentId, RoleId) VALUES
(9,1),
(10,1),
(11,1),
(12,1);

SELECT * from ResidentHistory


insert into Apartments (Number, Floor, Area, Entrance) VALUES
('1','2','27,80', 1),

drop TABLE Apartments;
select * from Apartments;

CREATE TABLE Apartments (
    ApartmentId INT IDENTITY(1,1) PRIMARY KEY, -- Первичный ключ
    Number NVARCHAR(10) NOT NULL, -- Номер квартиры
    Floor INT NOT NULL, -- Этаж
    Area DECIMAL(10,2) NOT NULL, -- Площадь
    Entrance INT NOT NULL, -- Номер подъезда
    ResidentId INT, -- Внешний ключ на владельца квартиры
    CONSTRAINT FK_Apartments_Residents FOREIGN KEY (ResidentId) REFERENCES Residents(ResidentId)
);

-- 1-й подъезд (78 квартир)
-- Генерация квартир для 1-го подъезда (площади повторяются каждые 6 квартир)
DECLARE @startApt INT = 1;
DECLARE @endApt INT = 78;
DECLARE @floorCount INT = 13; -- Всего этажей, начиная со второго
DECLARE @flatsPerFloor INT = 6;
DECLARE @entrance INT = 1; -- Номер подъезда
DECLARE @areas TABLE (OrderIndex INT, Area DECIMAL(10,2)); -- Вспомогательная таблица для хранения уникальных площадей

-- Заданная начальная площадь
INSERT INTO @areas (OrderIndex, Area) VALUES
(1, 55.00), -- площадь 1-ой квартиры
(2, 60.00), -- площадь 2-ой квартиры
(3, 65.00), -- площадь 3-ей квартиры
(4, 70.00), -- площадь 4-ой квартиры
(5, 75.00), -- площадь 5-ой квартиры
(6, 80.00); -- площадь 6-ой квартиры

WHILE @startApt <= @endApt BEGIN
    DECLARE @currentFloor INT = ((@startApt - 1) / @flatsPerFloor) + 2; -- Начинаем со второго этажа
    DECLARE @aptNum NVARCHAR(10) = FORMAT(@startApt, 'D3'); -- Формат номера квартиры
    
    -- Определяем нужную площадь путём деления с остатком
    DECLARE @index INT = (@startApt - 1) % 6 + 1; -- Циклический индекс площади (от 1 до 6)
    DECLARE @area DECIMAL(10,2) = (SELECT Area FROM @areas WHERE OrderIndex = @index);

    INSERT INTO Apartments (Number, Floor, Area, Entrance)
    VALUES (@aptNum, @currentFloor, @area, @entrance);

    SET @startApt += 1;
END


-- Генерация квартир для 2-го подъезда (площади повторяются каждые 9 квартир)
DECLARE @startApt INT = 79;
DECLARE @endApt INT = 195;
DECLARE @floorCount INT = 13; -- Всего этажей, начиная со второго
DECLARE @flatsPerFloor INT = 9;
DECLARE @entrance INT = 2; -- Номер подъезда

-- Объявляем временную таблицу для хранения уникальных площадей
DECLARE @areas TABLE (OrderIndex INT, Area DECIMAL(10,2));

-- Заполняем временную таблицу начальными значениями площадей
INSERT INTO @areas (OrderIndex, Area) VALUES
(1, 60.00), -- площадь 79-ой квартиры
(2, 65.00), -- площадь 80-ой квартиры
(3, 70.00), -- площадь 81-ой квартиры
(4, 75.00), -- площадь 82-ой квартиры
(5, 80.00), -- площадь 83-ой квартиры
(6, 85.00), -- площадь 84-ой квартиры
(7, 90.00), -- площадь 85-ой квартиры
(8, 95.00), -- площадь 86-ой квартиры
(9, 100.00); -- площадь 87-ой квартиры

-- Основной цикл вставки квартир
WHILE @startApt <= @endApt BEGIN
    DECLARE @currentFloor INT = ((@startApt - 79) / @flatsPerFloor) + 2; -- Начинаем со второго этажа
    DECLARE @aptNum NVARCHAR(10) = FORMAT(@startApt, 'D3'); -- Формат номера квартиры
    
    -- Определяем нужную площадь путём деления с остатком
    DECLARE @index INT = (@startApt - 79) % 9 + 1; -- Циклический индекс площади (от 1 до 9)
    DECLARE @area DECIMAL(10,2) = (SELECT Area FROM @areas WHERE OrderIndex = @index);

    -- Вставка данных в таблицу Apartments
    INSERT INTO Apartments (Number, Floor, Area, Entrance)
    VALUES (@aptNum, @currentFloor, @area, @entrance);

    -- Переходим к следующей квартире
    SET @startApt += 1;
END

select * from Apartments


-- Генерация квартир для 3-го подъезда (площади повторяются каждые 9 квартир)
DECLARE @startApt INT = 196;
DECLARE @endApt INT = 267;
DECLARE @floorCount INT = 13; -- Всего этажей, начиная со второго
DECLARE @flatsPerFloor INT = 9;
DECLARE @entrance INT = 3; -- Номер подъезда

-- Объявляем временную таблицу для хранения уникальных площадей
DECLARE @areas TABLE (OrderIndex INT, Area DECIMAL(10,2));

-- Заполняем временную таблицу начальными значениями площадей
INSERT INTO @areas (OrderIndex, Area) VALUES
(1, 65.00), -- площадь 196-ой квартиры
(2, 70.00), -- площадь 197-ой квартиры
(3, 75.00), -- площадь 198-ой квартиры
(4, 80.00), -- площадь 199-ой квартиры
(5, 85.00), -- площадь 200-ой квартиры
(6, 90.00), -- площадь 201-ой квартиры
(7, 95.00), -- площадь 202-ой квартиры
(8, 100.00), -- площадь 203-ой квартиры
(9, 105.00); -- площадь 204-ой квартиры

-- Основной цикл вставки квартир
WHILE @startApt <= @endApt BEGIN
    DECLARE @currentFloor INT = ((@startApt - 196) / @flatsPerFloor) + 2; -- Начинаем со второго этажа
    DECLARE @aptNum NVARCHAR(10) = FORMAT(@startApt, 'D3'); -- Формат номера квартиры
    
    -- Определяем нужную площадь путём деления с остатком
    DECLARE @index INT = (@startApt - 196) % 9 + 1; -- Циклический индекс площади (от 1 до 9)
    DECLARE @area DECIMAL(10,2) = (SELECT Area FROM @areas WHERE OrderIndex = @index);

    -- Вставка данных в таблицу Apartments
    INSERT INTO Apartments (Number, Floor, Area, Entrance)
    VALUES (@aptNum, @currentFloor, @area, @entrance);

    -- Переходим к следующей квартире
    SET @startApt += 1;
END