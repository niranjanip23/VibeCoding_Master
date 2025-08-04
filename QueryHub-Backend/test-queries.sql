-- QueryHub Database Test Queries
-- Use these queries in VS Code SQLite Viewer

-- 1. View all users with their details
SELECT 
    Id,
    Name,
    Email,
    Department,
    Reputation,
    CreatedAt
FROM Users;

-- 2. View questions with user information
SELECT 
    q.Id,
    q.Title,
    u.Name as AuthorName,
    q.ViewCount,
    q.VoteCount,
    q.CreatedAt
FROM Questions q
JOIN Users u ON q.UserId = u.Id;

-- 3. View question-tag relationships
SELECT 
    q.Title as QuestionTitle,
    t.Name as TagName,
    t.Description as TagDescription
FROM Questions q
JOIN QuestionTags qt ON q.Id = qt.QuestionId
JOIN Tags t ON qt.TagId = t.Id
ORDER BY q.Title;

-- 4. Count records in each table
SELECT 'Users' as TableName, COUNT(*) as RecordCount FROM Users
UNION ALL
SELECT 'Questions', COUNT(*) FROM Questions
UNION ALL
SELECT 'Answers', COUNT(*) FROM Answers
UNION ALL
SELECT 'Tags', COUNT(*) FROM Tags
UNION ALL
SELECT 'Votes', COUNT(*) FROM Votes
UNION ALL
SELECT 'Comments', COUNT(*) FROM Comments
UNION ALL
SELECT 'QuestionTags', COUNT(*) FROM QuestionTags;

-- 5. View database schema (like SSMS sys.tables)
SELECT 
    name as TableName,
    sql as CreateStatement
FROM sqlite_master 
WHERE type='table' 
AND name NOT LIKE 'sqlite_%'
ORDER BY name;

-- 6. Most active users by reputation
SELECT 
    Name,
    Email,
    Department,
    Reputation
FROM Users
ORDER BY Reputation DESC;

-- 7. Questions with their tag counts
SELECT 
    q.Id,
    q.Title,
    COUNT(qt.TagId) as TagCount
FROM Questions q
LEFT JOIN QuestionTags qt ON q.Id = qt.QuestionId
GROUP BY q.Id, q.Title
ORDER BY TagCount DESC;
