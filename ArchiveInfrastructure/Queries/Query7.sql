SELECT u1.Id AS UserA, u2.Id AS UserB
FROM AspNetUsers AS u1
JOIN AspNetUsers AS u2
  ON u1.Id < u2.Id
WHERE 
  NOT EXISTS (
    SELECT pl1.PoetryID
    FROM PoetryLikes AS pl1
    WHERE pl1.UserID = u1.Id
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
    WHERE pl1.UserID = u1.Id
  );