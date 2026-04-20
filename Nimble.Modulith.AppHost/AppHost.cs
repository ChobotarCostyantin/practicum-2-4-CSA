var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server database for Identity
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var usersDb = sqlServer.AddDatabase("usersdb");
var productsDb = sqlServer.AddDatabase("productsdb");
var customersDb = sqlServer.AddDatabase("customersdb");

// Add the Web API project with database reference
builder.AddProject<Projects.Nimble_Modulith_Web>("webapi")
    .WithReference(usersDb)
    .WithReference(productsDb)
    .WithReference(customersDb)
    .WaitFor(usersDb)
    .WaitFor(productsDb)
    .WaitFor(customersDb);

builder.Build().Run();