# Sphinx

A web application for managing Delta Sigma Phi (Delta Epsilon) fraternity operations.

## Contributing

Following the instructions below in order to get the website running locally and contribute changes.

### Installations

- Visual Studio Community Edition (latest)
- SQL Server 2016 Express 
  - Download and install the Express edition from [here][2]
  - Make sure you get SQL Server Management Studio too

### Clone & Build

Clone the repository with git and open the solution in Visual Studio.
Build the solution to download all the required NuGet packages, which may take a few minutes.

### Building Your Local Databases

This application uses [Entity Framework Code-First][1] to handle database interaction and management.
After you've installed everything, cloned the repo, and built the solution, open the NuGet Package Manager Console in Visual Studio and run the following commands:

~~~ sh
PM> Enable-Migrations -ContextTypeName Dsp.Data.SphinxDbContext -ProjectName Dsp.Data
PM> Update-Database -ConfigurationTypeName Dsp.Data.Migrations.Configuration -ProjectName Dsp.Data
~~~

If you receive an error about the database not existing, you will need to modify the connection string in the root `Web.config` to point to a fresh, local sql server database that you will need to create.
Get the latest version of SQL Server Management Studio to create a local database.

Entity Framework *migrations* exist to keep track of every change made to the database schema over time.
The first command simply turns on the ability to use migrations.
The second command creates a new migration based on anything that has changed since the previous migration.
The third command actually updates your local database by applying any unapplied migrations.

Contact Ty if you get stuck.

### Data
The `Seed` method in `Migrations/Configuration.cs` contains some commented out code that serves as a good starting place.
If you are working with Ty on the Delta Sigma Phi website, contact him for some recent data to throw in your local database.
To make yourself an administrator, run the following SQL query on the DspDb:

~~~ sql
INSERT INTO Leaders VALUES (/*your user id from Members table*/, 1, 30, '2014-01-01 12:00:00.000')
~~~

### Contributing

1. Create your feature branch: `git checkout -b my-new-feature`
2. Commit your changes: `git commit -am 'Add some feature'`
3. Push to the branch: `git push origin my-new-feature`
4. Submit a pull request

The idea is to do your work on a branch other than `master` and then submit a pull request for that branch to be merged into master on the upstream repo.

## Deployment

The easiest way is to download the Web Publish profile from the host to somewhere on your computer and use it.
Right-click on the `Dsp` project and select Publish.
In the Publish prompt, import the profile you downloaded and click through the rest of the sub-menus.
If there are new database migrations since the last time you published, make sure the "Execute Code-First Migrations" box is checked.

## Feedback
Use the issues feature of this repository to report bugs or make suggestions/requests for new features, or enhancements to existing ones.

[1]: http://www.entityframeworktutorial.net/code-first/entity-framework-code-first.aspx
[2]: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
