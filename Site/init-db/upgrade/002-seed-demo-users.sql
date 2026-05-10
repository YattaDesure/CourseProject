/*
  Demo seed: 50 realistic Residents + assign unique premises.
  - Adds Residents with simple passwords (for demo) and assigns User role.
  - Assigns 30 apartments + 10 storage rooms + 10 parking spaces to these residents.
  Idempotent: skips users if email exists.
*/

USE [Cursovaya];
GO

SET NOCOUNT ON;
GO

DECLARE @SeedPrefix NVARCHAR(32) = N'demo';

IF OBJECT_ID('tempdb..#SeedResidents') IS NOT NULL DROP TABLE #SeedResidents;
CREATE TABLE #SeedResidents (
  Email NVARCHAR(256) NOT NULL,
  Password NVARCHAR(128) NOT NULL,
  FirstName NVARCHAR(100) NOT NULL,
  LastName NVARCHAR(100) NOT NULL,
  Patronymic NVARCHAR(100) NULL,
  Phone NVARCHAR(32) NULL
);

INSERT INTO #SeedResidents (Email, Password, FirstName, LastName, Patronymic, Phone)
VALUES
 (N'demo.alina.korneeva01@mail.ru', N'user123', N'Алина', N'Корнеева', N'Сергеевна', N'+7 (911) 201-30-01'),
 (N'demo.maksim.nazarov02@mail.ru', N'user123', N'Максим', N'Назаров', N'Андреевич', N'+7 (911) 201-30-02'),
 (N'demo.sofia.gromova03@mail.ru', N'user123', N'Софья', N'Громова', N'Ильинична', N'+7 (911) 201-30-03'),
 (N'demo.artem.belyaev04@mail.ru', N'user123', N'Артём', N'Беляев', N'Павлович', N'+7 (911) 201-30-04'),
 (N'demo.maria.krylova05@mail.ru', N'user123', N'Мария', N'Крылова', N'Олеговна', N'+7 (911) 201-30-05'),
 (N'demo.egor.mironov06@mail.ru', N'user123', N'Егор', N'Миронов', N'Викторович', N'+7 (911) 201-30-06'),
 (N'demo.daria.voronina07@mail.ru', N'user123', N'Дарья', N'Воронина', N'Романовна', N'+7 (911) 201-30-07'),
 (N'demo.ivan.sazonov08@mail.ru', N'user123', N'Иван', N'Сазонов', N'Михайлович', N'+7 (911) 201-30-08'),
 (N'demo.elena.filippova09@mail.ru', N'user123', N'Елена', N'Филиппова', N'Николаевна', N'+7 (911) 201-30-09'),
 (N'demo.kirill.kuznetsov10@mail.ru', N'user123', N'Кирилл', N'Кузнецов', N'Алексеевич', N'+7 (911) 201-30-10'),

 (N'demo.victoria.smirnova11@mail.ru', N'user123', N'Виктория', N'Смирнова', N'Дмитриевна', N'+7 (911) 201-30-11'),
 (N'demo.roman.kalinin12@mail.ru', N'user123', N'Роман', N'Калинин', N'Игоревич', N'+7 (911) 201-30-12'),
 (N'demo.polina.morozova13@mail.ru', N'user123', N'Полина', N'Морозова', N'Артёмовна', N'+7 (911) 201-30-13'),
 (N'demo.denis.kolesnikov14@mail.ru', N'user123', N'Денис', N'Колесников', N'Петрович', N'+7 (911) 201-30-14'),
 (N'demo.karina.fomina15@mail.ru', N'user123', N'Карина', N'Фомина', N'Владимировна', N'+7 (911) 201-30-15'),
 (N'demo.nikita.orlov16@mail.ru', N'user123', N'Никита', N'Орлов', N'Станиславович', N'+7 (911) 201-30-16'),
 (N'demo.tatiana.zhuravleva17@mail.ru', N'user123', N'Татьяна', N'Журавлёва', N'Константиновна', N'+7 (911) 201-30-17'),
 (N'demo.oleg.karpov18@mail.ru', N'user123', N'Олег', N'Карпов', N'Евгеньевич', N'+7 (911) 201-30-18'),
 (N'demo.natalia.bogdanova19@mail.ru', N'user123', N'Наталья', N'Богданова', N'Валерьевна', N'+7 (911) 201-30-19'),
 (N'demo.pavel.gusev20@mail.ru', N'user123', N'Павел', N'Гусев', N'Аркадьевич', N'+7 (911) 201-30-20'),

 (N'demo.veronika.sokolova21@mail.ru', N'user123', N'Вероника', N'Соколова', N'Александровна', N'+7 (911) 201-30-21'),
 (N'demo.alexey.stepanov22@mail.ru', N'user123', N'Алексей', N'Степанов', N'Борисович', N'+7 (911) 201-30-22'),
 (N'demo.ksenia.rodionova23@mail.ru', N'user123', N'Ксения', N'Родионова', N'Васильевна', N'+7 (911) 201-30-23'),
 (N'demo.ilia.antonov24@mail.ru', N'user123', N'Илья', N'Антонов', N'Григорьевич', N'+7 (911) 201-30-24'),
 (N'demo.yana.cherkasova25@mail.ru', N'user123', N'Яна', N'Черкасова', N'Максимовна', N'+7 (911) 201-30-25'),
 (N'demo.sergey.dorofeev26@mail.ru', N'user123', N'Сергей', N'Дорофеев', N'Витальевич', N'+7 (911) 201-30-26'),
 (N'demo.lidia.ershova27@mail.ru', N'user123', N'Лидия', N'Ершова', N'Геннадьевна', N'+7 (911) 201-30-27'),
 (N'demo.timur.makarov28@mail.ru', N'user123', N'Тимур', N'Макаров', N'Леонидович', N'+7 (911) 201-30-28'),
 (N'demo.oksana.bykova29@mail.ru', N'user123', N'Оксана', N'Быкова', N'Юрьевна', N'+7 (911) 201-30-29'),
 (N'demo.stanislav.danilov30@mail.ru', N'user123', N'Станислав', N'Данилов', N'Фёдорович', N'+7 (911) 201-30-30'),

 (N'demo.evgenia.kiseleva31@mail.ru', N'user123', N'Евгения', N'Киселёва', N'Павловна', N'+7 (911) 201-30-31'),
 (N'demo.igor.isaev32@mail.ru', N'user123', N'Игорь', N'Исаев', N'Романович', N'+7 (911) 201-30-32'),
 (N'demo.milana.vetrova33@mail.ru', N'user123', N'Милана', N'Ветрова', N'Алексеевна', N'+7 (911) 201-30-33'),
 (N'demo.vadim.kopylov34@mail.ru', N'user123', N'Вадим', N'Копылов', N'Антонович', N'+7 (911) 201-30-34'),
 (N'demo.alla.kulikova35@mail.ru', N'user123', N'Алла', N'Куликова', N'Семёновна', N'+7 (911) 201-30-35'),
 (N'demo.gleb.savin36@mail.ru', N'user123', N'Глеб', N'Савин', N'Владиславович', N'+7 (911) 201-30-36'),
 (N'demo.diana.petrova37@mail.ru', N'user123', N'Диана', N'Петрова', N'Игоревна', N'+7 (911) 201-30-37'),
 (N'demo.andrey.chernov38@mail.ru', N'user123', N'Андрей', N'Чернов', N'Валентинович', N'+7 (911) 201-30-38'),
 (N'demo.nina.kazantseva39@mail.ru', N'user123', N'Нина', N'Казанцева', N'Олеговна', N'+7 (911) 201-30-39'),
 (N'demo.fedor.lebedev40@mail.ru', N'user123', N'Фёдор', N'Лебедев', N'Сергеевич', N'+7 (911) 201-30-40'),

 (N'demo.vera.belkina41@mail.ru', N'user123', N'Вера', N'Белкина', N'Петровна', N'+7 (911) 201-30-41'),
 (N'demo.ruslan.konovalov42@mail.ru', N'user123', N'Руслан', N'Коновалов', N'Егорович', N'+7 (911) 201-30-42'),
 (N'demo.kristina.avdeeva43@mail.ru', N'user123', N'Кристина', N'Авдеева', N'Михайловна', N'+7 (911) 201-30-43'),
 (N'demo.yaroslav.frolov44@mail.ru', N'user123', N'Ярослав', N'Фролов', N'Ильич', N'+7 (911) 201-30-44'),
 (N'demo.snezhana.baranova45@mail.ru', N'user123', N'Снежана', N'Баранова', N'Ивановна', N'+7 (911) 201-30-45'),
 (N'demo.anton.yakovlev46@mail.ru', N'user123', N'Антон', N'Яковлев', N'Алексеевич', N'+7 (911) 201-30-46'),
 (N'demo.larisa.zharkova47@mail.ru', N'user123', N'Лариса', N'Жаркова', N'Викторовна', N'+7 (911) 201-30-47'),
 (N'demo.marat.knyazev48@mail.ru', N'user123', N'Марат', N'Князев', N'Рустамович', N'+7 (911) 201-30-48'),
 (N'demo.olga.melnikova49@mail.ru', N'user123', N'Ольга', N'Мельникова', N'Сергеевна', N'+7 (911) 201-30-49'),
 (N'demo.german.pankratov50@mail.ru', N'user123', N'Герман', N'Панкратов', N'Вадимович', N'+7 (911) 201-30-50');

/* Insert Residents */
INSERT INTO dbo.Residents (Email, Password, FirstName, LastName, Patronymic, Phone)
SELECT s.Email, s.Password, s.FirstName, s.LastName, s.Patronymic, s.Phone
FROM #SeedResidents s
WHERE NOT EXISTS (SELECT 1 FROM dbo.Residents r WHERE r.Email = s.Email);

/* Ensure role tables exist; roleId=1 should be User in this DB */
IF OBJECT_ID(N'dbo.ResidentRoles', N'U') IS NOT NULL
BEGIN
  INSERT INTO dbo.ResidentRoles (ResidentId, RoleId)
  SELECT r.ResidentId, 1
  FROM dbo.Residents r
  INNER JOIN #SeedResidents s ON s.Email = r.Email
  WHERE NOT EXISTS (
    SELECT 1 FROM dbo.ResidentRoles rr WHERE rr.ResidentId = r.ResidentId
  );
END

/* Collect inserted ResidentIds in stable order */
IF OBJECT_ID('tempdb..#SeedIds') IS NOT NULL DROP TABLE #SeedIds;
CREATE TABLE #SeedIds (RowNum INT NOT NULL PRIMARY KEY, ResidentId INT NOT NULL);

WITH Seeded AS (
  SELECT r.ResidentId,
         ROW_NUMBER() OVER (ORDER BY r.ResidentId) AS rn
  FROM dbo.Residents r
  INNER JOIN #SeedResidents s ON s.Email = r.Email
)
INSERT INTO #SeedIds (RowNum, ResidentId)
SELECT rn, ResidentId FROM Seeded;

/* Assign 30 apartments (unique) */
;WITH Apts AS (
  SELECT ApartmentId,
         ROW_NUMBER() OVER (ORDER BY ApartmentId) AS rn
  FROM dbo.Apartments
  WHERE ResidentId IS NULL
),
Res AS (
  SELECT ResidentId, RowNum AS rn
  FROM #SeedIds
  WHERE RowNum BETWEEN 1 AND 30
)
UPDATE a
SET a.ResidentId = r.ResidentId
FROM dbo.Apartments a
INNER JOIN Apts x ON x.ApartmentId = a.ApartmentId
INNER JOIN Res r ON r.rn = x.rn;

/* Assign 10 storage rooms */
;WITH Stor AS (
  SELECT StorageRoomId,
         ROW_NUMBER() OVER (ORDER BY StorageRoomId) AS rn
  FROM dbo.StorageRooms
  WHERE OwnerId IS NULL
),
Res AS (
  SELECT ResidentId, (RowNum - 30) AS rn
  FROM #SeedIds
  WHERE RowNum BETWEEN 31 AND 40
)
UPDATE s
SET s.OwnerId = r.ResidentId
FROM dbo.StorageRooms s
INNER JOIN Stor x ON x.StorageRoomId = s.StorageRoomId
INNER JOIN Res r ON r.rn = x.rn;

/* Assign 10 parking rooms */
;WITH Park AS (
  SELECT ParkingRoomId,
         ROW_NUMBER() OVER (ORDER BY ParkingRoomId) AS rn
  FROM dbo.ParkingRooms
  WHERE OwnerId IS NULL
),
Res AS (
  SELECT ResidentId, (RowNum - 40) AS rn
  FROM #SeedIds
  WHERE RowNum BETWEEN 41 AND 50
)
UPDATE p
SET p.OwnerId = r.ResidentId
FROM dbo.ParkingRooms p
INNER JOIN Park x ON x.ParkingRoomId = p.ParkingRoomId
INNER JOIN Res r ON r.rn = x.rn;

SELECT COUNT(*) AS SeedUsersTotal
FROM dbo.Residents r
WHERE r.Email LIKE @SeedPrefix + N'.%';

