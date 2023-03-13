# Building the documentation

> [!IMPORTANT]
> To install the server, please read the [server documentation](server.md).

> [!CAUTION]
> Documentation is built off of Documentation Comments. You __MUST__ include these. See [the C# documentation](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments#d31-general) for more information.

## Viewing the documentation

The documentation is available on [GitHub Pages](https://wotanut.github.io/NEA/) for this project. To view the documentation locally, please build the documentation from source.

## Building the documentation from source

Assuming you have the .NET 6.0 SDK or higher, install docfx:

``` dotnet tool update -g docfx ```

Then build the documentation by running the following command from the root of the repository:

``` docfx docs/docfx.json ```

> [!TIP]
> If your console freezes __after__ building the documentation, press enter.

# Serving the documentation

To serve the documentation locally, run the following command from the root of the repository:

``` docfx docs/docfx.json --serve ```

> [!WARNING]
> You must have __**built**__ the documentation before serving it. You can also serve it directly and it will build before serving.