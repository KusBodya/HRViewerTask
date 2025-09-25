
IF DB_ID(N'HR_DB') IS NULL
BEGIN
    CREATE DATABASE HR_DB;
END;
GO
USE HR_DB;
GO

IF OBJECT_ID(N'dbo.[status]', N'U') IS NULL
CREATE TABLE dbo.[status](
                             id   INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE
    );

IF OBJECT_ID(N'dbo.deps', N'U') IS NULL
CREATE TABLE dbo.deps(
                         id   INT IDENTITY(1,1) PRIMARY KEY,
                         name NVARCHAR(200) NOT NULL UNIQUE
);

IF OBJECT_ID(N'dbo.posts', N'U') IS NULL
CREATE TABLE dbo.posts(
                          id   INT IDENTITY(1,1) PRIMARY KEY,
                          name NVARCHAR(200) NOT NULL UNIQUE
);

IF OBJECT_ID(N'dbo.persons', N'U') IS NULL
CREATE TABLE dbo.persons(
                            id            INT IDENTITY(1,1) PRIMARY KEY,
                            last_name     NVARCHAR(100) NOT NULL,
                            first_name    NVARCHAR(100) NOT NULL,
                            second_name   NVARCHAR(100) NULL,
    [status]      INT NOT NULL FOREIGN KEY REFERENCES dbo.[status](id),
    id_dep        INT NOT NULL FOREIGN KEY REFERENCES dbo.deps(id),
    id_post       INT NOT NULL FOREIGN KEY REFERENCES dbo.posts(id),
    date_employ   DATE NULL,
    date_unemploy DATE NULL
    );
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_status' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_status       ON dbo.persons([status]);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_dep' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_dep          ON dbo.persons(id_dep);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_post' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_post         ON dbo.persons(id_post);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_lastname' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_lastname     ON dbo.persons(last_name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_date_employ' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_date_employ  ON dbo.persons(date_employ);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_persons_date_unemploy' AND object_id=OBJECT_ID('dbo.persons'))
CREATE INDEX IX_persons_date_unemploy ON dbo.persons(date_unemploy);
GO

INSERT INTO dbo.[status](name)
SELECT v.name
FROM (VALUES (N'Активен'),(N'Уволен'),(N'В отпуске')) AS v(name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.[status] s WHERE s.name = v.name);

INSERT INTO dbo.deps(name)
SELECT v.name
FROM (VALUES (N'Отдел разработки'),(N'Тестирование'),(N'Кадры'),(N'Бухгалтерия')) AS v(name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.deps d WHERE d.name = v.name);

INSERT INTO dbo.posts(name)
SELECT v.name
FROM (VALUES (N'Junior разработчик'),(N'Middle разработчик'),
             (N'Test Engineer'),(N'HR-специалист'),(N'Бухгалтер')) AS v(name)
WHERE NOT EXISTS (SELECT 1 FROM dbo.posts p WHERE p.name = v.name);
GO

WITH v(last_name, first_name, second_name, status_name, dep_name, post_name, date_employ, date_unemploy) AS (
    SELECT N'Иванов',   N'Иван',     N'Иванович',  N'Активен', N'Отдел разработки', N'Junior разработчик',  CAST('2025-08-20' AS date), NULL UNION ALL
    SELECT N'Сидорова', N'Мария',    N'Алексеевна',N'Активен', N'Тестирование',     N'Test Engineer',       CAST('2025-09-10' AS date), NULL UNION ALL
    SELECT N'Петров',   N'Петр',     N'Петрович',  N'Уволен',  N'Кадры',            N'HR-специалист',       CAST('2025-06-20' AS date), CAST('2025-09-19' AS date) UNION ALL
    SELECT N'Кузнецов', N'Алексей',  N'Сергеевич', N'Активен', N'Отдел разработки', N'Middle разработчик',  CAST('2025-07-15' AS date), NULL UNION ALL
    SELECT N'Романова', N'Анна',     N'Викторовна',N'В отпуске',N'Бухгалтерия',     N'Бухгалтер',           CAST('2025-05-05' AS date), NULL UNION ALL
    SELECT N'Смирнов',  N'Дмитрий',  N'Андреевич', N'Активен', N'Отдел разработки', N'Junior разработчик',  CAST('2025-09-01' AS date), NULL UNION ALL
    SELECT N'Орлова',   N'Екатерина',N'Игоревна',  N'Активен', N'Тестирование',     N'Test Engineer',       CAST('2025-08-05' AS date), NULL UNION ALL
    SELECT N'Васильев', N'Никита',   N'Олегович',  N'Уволен',  N'Кадры',            N'HR-специалист',       CAST('2025-03-10' AS date), CAST('2025-07-01' AS date) UNION ALL
    SELECT N'Гусева',   N'Ольга',    N'Сергеевна', N'Активен', N'Бухгалтерия',      N'Бухгалтер',           CAST('2025-04-12' AS date), NULL UNION ALL
    SELECT N'Морозов',  N'Егор',     N'Алексеевич',N'Активен', N'Отдел разработки', N'Middle разработчик',  CAST('2025-02-28' AS date), NULL
)
INSERT INTO dbo.persons(last_name, first_name, second_name, [status], id_dep, id_post, date_employ, date_unemploy)
SELECT v.last_name, v.first_name, v.second_name,
       s.id, d.id, p.id,
       v.date_employ, v.date_unemploy
FROM v
         JOIN dbo.[status] s ON s.name = v.status_name
    JOIN dbo.deps   d  ON d.name = v.dep_name
    JOIN dbo.posts  p  ON p.name = v.post_name
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.persons x
    WHERE x.last_name = v.last_name
  AND x.first_name = v.first_name
  AND ISNULL(x.second_name,N'') = ISNULL(v.second_name,N'')
  AND x.date_employ = v.date_employ
    );
GO
