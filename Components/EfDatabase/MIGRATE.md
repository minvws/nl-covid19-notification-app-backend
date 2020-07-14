# DB Migration instructions

After you've made changes to the DbContext entities you'll have to add new db migrations for deployment purposes.

Open a terminal in the root of the solution and run the following commands for your OS:

## Windows

 
```bash
Add-Migration {MigrationDescription} -Context {DbContext} -OutputDir Migrations\{DbContext} -StartupProject DatabaseProvisioningTool -Project Components 
```
## MacOS

```bash
dotnet ef migrations add {MigrationDescription} -c {DbContext} -o Migrations/{DbContext} -s DatabaseProvisioningTool --project Components
```

### Example for WorkflowDbContext

```bash
 dotnet ef migrations add {MigrationDescription} -c WorkflowDbContext -o Migrations/WorkflowDbContext -s DatabaseProvisioningTool --project Components
```

