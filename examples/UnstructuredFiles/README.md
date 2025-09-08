# Ubiq Security .NET Core File Encrypt/Decrypt Sample

This sample application will demonstrate how to encrypt and decrypt entire files using 
the unstructured data API.

## Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Prerequisites

* [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

## Build

Open a command shell and change to the ```UnstructuredFiles``` folder:

```sh
cd UnstructuredFiles
dotnet build -c Release
```

## View Program Options

Open a command shell and change to the ```UnstructuredFiles``` folder:

```sh
cd UnstructuredFiles
dotnet run -- --help
```

<pre>
  -e, --encrypt      (Default: false) Encrypt the contents of the input file and write the results to output file
  -d, --decrypt      (Default: false) Decrypt the contents of the input file and write the results to output file
  -s, --simple       (Default: false) Use the simple encryption / decryption interfaces
  -p, --piecewise    (Default: false) Use the piecewise encryption / decryption interfaces
  -i, --in           Required. Set input file name
  -o, --out          Required. Set output file name
  -c, --creds        Set the file name with the API credentials
  -P, --profile      (Default: default) Identify the profile within the credentials file
  --help             Display this help screen.
  --version          Display version information.
</pre>

## Example Usage

Demonstrate using the simple (-s / --simple) API interface to encrypt this README.md file and write 
the encrypted data to README.enc

```sh
dotnet run -c Release -- -i README.md -o README.enc -e -s -c credentials
```

Demonstrate using the simple (-s / --simple) API interface to decrypt the README.enc file and write
the decrypted output to README.out

```sh
dotnet run -c Release -- -i README.enc -o README.out -d -s -c credentials
```

Demonstrate using the piecewise (-p / --piecewise) API interface to encrypt this README.md file and write
the encrypted data to README.enc

```sh
dotnet run -c Release -- -i README.md -o README.enc -e -p -c credentials
```

Demonstrate using the piecewise (-p / --piecewise) API interface to decrypt the README.enc file and write 
the decrypted output to README.out

```sh
dotnet run -c Release -- -i README.enc -o README.out -d -p -c credentials
```
