USE HR_DB;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetStatuses
    AS
BEGIN
  SET NOCOUNT ON;
SELECT id, name FROM dbo.[status] ORDER BY name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetDepartments
    AS
BEGIN
  SET NOCOUNT ON;
SELECT id, name FROM dbo.deps ORDER BY name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetPositions
    AS
BEGIN
  SET NOCOUNT ON;
SELECT id, name FROM dbo.posts ORDER BY name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployees
    @StatusId INT = NULL,
    @DepartmentId INT = NULL,
    @PositionId INT = NULL,
    @LastNameFilter NVARCHAR(100) = NULL
    AS
BEGIN
  SET NOCOUNT ON;
SELECT p.id,
       CONCAT(p.last_name,' ',LEFT(p.first_name,1),'. ',LEFT(p.second_name,1),'.') AS Fio,
       s.name AS StatusName,
       d.name AS DepartmentName,
       pos.name AS PostName,
       p.date_employ,
       p.date_unemploy
FROM dbo.persons p
         JOIN dbo.[status] s ON s.id = p.[status]
    JOIN dbo.deps    d ON d.id = p.id_dep
    JOIN dbo.posts  pos ON pos.id = p.id_post
WHERE (@StatusId IS NULL     OR p.[status] = @StatusId)
  AND (@DepartmentId IS NULL OR p.id_dep   = @DepartmentId)
  AND (@PositionId IS NULL   OR p.id_post  = @PositionId)
  AND (@LastNameFilter IS NULL OR p.last_name LIKE N'%'+@LastNameFilter+N'%')
ORDER BY p.last_name, p.first_name, p.second_name;
END;
GO

CREATE OR ALTER PROCEDURE dbo.usp_GetEmployeeStatistics
    @StatusId INT,
    @FromDate DATE,
    @ToDate   DATE,
    @IsHiredStats BIT
    AS
BEGIN
  SET NOCOUNT ON;
  IF @IsHiredStats = 1
SELECT p.date_employ AS [Date], COUNT(*) AS [Count]
FROM dbo.persons p
WHERE p.[status] = @StatusId
  AND p.date_employ IS NOT NULL
  AND p.date_employ BETWEEN @FromDate AND @ToDate
GROUP BY p.date_employ
ORDER BY p.date_employ;
ELSE
SELECT p.date_unemploy AS [Date], COUNT(*) AS [Count]
FROM dbo.persons p
WHERE p.[status] = @StatusId
  AND p.date_unemploy IS NOT NULL
  AND p.date_unemploy BETWEEN @FromDate AND @ToDate
GROUP BY p.date_unemploy
ORDER BY p.date_unemploy;
END;
GO
