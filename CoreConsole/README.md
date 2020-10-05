# Ubiq Security .NET Core Cross-Platform Sample Application

This sample application will demonstrate how to encrypt and decrypt data using 
the different APIs.


### Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Build From Source

Download the reference source from the Gitlab repository, open the ```ubiq-dotnet.sln``` in Visual Studio,
select ```CoreConsole``` project as the Startup Project, select the ```Release``` Solution Configuration, ```Any CPU``` Solution Configuration, and then do a full Rebuild.

## Credentials file

Edit the credentials file with your account credentials created using the Ubiq dashboard

```sh
[default]
ACCESS_KEY_ID = ...
SECRET_SIGNING_KEY = ...
SECRET_CRYPTO_ACCESS_KEY = ...
```

## View Program Options

Open a Windows command shell and change to the ```CoreConsole``` executable folder:

```sh
cd CoreConsole
dotnet bin\Release\netcoreapp2.0\CoreConsole.dll --help
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

#### Demonstrate using the simple (-s / --simple) API interface to encrypt this README.md file and write the encrypted data to README.enc

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsole.dll -i README.md -o README.enc -e -s -c credentials
```

#### Demonstrate using the simple (-s / --simple) API interface to decrypt the README.enc file and write the decrypted output to README.out

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsole.dll -i README.enc -o README.out -d -s -c credentials
```

#### Demonstrate using the piecewise (-ps / --piecewise) API interface to encrypt this README.md file and write the encrypted data to README.enc

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsole.dll -i README.md -o README.enc -e -p -c credentials
```

#### Demonstrate using the piecewise (-p / --piecewise) API interface to decrypt the README.enc file and write the decrypted output to README.out

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsole.dll -i README.enc -o README.out -d -p -c credentials
```

