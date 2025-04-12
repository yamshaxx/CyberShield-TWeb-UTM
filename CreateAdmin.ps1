# PowerShell script to add an admin user to CyberShield application
# This script updates an existing user to have admin rights

# Set SQL connection parameters
$sqlInstance = "(LocalDb)\MSSQLLocalDB"
$database = "CyberShieldDb"
$mdfPath = Join-Path $PWD "CyberShieldWeb\App_Data\CyberShieldDb.mdf"

# Check if database file exists
if (-not (Test-Path $mdfPath)) {
    Write-Host "Database file not found at $mdfPath" -ForegroundColor Red
    Write-Host "Please run the application at least once to create the database before running this script." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit
}

# Function to execute SQL query
function Execute-SqlQuery {
    param (
        [string]$query
    )
    
    $connectionString = "Data Source=$sqlInstance;Initial Catalog=$database;Integrated Security=True;AttachDBFilename=$mdfPath"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $command = New-Object System.Data.SqlClient.SqlCommand($query, $connection)
    
    try {
        $connection.Open()
        $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($command)
        $dataset = New-Object System.Data.DataSet
        $adapter.Fill($dataset) | Out-Null
        return $dataset.Tables[0]
    }
    catch {
        Write-Host "Error executing SQL query: $_" -ForegroundColor Red
        return $null
    }
    finally {
        $connection.Close()
    }
}

# Check for existing users
$usersQuery = "SELECT Id, Username, Email, IsAdmin FROM Users"
$users = Execute-SqlQuery -query $usersQuery

if ($users -eq $null -or $users.Rows.Count -eq 0) {
    Write-Host "No users found in the database." -ForegroundColor Yellow
    Write-Host "Please register a user through the application first, then run this script to grant admin rights." -ForegroundColor Yellow
    Read-Host "Press Enter to exit"
    exit
}

# List existing users
Write-Host "Existing users:" -ForegroundColor Cyan
foreach ($user in $users.Rows) {
    $adminStatus = if ($user.IsAdmin) { "[ADMIN]" } else { "" }
    Write-Host "$($user.Id): $($user.Username) - $($user.Email) $adminStatus"
}

# Check if admin user already exists
$adminExists = $false
foreach ($user in $users.Rows) {
    if ($user.IsAdmin) {
        $adminExists = $true
        break
    }
}

if ($adminExists) {
    Write-Host "An admin user already exists in the database." -ForegroundColor Green
    $createAnother = Read-Host "Do you want to create another admin user? (y/n)"
    if ($createAnother -ne "y") {
        exit
    }
}

# Prompt for user ID to make admin
$userId = Read-Host "Enter the ID of the user you want to make admin"

# Update user to be admin
$updateQuery = "UPDATE Users SET IsAdmin = 1 WHERE Id = $userId"
Execute-SqlQuery -query $updateQuery

# Verify the update
$verifyQuery = "SELECT Username, Email FROM Users WHERE Id = $userId AND IsAdmin = 1"
$verifiedUser = Execute-SqlQuery -query $verifyQuery

if ($verifiedUser -ne $null -and $verifiedUser.Rows.Count -gt 0) {
    Write-Host "User '$($verifiedUser.Rows[0].Username)' has been updated to have admin rights." -ForegroundColor Green
    Write-Host "You can now log in with this account and access the admin dashboard at /Admin" -ForegroundColor Green
}
else {
    Write-Host "Failed to update user to admin. Please try again." -ForegroundColor Red
}

Read-Host "Press Enter to exit" 