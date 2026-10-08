-- ============================================================
-- QUIZ APPLICATION - SQL SERVER EXPRESS SCHEMA
-- ============================================================

USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QuizAppDB')
    DROP DATABASE QuizAppDB;
GO

CREATE DATABASE QuizAppDB;
GO

USE QuizAppDB;
GO

-- ============================================================
-- USERS TABLE
-- ============================================================
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(200) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',  -- 'User' or 'Admin'
    IsActive BIT NOT NULL DEFAULT 1,
    ProfilePicture NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    LastLogin DATETIME2 NULL
);
GO

-- ============================================================
-- CATEGORIES TABLE
-- ============================================================
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    ParentId INT NULL REFERENCES Categories(Id),
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- QUIZZES TABLE
-- ============================================================
CREATE TABLE Quizzes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    CategoryId INT NOT NULL REFERENCES Categories(Id),
    Difficulty NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- 'Easy','Medium','Hard'
    DurationMinutes INT NOT NULL DEFAULT 15,
    TotalMarks INT NOT NULL DEFAULT 0,
    PassingPercentage DECIMAL(5,2) NOT NULL DEFAULT 50.0,
    NegativeMarking BIT NOT NULL DEFAULT 0,
    NegativeMarkValue DECIMAL(5,2) NOT NULL DEFAULT 0.25,
    RandomizeQuestions BIT NOT NULL DEFAULT 0,
    RandomizeOptions BIT NOT NULL DEFAULT 0,
    MaxQuestionsPerAttempt INT NULL,  -- NULL = use all questions
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy INT NOT NULL REFERENCES Users(Id),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME2 NULL
);
GO

-- ============================================================
-- QUESTIONS TABLE
-- ============================================================
CREATE TABLE Questions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    QuizId INT NOT NULL REFERENCES Quizzes(Id) ON DELETE CASCADE,
    QuestionText NVARCHAR(2000) NOT NULL,
    OptionA NVARCHAR(500) NOT NULL,
    OptionB NVARCHAR(500) NOT NULL,
    OptionC NVARCHAR(500) NOT NULL,
    OptionD NVARCHAR(500) NOT NULL,
    CorrectOption CHAR(1) NOT NULL,  -- 'A','B','C','D'
    Marks INT NOT NULL DEFAULT 1,
    Difficulty NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    Explanation NVARCHAR(2000) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- ============================================================
-- QUIZ ATTEMPTS TABLE
-- ============================================================
CREATE TABLE Attempts (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES Users(Id),
    QuizId INT NOT NULL REFERENCES Quizzes(Id),
    Score DECIMAL(10,2) NOT NULL DEFAULT 0,
    TotalMarks INT NOT NULL DEFAULT 0,
    Percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    CorrectAnswers INT NOT NULL DEFAULT 0,
    WrongAnswers INT NOT NULL DEFAULT 0,
    UnansweredQuestions INT NOT NULL DEFAULT 0,
    TimeTakenSeconds INT NOT NULL DEFAULT 0,
    Status NVARCHAR(20) NOT NULL DEFAULT 'InProgress', -- 'InProgress','Passed','Failed'
    AttemptedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL
);
GO

-- ============================================================
-- ATTEMPT ANSWERS TABLE
-- ============================================================
CREATE TABLE AttemptAnswers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AttemptId INT NOT NULL REFERENCES Attempts(Id) ON DELETE CASCADE,
    QuestionId INT NOT NULL REFERENCES Questions(Id),
    SelectedOption CHAR(1) NULL,  -- 'A','B','C','D' or NULL if unanswered
    IsCorrect BIT NOT NULL DEFAULT 0,
    MarksAwarded DECIMAL(5,2) NOT NULL DEFAULT 0
);
GO

-- ============================================================
-- BOOKMARKED QUESTIONS TABLE
-- ============================================================
CREATE TABLE BookmarkedQuestions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL REFERENCES Users(Id),
    QuestionId INT NOT NULL REFERENCES Questions(Id),
    BookmarkedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    UNIQUE(UserId, QuestionId)
);
GO

-- ============================================================
-- SEED: ADMIN USER
-- Password for both users: Admin@123 (bcrypt, cost 11)
-- ============================================================
INSERT INTO Users (Name, Email, PasswordHash, Role)
VALUES ('Admin', 'admin@quizapp.com', '$2a$11$KZL2lbhC.roQ6hfOXtUieeOUHm0.3cRxYL4EOICRmT5jIx.oT4iMa', 'Admin');

INSERT INTO Users (Name, Email, PasswordHash, Role)
VALUES ('Malesh', 'malesh@quizapp.com', '$2a$11$KZL2lbhC.roQ6hfOXtUieeOUHm0.3cRxYL4EOICRmT5jIx.oT4iMa', 'User');
GO

-- ============================================================
-- SEED: CATEGORIES
-- ============================================================
INSERT INTO Categories (Name, Description) VALUES ('Programming', 'Programming Languages and Concepts');
INSERT INTO Categories (Name, Description) VALUES ('Database', 'Database and SQL concepts');
INSERT INTO Categories (Name, Description) VALUES ('Aptitude', 'Quantitative and Logical Aptitude');
INSERT INTO Categories (Name, Description) VALUES ('General Knowledge', 'General Knowledge and Current Affairs');
INSERT INTO Categories (Name, Description) VALUES ('Science', 'Science and Technology');

-- Sub-categories
INSERT INTO Categories (Name, ParentId, Description) VALUES ('Java', 1, 'Java Programming');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('Python', 1, 'Python Programming');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('C#', 1, 'C# and .NET');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('SQL', 2, 'SQL Fundamentals');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('PostgreSQL', 2, 'PostgreSQL Advanced');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('Quantitative', 3, 'Quantitative Aptitude');
INSERT INTO Categories (Name, ParentId, Description) VALUES ('Logical Reasoning', 3, 'Logical Reasoning');
GO

-- ============================================================
-- NOTE: No default quizzes or questions are seeded.
-- Admins create quizzes from the Admin Panel:
--   * "+ New Quiz"          -> build manually
--   * "Generate with AI"    -> from a topic or an uploaded file
-- ============================================================

-- ============================================================
-- USEFUL VIEWS
-- ============================================================
CREATE VIEW vw_QuizDetails AS
SELECT 
    q.Id, q.Title, q.Description, q.Difficulty, q.DurationMinutes,
    q.TotalMarks, q.PassingPercentage, q.IsActive,
    c.Name AS CategoryName,
    c.Id AS CategoryId,
    pc.Name AS ParentCategory,
    (SELECT COUNT(*) FROM Questions WHERE QuizId = q.Id AND IsActive = 1) AS ActiveQuestions,
    (SELECT COUNT(*) FROM Attempts WHERE QuizId = q.Id) AS TotalAttempts,
    u.Name AS CreatedByName,
    q.CreatedAt
FROM Quizzes q
JOIN Categories c ON q.CategoryId = c.Id
LEFT JOIN Categories pc ON c.ParentId = pc.Id
JOIN Users u ON q.CreatedBy = u.Id;
GO

CREATE VIEW vw_LeaderBoard AS
SELECT TOP 100
    u.Id AS UserId,
    u.Name,
    AVG(a.Percentage) AS AverageScore,
    COUNT(a.Id) AS TotalAttempts,
    MAX(a.Percentage) AS HighestScore,
    SUM(CASE WHEN a.Status = 'Passed' THEN 1 ELSE 0 END) AS PassedCount,
    DENSE_RANK() OVER (ORDER BY AVG(a.Percentage) DESC, COUNT(a.Id) DESC) AS Rank
FROM Users u
JOIN Attempts a ON u.Id = a.UserId
WHERE a.Status IN ('Passed', 'Failed')
GROUP BY u.Id, u.Name;
GO

CREATE VIEW vw_AdminDashboard AS
SELECT 
    (SELECT COUNT(*) FROM Users WHERE Role = 'User') AS TotalUsers,
    (SELECT COUNT(*) FROM Quizzes WHERE IsActive = 1) AS TotalQuizzes,
    (SELECT COUNT(*) FROM Questions WHERE IsActive = 1) AS TotalQuestions,
    (SELECT COUNT(*) FROM Attempts WHERE Status != 'InProgress') AS TotalAttempts;
GO

PRINT 'Database schema created successfully!';
GO
