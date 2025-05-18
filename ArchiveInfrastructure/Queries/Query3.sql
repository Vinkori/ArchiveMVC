SELECT DISTINCT f.Id, f.FormName
FROM Forms AS f
JOIN FormsPoetry AS fp ON f.Id = fp.FormID
JOIN Poetry AS p ON fp.PoetryID = p.Id
WHERE p.PublicationDate > @AfterDate
ORDER BY f.FormName;