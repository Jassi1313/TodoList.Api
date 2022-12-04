# Todo Items API
This part of the repository contains the backend code for Clearpoint Full Stack Developer assessment test

## Stack used
1. .NET 5 API
2. EF Core with In-memory database
3. Automapper
4. XUnit + MOQ

## How to run the app
Run the app using Visual Studio or dotnet cli
The API swagger file will open on port 3002

> ℹ️ **If you want to run the app along with API via Docker see the Readme instructions for the repository**

## Unit tests
Runs following unit tests using XUnit and MOQ
> ℹ️ **As there is no business logic in business layer I just wrote tests for Controllers. However, ideally; there will be tests for business layer too and Integration tests where we call actual repository and data store**

![image](https://user-images.githubusercontent.com/20395556/140626279-f1dc69b5-0b80-415d-8104-1c9949dd0191.png)

