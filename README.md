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

#### Compiling using Visual Studio Environment
-   Visual Studio 2017 or newer
-   In the Visual Studio Installer, make sure the following items are checked in the *Workloads* category:
    - .NET desktop development
    - .NET Core cross-platform development
-   If building the ubiq-dotnet library from source, the ubiq-dotnet solution assumes the .NET Framework 4.6.1 for Windows, .NET Core 2.0 or later, and .NET Standard 2.0 or later.

Within the Solution Explorer pane, right-click the UbiqSecurity project, then select *Set as Startup Project*.

From the Build menu, execute *Rebuild Solution* to compile all projects.

#### Requirements to Use Ubiq-Security library
-   Visual Studio 2017 or newer with one of the following development environments
    - .NET Framework (4.6.1 or newer) desktop development
    - .NET Core (2.0 or newer) cross-platform development

## Usage

The library needs to be configured with your [account credentials][manage-keys] which is
available in your [Ubiq Dashboard][dashboard].
The [credentials][use-credentials] can be set using environment variables, loaded from an explicitly
specified file, or loaded from a file in your Windows
user account directory [c:/users/_yourlogin_/.ubiq/credentials].

### Sample applications

See the reference source and Readme in *WinConsole* and *CoreConsole*  in the ```ubiq-dotnet.sln```

-   *WinConsole* - for Windows .NET Framework 4.6.1 or later
    - [README.md][WinConsoleReadme]
-   *CoreConsole* - for portable .NET Core runtime.
    - [README.md][CoreConsoleReadme]
-   *CoreConsoleFpe* - for portable .NET Core runtime using FPE.
    - [README.md][CoreConsoleFpeReadme]

Both test apps reference the *same* portable UbiqSecurity DLL library, build against .NET Standard 2.0.


### Referencing the Ubiq Security library
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

### Runtime "hangs"

Some users have experienced "hangs" during encryption and decryption operations. So far, this
has been solved by adding `.ConfigureAwait(false)` to those calls as in:

```cs
await UbiqEncrypt.EncryptAsync(credentials, plainBytes).ConfigureAwait(false);
```

More information can be found about C# `SynchronizationContext` can be found
[here][sync-context-info].

### Encrypt a simple block of data

Pass credentials and plaintext bytes into the encryption function.  The encrypted data
bytes will be returned.
**Note:** This is a non-blocking function, so be sure to use the appropriate process controls to make sure the results are available when desired.  See the the following [Microsoft documentation][dotnet-async] for additional information.

```cs
using UbiqSecurity;

byte[] plainBytes = ...;
byte[] encryptedBytes = await UbiqEncrypt.EncryptAsync(credentials, plainBytes);
```

### Decrypt a simple block of data

Pass credentials and encrypted data into the decryption function.  The plaintext data
bytes will be returned.
**Note:** This is a non-blocking function, so be sure to use the appropriate process controls to make sure the results are available when desired.  See the the following [Microsoft documentation][dotnet-async] for additional information.

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

Below is the working code from the test application in the reference source:

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

Below is the working code from the test application in the reference source:

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

## FPE/eFPE (Optionally Available Feature)

This library incorporates format preserving encryption (FPE) and embedded format preserving encryption (eFPE). Please contact support@ubiqsecurity.com to add this capability to your account.

## Usage
FPE is an optional capability and may require an upgrade to your plan.  If you do not see the FPE option when [registering an application][register-app], contact support@ubiqsecurity.com to find out about adding FPE to your plan.  

The library needs to be configured with your [account credentials][manage-keys] which is
available in your [Ubiq Dashboard][dashboard].
The [credentials][use-credentials] can be set using environment variables, loaded from an explicitly
specified file, or loaded from a file in your Windows
user account directory [c:/users/_yourlogin_/.ubiq/credentials].

### Referencing the Ubiq Security library
Make sure your project has a reference to the UbiqSecurity DLL library, either by adding the NuGet package
(if using prebuilt library) or by adding a project reference (if built from source).
Then, add the following to the top of your C# source file:

```cs
using UbiqSecurity;
```

### Reading and setting credentials

The FPE/eFPE functions work with the credentials file and/or environmental variables in the same way as described
earlier in this document. You'll only need to make sure that the API keys you pull from the Ubiq dashboard are enabled for
FPE/eFPE capability.

### Encrypt a social security text field

Lets assume you have a field containing a social security number "123-45-6789". You are able to encrypt the contents of that field by adding these lines to your program and calling the function with the appropriate values.
```cs
async Task EncryptionAsync(String FfsName, String plainText, IUbiqCredentials ubiqCredentials)
{
	// default tweak in case the FFS model allows for external tweak insertion          
	byte[] tweakFF1 = {};

	using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
	{
		var cipherText = await ubiqEncryptDecrypt.EncryptAsync(FfsName, plainText, tweakFF1);
		Console.WriteLine($"ENCRYPTED cipherText= {cipherText}\n");
	}

	return;
}
```

### Decrypt the encrypted social security cipher text

To decrypt the cipher text (e.g. "W$+-qF-oMMV") of a social security number, your function could look like the following and called with the appropriate values.

```cs
async Task DecryptionAsync(String FfsName, String cipherText, IUbiqCredentials ubiqCredentials)
{

	byte[] tweakFF1 = {};

	using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
	{
		var plainText = await ubiqEncryptDecrypt.DecryptAsync(FfsName, cipherText, tweakFF1);
		Console.WriteLine($"DECRYPTED plainText= {plainText}\n");
	}

	return;
}
```

### Other FFS models to explore

Additional information on how to use these FFS models in your own applications is available by contacting
Ubiq. You may also view the sample application [CoreConsoleFpe/Program.cs][fpesample] source code.


[dashboard]:https://dashboard.ubiqsecurity.com/
[nuget-cli]: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference
[dotnet-core-cli-tools]: https://docs.microsoft.com/en-us/dotnet/core/tools/
[package-manager-console]: https://docs.microsoft.com/en-us/nuget/tools/package-manager-console
[WinConsoleReadme]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/WinConsole/README.md
[CoreConsoleReadme]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/CoreConsole/README.md
[CoreConsoleFpeReadme]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/CoreConsoleFpe/README.md
[dotnet-async]:https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
[sync-context-info]:https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
[fpesample]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/CoreConsoleFpe/Program.cs
[manage-keys]:https://dev.ubiqsecurity.com/docs/how-to-manage-api-keys
[use-credentials]:https://dev.ubiqsecurity.com/docs/using-api-key-credentials
[register-app]:https://dev.ubiqsecurity.com/docs/how-to-register-applications
