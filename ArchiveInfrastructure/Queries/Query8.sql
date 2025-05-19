SELECT u.Id, u.UserName
FROM AspNetUsers AS u
WHERE NOT EXISTS (
    SELECT p.Id
    FROM Poetry AS p
    WHERE p.AuthorID = (SELECT Id FROM Authors WHERE LastName = @LastName)
    EXCEPT
    SELECT pl.PoetryID
    FROM PoetryLikes AS pl
    WHERE pl.UserID = u.Id
)
ORDER BY u.UserName;