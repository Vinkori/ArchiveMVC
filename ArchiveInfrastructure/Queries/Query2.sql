SELECT DISTINCT a.Id, a.FirstName, a.LastName
FROM Authors AS a
JOIN Poetry AS p ON p.AuthorID = a.Id
JOIN Languages AS l ON p.LanguageID = l.Id
WHERE l.Language = @LanguageName
ORDER BY a.LastName, a.FirstName;