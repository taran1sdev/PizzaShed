### Requirements
- .NET8+
- SQL Server 2022
- SQL Server Management Studio

### Dependencies
- MaterialDesignThemes (5.2.1)
- MaterialDesignThemes (1.0.1)
- Microsoft.Data.SqlClient (6.1.1)

### Build

The release build of the application can be found in `PizzaShed` -> `bin` -> `Release` -> `net8.0-windows` -> `PizzaShed.exe`.

If the application fails to launch the `PizzaShed.sln` file can be opened in Visual Studio or alternatively the application can be built from source by cloning the [PizzaShed](https://github.com/taran1sdev/PizzaShed.git) repository.

## Setup

Run `PizzaShed` -> `SQL` -> `init_db.sql` in SQL Server Management Studio. This query will 
- Create the PizzaShed database
- Create the PizzaShedDB login used by the application
- Create the Database schema
- Create the Types and Stored Procedures required by the application
- Populate the Database 

This script can be executed again to reset the database - alternatively `drop_tables.sql`, `create_schema.sql` and `insert_data.sql` can be run.
### Login Info

The PIN's for each role are as follows:
- Manager: 0000
- Cashier: 0001
- Pizzaiolo: 0002
- Grill Cook: 0003
- Driver: 0004

### Logs
The application generates a log file that can be found in the `%AppData%\Roaming\` directory.

### Notes

The `ManagerView` has not yet been implemented, logging in as the Manager will redirect to the cashier view. It is not possible to navigate to the "Collections" when logged in as the Manager.

If using the application outside of the opening hours found in the brief it will not be possible to create new orders.  

If using the application during "Peak Hours" (18:00 - 21:00) please note that there may be a delay between orders being created and appearing in the Order View, this is an intended feature but can be changed by editing the source code:

`PizzaShed` -> `Services` -> `Data` -> `OrderRepository.cs`

Line 1015 :
```cs
// Change this line
TimeSpan prepTime = now > times.PeakStart && now < times.PeakEnd ? new TimeSpan(00,25,00) : new TimeSpan(00,15,00);

// To this
TimeSpan prepTime = TimeSpan(00,15,00);
```
