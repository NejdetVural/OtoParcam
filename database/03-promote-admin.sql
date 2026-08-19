-- OtoParcam — bootstrap the first Administrator account
--
-- There is no seeded admin user and no `PasswordHash` is written here on purpose: ASP.NET
-- Core Identity's password hash is a PBKDF2 digest computed with a random per-user salt, and
-- hand-crafting one outside `UserManager` is fragile and not worth the risk of getting the
-- format wrong. Instead:
--
--   1. Register a normal account through the app (POST /api/v1/auth/register, or the /kayit
--      page) with the email you want to use as the first admin.
--   2. Edit @TargetEmail below to that email.
--   3. Run this script. It confirms the email (there's no real email sending yet — see
--      auth-known-gaps notes) and grants the Administrator role. A user with both
--      Administrator and Customer (every account keeps Customer too, see
--      admin-role-adaptations) can then log in and use /admin/musteriler to promote/demote
--      any other account through the UI from then on — you only need this script once.
--
-- Run with:
--   sqlcmd -S "(localdb)\mssqllocaldb" -d OtoParcamDb -i database\03-promote-admin.sql -f 65001

SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @TargetEmail NVARCHAR(256) = N'CHANGE_ME@example.com';

IF @TargetEmail = N'CHANGE_ME@example.com'
BEGIN
    RAISERROR(N'Edit @TargetEmail in this script to the account you registered before running it.', 16, 1);
    RETURN;
END

IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @TargetEmail)
BEGIN
    RAISERROR(N'No registered account found with that email — register through the app first.', 16, 1);
    RETURN;
END

UPDATE AspNetUsers
SET EmailConfirmed = 1
WHERE Email = @TargetEmail;

INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u
CROSS JOIN AspNetRoles r
WHERE u.Email = @TargetEmail
  AND r.Name = N'Administrator'
  AND NOT EXISTS (
      SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
  );

PRINT N'Account promoted to Administrator and email confirmed: ' + @TargetEmail;
