dotnet clean
dotnet publish

docker rm $(docker stop $(docker ps -a -q --filter ancestor=scraper))
docker build . -t scraper
docker run -d scraper