Install nuget locally (in directory where nupkg file is located)

```
dotnet tool install --global --add-source . ShinyPDF.Previewer --global
```

Run on default port

```
shinypdf-previewer
```

Run on custom port

```
shinypdf-previewer 12500
```

Remove nuget locally 

```
dotnet tool uninstall ShinyPDF.Previewer --global
```