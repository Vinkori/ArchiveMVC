SELECT u2.Id, u2.UserName
FROM AspNetUsers AS u2
WHERE u2.UserName <> @UserName
  AND NOT EXISTS (
      SELECT pl1.PoetryID
      FROM PoetryLikes AS pl1
      WHERE pl1.UserID = (SELECT Id FROM AspNetUsers WHERE UserName = @UserName)
      EXCEPT
      SELECT pl2.PoetryID
      FROM PoetryLikes AS pl2
      WHERE pl2.UserID = u2.Id
  )
  AND NOT EXISTS (
      SELECT pl2.PoetryID
      FROM PoetryLikes AS pl2
      WHERE pl2.UserID = u2.Id
      EXCEPT
      SELECT pl1.PoetryID
      FROM PoetryLikes AS pl1
      WHERE pl1.UserID = (SELECT Id FROM AspNetUsers WHERE UserName = @UserName)
  )
ORDER BY u2.UserName;