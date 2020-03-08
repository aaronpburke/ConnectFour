
# Connect Four - ASP.NET Core 3.1 Server

Back-end API server for a \"Connect Four\"-style game.

## How to Build

### Linux/OS X

```
sh build.sh
```

### Windows

```
build.bat
```

### Docker

```
cd src
docker build -t connectfour.api .
docker run -p 5000:5000 connectfour.api
```

## API documentation

API documentation can be found by running the project and navigating to /swagger of the server root. The API follows the OpenAPI v3 specification. The OpenAPI document can be found at /swagger/1.0.0/swagger.json and is a dynamically created document. Client libraries can be generated from this specification using any OpenAPI-compatible tool, such as the [Swagger Editor](https://editor.swagger.io/).

## Known Issues

 - Database layer implementation may not allow for clustered application deployment due to a bug in internal caching optimizations.
