-- Пример SQL запросов для базы данных Cursovaya

-- Просмотр всех пользователей
SELECT 
    ResidentId,
    FirstName,
    LastName,
    Email,
    Phone
FROM Residents
ORDER BY LastName, FirstName;

-- Статистика по квартирам
SELECT 
    COUNT(*) as TotalApartments,
    SUM(CASE WHEN ResidentId IS NULL THEN 1 ELSE 0 END) as Available,
    SUM(CASE WHEN ResidentId IS NOT NULL THEN 1 ELSE 0 END) as Occupied
FROM Apartments;

-- Статистика по парковке
SELECT 
    COUNT(*) as TotalParking,
    SUM(CASE WHEN OwnerId IS NULL THEN 1 ELSE 0 END) as Available,
    SUM(CASE WHEN OwnerId IS NOT NULL THEN 1 ELSE 0 END) as Occupied
FROM ParkingRooms;

-- Статистика по кладовым
SELECT 
    COUNT(*) as TotalStorage,
    SUM(CASE WHEN OwnerId IS NULL THEN 1 ELSE 0 END) as Available,
    SUM(CASE WHEN OwnerId IS NOT NULL THEN 1 ELSE 0 END) as Occupied
FROM StorageRooms;

-- Квартиры с владельцами
SELECT 
    a.ApartmentId,
    a.Number,
    a.Floor,
    a.Area,
    a.Entrance,
    r.FirstName + ' ' + r.LastName as OwnerName,
    r.Email as OwnerEmail
FROM Apartments a
LEFT JOIN Residents r ON a.ResidentId = r.ResidentId
ORDER BY a.Number;

-- Пользователи с ролями
SELECT 
    r.ResidentId,
    r.FirstName,
    r.LastName,
    r.Email,
    COALESCE(rol.RoleName, 'User') as RoleName
FROM Residents r
LEFT JOIN ResidentRoles rr ON r.ResidentId = rr.ResidentId
LEFT JOIN roles rol ON rr.RoleId = rol.RoleId
ORDER BY r.LastName, r.FirstName;

