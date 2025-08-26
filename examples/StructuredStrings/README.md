# Ubiq Security .NET Core String Encrypt/Decypt Sample

This sample application will demonstrate how to encrypt and decrypt string arguments
using the structured data API.

## Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Prerequisites

* [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Build

Open a command shell and change to the ```StructuredStrings``` folder:

```sh
cd StructuredStrings
dotnet build -c Release
```

## View Program Options

Open a command shell and change to the ```StructuredStrings``` folder:

```sh
cd StructuredStrings
dotnet bin\Release\net6.0\StructuredStrings.dll --help
```

<pre>
  -e, --encrypttext    Set the field text value to encrypt and will return the encrypted cipher text
  -d, --decrypttext    Set the cipher text value to decrypt and will return the decrypted text
  -n, --ffsname        Set the ffs name, for example SSN
  -h, --help           (Default: false) Print app parameter summary
  -c, --creds          Set the file name with the API credentials
  -P, --profile        (Default: default) Identify the profile within the credentials file
  -V, --version        Show program's version number and exit
  --help               Display this help screen.
  --version            Display version information.
</pre>


## Example Usage 

Demonstrate encrypting a social security number and returning a cipher text

```sh
dotnet bin\Release\net6.0\StructuredStrings.dll  -e "123-45-6789" -c credentials -n ALPHANUM_SSN
```

Demonstrate decrypting a social security number and returning the plain text

```sh
dotnet bin\Release\net6.0\StructuredStrings.dll  -d "W#]-iV-`,\"j" -c credentials -n ALPHANUM_SSN
```
