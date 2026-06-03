# 🚀 FinanceAPI (Finze)
A backend Restful API built with .NET 8 and C# to help users track and manage their personal expenses. 
This API provides the backend which has the same logic as the fullstack FinanceApp web application.

## ✨ Key Features
Finze Web API is a system that allows users to:

* Manage their accounts (registering, verifying account, accessing their profile, updating data and resetting password).
* Setting their Budgets.
* Tracking their financial transactions.
* Registering categories for their expenses.
* Adding bill reminders for recurring payments.
* Setting financial goals.
* Adding recurring bills based on frequency of recurrence.
* Managing their financial accounts, this module represents users personal account, business account, and similar accounts.
* Receiving notifications about their account activity or updates.
* Checking different reports related to their data.
* Exporting their data in two formats: excel and pdf.
* Filtering data, retrieving, adding, updating and deleting.

## 🛠 Tech Stack

* **Language:** C#
* **Framework:** .NET 8 / ASP.NET Core WEB API
* **Database:** SQL Server
* **ORM:** Entity Framework Core / LINQ
* **Security:** Authentication (JWT token based, hashed/salted user password), role based authorization (Admin_Role and Customer_Role).
* **Export to files:** Export data to Excel (ClosedXML) or Pdf (QuestPdf).
* **Logging:** Logging to files (Serilog).
* **API Documentation:** Swagger

## 🚀 Getting Started
To run this project locally, follow these steps:

1. **Clone the repo:**
git clone https://github.com/yllka-shala/FinzeWebAPI.git

2. **Open the solution:**
Open `FinanceApi.sln` in Visual Studio.

3. **Open the AppSettings.json file:**
Add your own "Your_Server", "Your_Email", "Your_Password" and "Your_SingingKey".

4. **Migrate database:**
In Package Manager Console add command Update-Database to create the database in MSSQL.

5. **Run:**
Press `F5` to build and launch the app.
