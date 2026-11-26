=====================================================================
 ONLINE SCHOOL ID REPLACEMENT SYSTEM (PORTABLE MULTI-PC EDITION)
 C# / Windows Forms / Auto-SQL & Built-In Local Database
=====================================================================

WHAT WAS UPGRADED FOR ANY PC / LAPTOP
-------------------------------------
1. Zero-Install Database Portability (Data\Database.cs & EmbeddedDb.cs):
   - On startup, the app automatically scans for SQL Server / LocalDB
     ((LocalDB)\MSSQLLocalDB, (LocalDB)\v11.0, .\SQLEXPRESS, localhost).
   - If SQL Server is NOT installed on the PC or laptop, it seamlessly
     switches to a built-in persistent local database stored at:
       %LOCALAPPDATA%\SchoolIdReplacement\SchoolIdReplacementDb.xml
     Every feature (login, requests, approvals, payments, ID printing,
     CSV reports, student management, password changes, audit logs)
     works immediately with zero database setup.
2. One-Click Build & Run Without Visual Studio (Build_And_Run.bat):
   - Double-click Build_And_Run.bat on ANY Windows 10 or Windows 11 PC
     to compile a clean Release executable using Windows' built-in .NET
     compiler and launch the app immediately.
   - Or open SchoolIdReplacement.sln in Visual Studio 2015 or newer and
     press F5.
3. Portable Student Photos & High-DPI Support:
   - Student photos are stored by filename inside %LOCALAPPDATA%\SchoolIdReplacement\Photos
     so paths never break across machines.
   - Added crisp DPI-aware rendering on modern laptop displays.

DEFAULT ACCOUNTS
----------------
  Registrar / Administrator
      Username: admin
      Password: admin123

  Student
      Username: 2024-0001   Password: student123
      Username: 2024-0002   Password: student123
      Username: 2024-0003   Password: student123
=====================================================================
