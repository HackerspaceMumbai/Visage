SELECT TOP 10 Id, Email, IsProfileComplete, ProfileCompletedAt, FirstName, LastName,
       MobileNumber, AddressLine1, City, State, PostalCode, GovtIdLast4Digits,
       GovtIdType, OccupationStatus
FROM Registrants
WHERE Email = ''
ORDER BY ProfileCompletedAt DESC;
