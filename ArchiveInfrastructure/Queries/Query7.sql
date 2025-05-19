SELECT DISTINCT a2.Id, a2.FirstName, a2.LastName
FROM Authors AS a2
WHERE a2.LastName <> @LastName
  AND NOT EXISTS (
      SELECT f.Id
      FROM Forms AS f
      JOIN FormsPoetry AS fp ON f.Id = fp.FormID
      JOIN Poetry AS p ON fp.PoetryID = p.Id
      JOIN Authors AS a ON p.AuthorID = a.Id
      WHERE a.LastName = @LastName
      EXCEPT
      SELECT f2.Id
      FROM Forms AS f2
      JOIN FormsPoetry AS fp2 ON f2.Id = fp2.FormID
      JOIN Poetry AS p2 ON fp2.PoetryID = p2.Id
      WHERE p2.AuthorID = a2.Id
  )
  AND NOT EXISTS (
      SELECT f2.Id
      FROM Forms AS f2
      JOIN FormsPoetry AS fp2 ON f2.Id = fp2.FormID
      JOIN Poetry AS p2 ON fp2.PoetryID = p2.Id
      WHERE p2.AuthorID = a2.Id
      EXCEPT
      SELECT f.Id
      FROM Forms AS f
      JOIN FormsPoetry AS fp ON f.Id = fp.FormID
      JOIN Poetry AS p ON fp.PoetryID = p.Id
      JOIN Authors AS a ON p.AuthorID = a.Id
      WHERE a.LastName = @LastName
  )
ORDER BY a2.LastName, a2.FirstName;