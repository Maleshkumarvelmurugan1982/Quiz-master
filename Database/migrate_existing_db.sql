-- ============================================================
-- RUN THIS ONLY IF YOU ALREADY CREATED QuizAppDB with the OLD schema.sql
-- (a fresh run of the new schema.sql does not need this file)
--
-- 1) Removes the 3 default quizzes and their sample questions
-- 2) Fixes the seeded Admin / Malesh password hash (the old hash did not
--    match "Admin@123", so seeded logins failed)
-- ============================================================
USE QuizAppDB;
GO

-- Remove everything that depends on the default quizzes (Ids 1-3 / seeded titles)
DECLARE @ids TABLE (Id INT);
INSERT INTO @ids
SELECT Id FROM Quizzes
WHERE Title IN ('Java Fundamentals', 'SQL Basics', 'C# Essentials')
  AND CreatedBy = (SELECT Id FROM Users WHERE Email = 'admin@quizapp.com');

DELETE FROM BookmarkedQuestions WHERE QuestionId IN (SELECT Id FROM Questions WHERE QuizId IN (SELECT Id FROM @ids));
DELETE FROM AttemptAnswers      WHERE AttemptId  IN (SELECT Id FROM Attempts  WHERE QuizId IN (SELECT Id FROM @ids));
DELETE FROM Attempts            WHERE QuizId IN (SELECT Id FROM @ids);
DELETE FROM Questions           WHERE QuizId IN (SELECT Id FROM @ids);
DELETE FROM Quizzes             WHERE Id     IN (SELECT Id FROM @ids);
GO

-- Fix seeded password hashes (password = Admin@123)
UPDATE Users
SET PasswordHash = '$2a$11$KZL2lbhC.roQ6hfOXtUieeOUHm0.3cRxYL4EOICRmT5jIx.oT4iMa'
WHERE Email IN ('admin@quizapp.com', 'malesh@quizapp.com');
GO

PRINT 'Default quizzes removed and seeded passwords fixed.';
GO
