# FunBooksAndVideos


## Start the project


```bash
# setup MS SQL Server
docker compose up -d


# run app
dotnet restore
dotnet build
dotnet run --project src/FunBooksAndVideos.Api
```


## clean up


```bash
docker compose down -v
```
