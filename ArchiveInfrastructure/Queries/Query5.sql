SELECT p.Id, p.Title, p.Text, p.PublicationDate, 
       CONCAT(a.FirstName, ' ', a.LastName) AS AuthorName, 
       l.Language AS LanguageName,
       u.UserName AS AdminName
FROM Poetry AS p
JOIN Authors AS a ON p.AuthorID = a.Id
JOIN AspNetUsers AS u ON p.AddedByUserId = u.Id
JOIN AspNetUserRoles AS ur ON u.Id = ur.UserId
JOIN AspNetRoles AS r ON ur.RoleId = r.Id
JOIN Languages AS l ON p.LanguageID = l.Id
WHERE r.Name = 'Admin'
  AND u.UserName = @AdminName
  AND l.Language = @LanguageName
ORDER BY p.PublicationDate DESC;