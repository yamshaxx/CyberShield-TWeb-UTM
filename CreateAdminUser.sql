-- SQL Script to add an admin user to the CyberShield database
-- Note: This uses a pre-hashed password 'Admin123!' with System.Web.Helpers.Crypto.HashPassword()
-- The password hash below corresponds to 'Admin123!'

-- Check if admin user exists
IF NOT EXISTS (SELECT * FROM Users WHERE IsAdmin = 1)
BEGIN
    -- Insert admin user
    INSERT INTO Users (Username, Email, PasswordHash, IsAdmin)
    VALUES ('admin', 'admin@cybershield.com', 'ALzCVecLdWVXvAuE1PSJpPKOcRJsQnhwZkQEwKjpAXgdQNZ9oBNEhdgJiS8smQ1WVg==', 1)
    
    PRINT 'Admin user created successfully!'
END
ELSE
BEGIN
    PRINT 'Admin user already exists!'
    
    -- Display existing admin users
    SELECT Username, Email FROM Users WHERE IsAdmin = 1
END

-- To run this script:
-- 1. Open SQL Server Management Studio
-- 2. Connect to the LocalDB instance (typically (LocalDb)\MSSQLLocalDB)
-- 3. Select the CyberShieldDb database
-- 4. Execute this script 