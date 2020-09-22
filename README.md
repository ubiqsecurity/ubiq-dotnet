# Ubiq Security .NET Library

The Ubiq Security dotnet (.NET) library provides convenient interaction with the
Ubiq Security Platform API from applications written in the C# language for .NET.
It includes a pre-defined set of classes that will provide simple interfaces
to encrypt and decrypt data.

## Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Installation

Using the [.NET Core command-line interface (CLI) tools][dotnet-core-cli-tools]:

```sh
dotnet add package ubiq-security
```

Using the [NuGet Command Line Interface (CLI)][nuget-cli]:

```sh
nuget install ubiq-security
```

Using the [Package Manager Console][package-manager-console]:

```powershell
Install-Package ubiq-security
```

#### Building from source:
From within the cloned local git repository folder, use Visual Studio to open the solution file:

```
ubiq-dotnet.sln
```

Within the Solution Explorer pane, right-click the WinConsole project, then select *Set as Startup Project*.

From the Build menu, execute *Rebuild Solution* to compile all projects.


## Requirements

-   Visual Studio 2017
-   In the Visual Studio Installer, make sure the following items are checked in the *Workloads* category:
    - .NET desktop development
    - .NET Core cross-platform development
-   If building the ubiq-dotnet library from source, the ubiq-dotnet solution assumes the .NET Framework 4.6.1 for Windows, .NET Core 2.0 or later, and .NET Standard 2.0 or later.

## Usage

The library needs to be configured with your account credentials which is
available in your [Ubiq Dashboard][dashboard] [credentials][credentials].
The credentials can be hardcoded into your application, specified with environment variables,
loaded from an explicit file, or loaded from a file in your Windows 
user account directory [c:/users/_yourlogin_/.ubiq/credentials].

### Sample applications

See the reference source includes two command-line test apps in the ```ubiq-dotnet.sln```

-   *WinConsole* - for Windows .NET Framework 4.6.1 or later.
-   *CoreConsole* - for portable .NET Core runtime.

Both test apps reference the *same* portable UbiqSecurity DLL library, build against .NET Standard 2.0.


### Referencing the Ubiq library
Make sure your project has a reference to the UbiqSecurity DLL library, either by adding the NuGet package
(if using prebuilt library) or by adding a project reference (if built from source).
Then, add the following to the top of your C# source file:

```cs
using UbiqSecurity;
```

### Read credentials from a specific file and use a specific profile 
```cs
var credentials = UbiqFactory.ReadCredentialsFromFile("some-credential-file", "some-profile");
```

### Read credentials from c:/users/_yourlogin_/.ubiq/credentials and use the default profile
```cs
var credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, null);
```

### Use the following environment variables to set the credential values
UBIQ_ACCESS_KEY_ID
UBIQ_SECRET_SIGNING_KEY
UBIQ_SECRET_CRYPTO_ACCESS_KEY
```cs
var credentials = UbiqFactory.CreateCredentials()
```

### Explicitly set the credentials
```cs
var credentials = UbiqFactory.CreateCredentials(accessKeyId: "...", secretSigningKey: "...", secretCryptoAccessKey: "...");
```

### Runtime exceptions

Unsuccessful requests raise exceptions. The exception object will contain the error details. 

### Encrypt a simple block of data

Pass credentials and plaintext bytes into the encryption function.  The encrypted data
bytes will be returned.

```cs
using UbiqSecurity;

byte[] plainBytes = ...;
byte[] encryptedBytes = await UbiqEncrypt.EncryptAsync(credentials, plainBytes);
```

### Decrypt a simple block of data

Pass credentials and encrypted data into the decryption function.  The plaintext data
bytes will be returned.

```cs
using UbiqSecurity;

byte[] encryptedBytes = ...;
byte[] plainBytes = await UbiqDecrypt.DecryptAsync(credentials, encryptedBytes);
```

### Encrypt a large data element where data is loaded in chunks

- Create an encryption object using the credentials.
- Call the encryption instance ```BeginAsync()``` method.
- Call the encryption instance ```Update()``` method repeatedly until all the data is processed.
- Call the encryption instance ```End()``` method.

 Here's the working code from the test application in the reference source:

 ```cs
async Task PiecewiseEncryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
{
    using (var plainStream = new FileStream(inFile, FileMode.Open))
    {
        using (var cipherStream = new FileStream(outFile, FileMode.Create))
        {
            using (var ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1))
            {
                // start the encryption
                var cipherBytes = await ubiqEncrypt.BeginAsync();
                cipherStream.Write(cipherBytes, 0, cipherBytes.Length);

                // process 128KB at a time
                var plainBytes = new byte[0x20000];

                // loop until the end of the input file is reached
                int bytesRead = 0;
                while ((bytesRead = plainStream.Read(plainBytes, 0, plainBytes.Length)) > 0)
                {
                    cipherBytes = ubiqEncrypt.Update(plainBytes, 0, bytesRead);
                    cipherStream.Write(cipherBytes, 0, cipherBytes.Length);
                }

                // finish the encryption
                cipherBytes = ubiqEncrypt.End();
                cipherStream.Write(cipherBytes, 0, cipherBytes.Length);
            }
        }
    }
}
```

### Decrypt a large data element where data is loaded in chunks

- Create a decryption object using the credentials.
- Call the decryption instance ```Begin()``` method.
- Call the decryption instance ```UpdateAsync()``` method repeatedly until all data is processed.
- Call the decryption instance ```End()``` method

Here's the working code from the test application in the reference source:

 ```cs
async Task PiecewiseDecryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
{
    using (var cipherStream = new FileStream(inFile, FileMode.Open))
    {
        using (var plainStream = new FileStream(outFile, FileMode.Create))
        {
            using (var ubiqDecrypt = new UbiqDecrypt(ubiqCredentials))
            {
                // start the decryption
                var plainBytes = ubiqDecrypt.Begin();
                plainStream.Write(plainBytes, 0, plainBytes.Length);

                // process 128KB at a time
                var cipherBytes = new byte[0x20000];

                // loop until the end of the input file is reached
                int bytesRead = 0;
                while ((bytesRead = cipherStream.Read(cipherBytes, 0, cipherBytes.Length)) > 0)
                {
                    plainBytes = await ubiqDecrypt.UpdateAsync(cipherBytes, 0, bytesRead);
                    plainStream.Write(plainBytes, 0, plainBytes.Length);
                }

                // finish the decryption
                plainBytes = ubiqDecrypt.End();
                plainStream.Write(plainBytes, 0, plainBytes.Length);
            }
        }
    }
}
```

[dashboard]:https://dashboard.ubiqsecurity.com/
[credentials]:https://dev.ubiqsecurity.com/docs/how-to-create-api-keys
[nuget-cli]: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference
[dotnet-core-cli-tools]: https://docs.microsoft.com/en-us/dotnet/core/tools/
[package-manager-console]: https://docs.microsoft.com/en-us/nuget/tools/package-manager-console
