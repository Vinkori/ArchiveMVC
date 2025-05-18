SELECT p.Id, p.Title, p.Text, p.PublicationDate, 
       CONCAT(a.FirstName, ' ', a.LastName) AS AuthorName, 
       l.Language AS LanguageName,
       u.UserName AS AdminName
FROM Poetry AS p
JOIN Authors AS a ON p.AuthorID = a.Id
JOIN Languages AS l ON p.LanguageID = l.Id
JOIN AspNetUsers AS u ON p.AddedByUserId = u.Id
LEFT JOIN PoetryLikes AS pl ON p.Id = pl.PoetryID
WHERE a.LastName = @LastName
  AND p.Title LIKE '%' + @Keyword + '%'
GROUP BY p.Id, p.Title, p.Text, p.PublicationDate, a.FirstName, a.LastName, l.Language, u.UserName
HAVING COUNT(pl.UserID) >= @MinLikes
ORDER BY p.PublicationDate;