# Development

## Building the app for MacOS :

###[Documentation](https://docs.avaloniaui.net/docs/distribution-publishing/macos)

```sh
dotnet msbuild -t:BundleApp -p:RuntimeIdentifier=osx-x64 -property:Configuration=Release -p:UseAppHost=true  -p:CFBundleShortVersionString=0.1
```

## Requirements for `Info.plist`

### [Documentation](https://docs.avaloniaui.net/docs/distribution-publishing/macos)

* The value of `CFBundleExecutable` matches the binary name generated by `dotnet publish` -- typically this is the same
  as your `.dll` assembly name __without__ `.dll`.
* `CFBundleName` is set to the display name for your application. If this is longer than 15 characters, set
  CFBundleDisplayName too.
* `CFBundleIconFile` is set to the name of your `icns` icon file (including extension)
* `CFBundleIdentifier` is set to a unique identifier, typically in reverse-DNS format -- e.g. `com.myapp.macos`.
* `NSHighResolutionCapable` is set to true (`<true/>` in the `Info.plist`).
* `CFBundleVersion` is set to the version for your bundle, e.g. 1.4.2.
* `CFBundleShortVersionString` is set to the user-visible string for your application's version,
  e.g. `Major.Minor.Patch`.