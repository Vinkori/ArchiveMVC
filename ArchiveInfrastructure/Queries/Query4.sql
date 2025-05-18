SELECT DISTINCT u.Id, u.UserName, u.Email
FROM AspNetUsers AS u
JOIN PoetryLikes AS pl ON u.Id = pl.UserID
JOIN FormsPoetry AS fp ON pl.PoetryID = fp.PoetryID
JOIN Forms AS f ON fp.FormID = f.Id
WHERE f.FormName = @FormName
ORDER BY u.UserName;