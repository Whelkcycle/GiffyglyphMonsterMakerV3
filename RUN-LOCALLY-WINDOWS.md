# Running your local Monster Maker (Windows)

This branch is configured for a private, local test installation using SQL Server Express LocalDB.

## One-time prerequisites

Install both of these on your Windows computer:

1. **.NET 9 SDK** (not just the runtime): https://dotnet.microsoft.com/en-us/download/dotnet/9.0
2. **SQL Server Express LocalDB**: install the LocalDB component from Microsoft's SQL Server Express download page: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

Restart Windows if either installer asks you to.

## Start the app

1. Download the ZIP for the `custom-monster-maker` branch:
   https://github.com/Whelkcycle/GiffyglyphMonsterMakerV3/archive/refs/heads/custom-monster-maker.zip
2. Extract the ZIP somewhere convenient, such as `Documents\MonsterMaker`. Do not run it from inside the ZIP preview.
3. Open the extracted folder.
4. Double-click **Start-MonsterMaker.bat**.
5. Wait for the first-run build and database setup to finish. The browser should open at http://localhost:5080. If it does not, open that address yourself.

The first launch creates a database named `GiffyglyphMonsterMakerV3_Local` in your Windows LocalDB instance and applies the custom database additions. Your monsters and templates stay on this computer.

## First login

Choose **Register** and create your own account. Local development does not require email confirmation or an SMTP account. Your account is stored in your local database.

## Every later launch

Double-click **Start-MonsterMaker.bat** again. Keep the black command window open while using the app. Close that window when you're finished.

## Troubleshooting

- If it says .NET 9 SDK is missing, install the SDK (the SDK includes what is needed to build and run the app).
- If it says LocalDB is missing, install SQL Server Express LocalDB.
- If the browser opens before the app is ready, wait a few seconds and refresh http://localhost:5080.
- If the command window shows an error, copy the error text and send it to me.

This local setup uses Development mode, a local-only database, and automatic account sign-in. The normal production email-confirmation setting remains enabled outside Development mode.
