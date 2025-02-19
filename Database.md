create database project_sa_web
go
use project_sa_web
go

CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password_hash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Admin', 'Employee'))
);
INSERT INTO Users (Username, Password_hash, Role) 
VALUES ('admin', '123', 'Admin');
INSERT INTO Users (Username, Password_hash, Role) 
VALUES ('Moncef', '123', 'Employee');
SELECT * FROM Users;
GO
