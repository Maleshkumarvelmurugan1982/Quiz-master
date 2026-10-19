-- ============================================================
-- Adds the student Section (A / B / C) column to an EXISTING QuizAppDB.
-- You normally do NOT need to run this: the app adds the column
-- automatically when it starts. It is here in case you want to do it by hand.
-- ============================================================
USE QuizAppDB;
GO

IF COL_LENGTH('Users', 'Section') IS NULL
    ALTER TABLE Users ADD Section NVARCHAR(5) NULL;
GO

PRINT 'Section column is ready.';
GO
