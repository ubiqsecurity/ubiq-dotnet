# Ubiq Security Sample Application using .NET Library


This sample application will demonstrate how to encrypt and decrypt data using 
the different APIs.


### Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Build From Source

Download the reference source from the Gitlab repository, open the ```ubiq-dotnet.sln``` in Visual Studio,
select ```WinConsole``` or ```CoreConsole``` project as the Startup Project, then do a full Rebuild.

## Credentials file

Edit the credentials file with your account credentials created using the Ubiq dashboard

```sh
[default]
ACCESS_KEY_ID = ...  
SECRET_SIGNING_KEY = ...  
SECRET_CRYPTO_ACCESS_KEY = ...  
```



## View Program Options

Open a Windows command shell and change to either the ```WinConsole``` or ```CoreConsole``` executable folder:

```
cd CoreConsole
dotnet bin\Debug\netcoreapp2.0\CoreConsole.dll --help
```

```
cd WinConsole
bin\Debug\WinConsole.exe --help
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

**Note**: specify an empty string ("") as the credentials option to load your user-specific credentials file, located at ```c:\Users\_YourLogin_\.ubiq\credentials```.

#### Demonstrate using the simple (-s / --simple) API interface to encrypt this README.md file and write the encrypted data to README.md.cipher

```sh
cd CoreConsole
dotnet bin\Debug\netcoreapp2.0\CoreConsole.dll -i README.md -o README.md.cipher -e -s -c "" 
cd ..\WinConsole
bin\Debug\WinConsole.exe -i ..\CoreConsole\README.md -o README.md.cipher -e -s -c "" 
```

#### Demonstrate using the simple (-s / --simple) API interface to decrypt the README.md.cipher file and write the decrypted output to README-rehydrated.md

```sh
cd CoreConsole
dotnet bin\Debug\netcoreapp2.0\CoreConsole.dll -i README.md.cipher -o README-rehydrated.md -d -s -c "" 
cd WinConsole
bin\Debug\WinConsole.exe -i README.md.cipher -o README-rehydrated.md -d -s -c "" 
```

#### Demonstrate using the piecewise (-ps / --piecewise) API interface to encrypt this README.md file and write the encrypted data to README.md.cipher

```sh
cd CoreConsole
dotnet bin\Debug\netcoreapp2.0\CoreConsole.dll -i README.md -o README.md.cipher -e -p -c "" 
cd ..\WinConsole
bin\Debug\WinConsole.exe -i ..\CoreConsole\README.md -o README.md.cipher -e -p -c "" 
```

#### Demonstrate using the piecewise (-p / --piecewise) API interface to decrypt the README.md.cipher file and write the decrypted output to README-rehydrated.md

```sh
cd CoreConsole
dotnet bin\Debug\netcoreapp2.0\CoreConsole.dll -i README.md.cipher -o README-rehydrated.md -d -p -c "" 
cd WinConsole
bin\Debug\WinConsole.exe -i README.md.cipher -o README-rehydrated.md -d -p -c "" 
```

