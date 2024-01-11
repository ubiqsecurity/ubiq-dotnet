# Ubiq Security .NET Library

The Ubiq Security dotnet (.NET) library provides convenient interaction with the
Ubiq Security Platform API from applications written in the C# language for .NET.
It includes a pre-defined set of classes that will provide simple interfaces
to encrypt and decrypt data.

This library also incorporates Ubiq Format Preserving Encryption (eFPE).  eFPE allows
encrypting so that the output cipher text is in the same format as the original plaintext.
This includes preserving special characters and control over what characters are permitted
in the cipher text. For example, consider encrypting a social security number '123-45-6789'.
The cipher text will maintain the dashes and look something like: 'W$+-qF-oMMV'.

## Documentation

See the [.NET API docs][ubiq-dotnet-docs].

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

### Requirements to Use Ubiq-Security library

- .NET Framework (4.6.2 or newer) desktop development
- .NET Core (6.0 or newer) cross-platform development

## Building from source

From within the cloned local git repository folder, use Visual Studio to open the solution file:

```text
ubiq-dotnet.sln
```

### Compiling from command line

```cmd
    dotnet build -c Release
```

### Compiling using Visual Studio Environment

- Visual Studio 2022 or newer
- In the Visual Studio Installer, make sure the following items are checked in the *Workloads* category:
  - .NET desktop development
  - .NET Core cross-platform development

Within the Solution Explorer pane, right-click the UbiqSecurity project, then select *Set as Startup Project*.

From the Build menu, execute *Rebuild Solution* to compile all projects.

## Usage

The library needs to be configured with your [account credentials][manage-keys] which is
available in your [Ubiq Dashboard][dashboard].
The [credentials][use-credentials] can be set using environment variables, loaded from an explicitly
specified file, or loaded from a file in your Windows
user account directory [c:/users/*yourlogin*/.ubiq/credentials].

### Sample applications

See the reference [sample applications][ubiq-dotnet-samples].

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

### Read credentials from c:/users/*yourlogin*/.ubiq/credentials and use the default profile

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

## Ubiq Format Preserving Encryption

This library incorporates Ubiq Format Preserving Encryption (eFPE).

## Requirements

- Please follow the same requirements as described above for the non-eFPE functionality.

## Usage

You will need to obtain account credentials in the same way as described above for conventional encryption/decryption. When
you do this in your [Ubiq Dashboard][dashboard] [credentials][credentials], you'll need to enable the eFPE option.
The credentials can be set using environment variables, loaded from an explicitly
specified file, or read from the default location (c:/users/*yourlogin*/.ubiq/credentials).

### Referencing the Ubiq Security library

Make sure your project has a reference to the UbiqSecurity DLL library, either by adding the NuGet package
(if using prebuilt library) or by adding a project reference (if built from source).
Then, add the following to the top of your C# source file:

```cs
using UbiqSecurity;
```

### Reading and setting credentials

The eFPE functions work with the credentials file and/or environmental variables in the same way as described
earlier in this document. You'll only need to make sure that the API keys you pull from the Ubiq dashboard are enabled for
eFPE capability.

### Encrypt a social security text field - simple interface

Pass credentials, the name of a Field Format Specification, FFS, and data into the encryption function.
The encrypted data will be returned.

```cs
{
  byte[] tweakFF1 = {};
  var ffsName = "SSN";
  var plainText = "123-45-6789";

  var ubiqCredentials = UbiqFactory.ReadCredentialsFromFile("path/to/credentials/file", "default");

  var cipherText = await UbiqFPEEncryptDecrypt.EncryptAsync(ubiqCredentials, plainText, ffsName, tweakFF1);
}
```

### Decrypt a social security text field - simple interface

Pass credentials, the name of a Field Format Specification, FFS, and data into the decryption function.
The plain text data will be returned.

```cs
{
  byte[] tweakFF1 = {};
  var ffsName = "SSN";
  var cipherText = "7\"c-`P-fGj?";

  var ubiqCredentials = UbiqFactory.ReadCredentialsFromFile("path/to/credentials/file", "default");

  var plainText = await UbiqFPEEncryptDecrypt.DecryptAsync(ubiqCredentials, cipherText, ffsName, tweakFF1);
}
```

### Encrypt a social security text field - bulk interface

Create an Encryption / Decryption object with the credentials and then allow repeatedly call encrypt
data using a Field Format Specification, FFS, and the data.  The encrypted data will be returned after each call

Note that you would only need to create the "ubiqEncryptDecrypt" object once for any number of
EncryptAsync and DecryptAsync calls, for example when you are bulk processing many such
encrypt / decrypt operations in a session.

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

### Decrypt a social security text field - bulk interface

Create an Encryption / Decryption object with the credentials and then repeatedly decrypt
data using a Field Format Specification, FFS, and the data.  The decrypted data will be returned after each call.

Note that you would only need to create the "ubiqEncryptDecrypt" object once for any number of
EncryptAsync and DecryptAsync calls, for example when you are bulk processing many such
encrypt / decrypt operations in a session.

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

### Custom Metadata for Usage Reporting

There are cases where a developer would like to attach metadata to usage information reported by the application. Both the structured and unstructured interfaces 
allow user_defined metadata to be sent with the usage information reported by the libraries.

The <b>AddReportingUserDefinedMetadata</b> function accepts a string in JSON format that will be stored in the database with the usage records. The string 
must be less than 1024 characters and be a valid JSON format.  The string must include both the <b>{</b> and <b>}</b> symbols.  The supplied value will be used 
until the object goes out of scope.  Due to asynchronous processing, changing the value may be immediately reflected in subsequent usage.  If immediate changes to 
the values are required, it would be safer to create a new encrypt / decrypt object and call the <b>AddReportingUserDefinedMetadata</b> function with the new values.

Examples are shown below.

```cs
    using var ubiq = new UbiqFPEEncryptDecrypt(ubiqCredentials);
    ubiqEncryptDecrypt.AddReportingUserDefinedMetadata("{\"some_meaningful_flag\" : true }");
}
```

```cs
    using var ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1);
    ubiqEncrypt.AddReportingUserDefinedMetadata("{\"some_key\" : \"some_value\" }");
```

### Searching for a value in a database that is encrypted

For example say we want to search for an employee by SSN, but that field was encrypted in the database. The encryption key may have rotated since the employee SSN was originally encrypted, so we can use the EncryptForSearchAsync() method to get an array of all possible encrypted values.

```cs
using var ubiq = new UbiqFPEEncryptDecrypt(ubiqCredentials);

var encryptedSsns = await ubiq.EncryptForSearchAsync("SSN_Dataset", unencryptedSsn)

var user = _dbContext
                .Employees
                .Where(x => encryptedSsns.Contains(x.EncryptedSSN))
                .FirstOrDefault();
```

### More Information

Additional information on how to use these FFS models in your own applications is available by contacting
Ubiq. You may also view some use-cases implemented in the unit test [UbiqFpeEncryptDecryptTests.cs]
and the sample application [source code][ubiq-dotnet-samples].

[ubiq-dotnet-docs]:https://dev.ubiqsecurity.com/docs/dotnet-library
[ubiq-dotnet-samples]:https://gitlab.com/ubiqsecurity/ubiq-dotnet-sample
[dashboard]:https://dashboard.ubiqsecurity.com/
[nuget-cli]: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference
[dotnet-core-cli-tools]: https://docs.microsoft.com/en-us/dotnet/core/tools/
[package-manager-console]: https://docs.microsoft.com/en-us/nuget/tools/package-manager-console
[dotnet-async]:https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
[sync-context-info]:https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
[efpesample]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/CoreConsoleFpe/Program.cs
[manage-keys]:https://dev.ubiqsecurity.com/docs/api-keys
[use-credentials]:https://dev.ubiqsecurity.com/docs/using-api-key-credentials
[credentials]:https://dev.ubiqsecurity.com/docs/api-key-credentials
[UbiqFpeEncryptDecryptTests.cs]:https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/blob/master/tests/UbiqSecurity.Tests/UbiqFpeEncryptDecryptTests.cs
