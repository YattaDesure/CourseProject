DECLARE @residentId INT;
SELECT @residentId = ResidentId
FROM Residents
WHERE LastName = 'Мотылькова';

-- Обновить владельца парковочного места №36
UPDATE ParkingRooms
SET OwnerId = 4
WHERE ParkingRoomId = 28;

-- Обновить владельца квартиры №79
UPDATE Apartments
SET ResidentId = 4
WHERE ApartmentId = 79;

SELECT * from ParkingRooms