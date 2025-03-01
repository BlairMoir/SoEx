# SoEx Example101

InProc only example combined with RazorPages

Demonstrates:
* Populating AuthContext using filter
* Mocking service within a test
* Mocking service dependency within a test


Make sure you have dotnet-ef tools installed

run
```
dotnet tool install --global dotnet-ef
```

run database migrations

run
```
cd Host/Web 
dotnet ef database update
```