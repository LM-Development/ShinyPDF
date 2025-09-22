Install nuget locally (in directory where nupkg file is located)

```
dotnet tool install --global --add-source . OpenQuestPDF.Previewer --global
```

Run on default port

```
questpdf-previewer
```

Run on custom port

```
questpdf-previewer 12500
```

Remove nuget locally 

```
dotnet tool uninstall OpenQuestPDF.Previewer --global
```