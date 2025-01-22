# Ubiq Security .NET Library

The Ubiq Security dotnet (.NET) library provides convenient interaction with the
Ubiq Security Platform API from applications written in the C# language for .NET.
It includes a pre-defined set of classes that will provide simple interfaces
to encrypt and decrypt data.

## Documentation

See the [.NET API docs][ubiq-dotnet-docs].

## Requirements

- .NET Framework (4.6.2 or newer) desktop development
- .NET Core (6.0 or newer) cross-platform development

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

### Unstructured encryption of a simple block of data

Pass credentials and plaintext bytes into the unstructured encryption function.  The encrypted data
bytes will be returned.
**Note:** This is a non-blocking function, so be sure to use the appropriate process controls to make sure the results are available when desired.  See the the following [Microsoft documentation][dotnet-async] for additional information.

```cs
using UbiqSecurity;

UbiqCredentials credentials = ...;
byte[] plainBytes = ...;
byte[] encryptedBytes = await UbiqEncrypt.EncryptAsync(credentials, plainBytes);
```

### Unstructured decryption of a simple block of data

Pass credentials and encrypted data into the unstructured decryption function.  The plaintext data
bytes will be returned.
**Note:** This is a non-blocking function, so be sure to use the appropriate process controls to make sure the results are available when desired.  See the the following [Microsoft documentation][dotnet-async] for additional information.

```cs
using UbiqSecurity;

UbiqCredentials credentials = ...;
byte[] encryptedBytes = ...;
byte[] plainBytes = await UbiqDecrypt.DecryptAsync(credentials, encryptedBytes);
```

### Unstructured encryption of a large data element where data is loaded in chunks

- Create an unstructured encryption object using the credentials.
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

### Encrypt several objects using the same data encryption key (fewer calls to the server)

In this example, the same data encryption key is used to encrypt several different plain text objects,
object1 .. objectn.  In each case, a different initialization vector, IV, is automatically used but the ubiq
platform is not called to obtain a new data encryption key, resulting in better throughput.  For data security
reasons, you should limit n to be less than 2^32 (4,294,967,296) for each unique data encryption key.

1. Create an encryption object using the credentials.
2. Repeat following three steps as many times as appropriate

    - Call the encryption instance begin method
    - Call the encryption instance update method repeatedly until a single object's data is processed
    - Call the encryption instance end method

3. Call the encryption instance close method

```cs
      UbiqCredentials ubiqCredentials = UbiqFactory.readCredentialsFromFile("path/to/file", "default");

      ... 
      UbiqEncrypt ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1);

      List<Byte> cipherBytes = new ArrayList<Byte>();
      // object1 is a full unencrypted object
      byte[] tmp = ubiqEncrypt.begin();
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.update(object1, 0, object1.length);
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.end();
      cipherBytes.addAll(Bytes.asList(tmp))
      // Do something with the encrypted data: cipherBytes

      // In this case, object2 is broken into two pieces, object2_part1 and object2_part2
      cipherBytes = new ArrayList<Byte>();
      tmp = ubiqEncrypt.begin();
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.update(object2_part1, 0, object2_part1.length);
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.update(object2_part2, 0, object2_part2.length);
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.end();
      cipherBytes.addAll(Bytes.asList(tmp))
      // Do something with the encrypted data: cipherBytes

      ...
      // In this case, objectb is broken into two pieces, object2_part1 and object2_part2
      cipherBytes = new ArrayList<Byte>();
      // objectn is a full unencrypted object
      tmp = ubiqEncrypt.begin();
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.update(objectn, 0, objectn.length);
      cipherBytes.addAll(Bytes.asList(tmp))
      tmp = ubiqEncrypt.end();
      cipherBytes.addAll(Bytes.asList(tmp))
      // Do something with the encrypted data: cipherBytes

      ubiqEncrypt.close()
}
```

### Unstructured decryption of a large data element where data is loaded in chunks

- Create a unstructured decryption object using the credentials.
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

## Ubiq Structured Encryption

### Reading and setting credentials

The structured encryption functions work with the credentials file and/or environmental variables in the same way as described
earlier in this document. You'll only need to make sure that the API keys you pull from the Ubiq dashboard are are associated with a structured dataset.

### Encrypt a social security text field

Create an Encryption / Decryption object with the credentials and then allow repeatedly call encrypt
data using a structured dataset and the data.  The encrypted data will be returned after each call.

Note that you would only need to create the "ubiqEncryptDecrypt" object once for any number of
EncryptAsync and DecryptAsync calls, for example when you are bulk processing many such
encrypt / decrypt operations in a session.

```cs
async Task EncryptionAsync(String FfsName, String plainText, IUbiqCredentials ubiqCredentials)
{
    // default tweak in case the FFS model allows for external tweak insertion          
    byte[] tweakFF1 = {};

    using (var ubiqEncryptDecrypt = new UbiqStructuredEncryptDecrypt(ubiqCredentials))
    {
        var cipherText = await ubiqEncryptDecrypt.EncryptAsync(FfsName, plainText, tweakFF1);
        Console.WriteLine($"ENCRYPTED cipherText= {cipherText}\n");
    }

    return;
}
```

### Decrypt a social security text field

Create an Encryption / Decryption object with the credentials and then repeatedly decrypt
data using a structured dataset and the data. The decrypted data will be returned after each call.

Note that you would only need to create the "ubiqEncryptDecrypt" object once for any number of
EncryptAsync and DecryptAsync calls, for example when you are bulk processing many such
encrypt / decrypt operations in a session.

```cs
async Task DecryptionAsync(String FfsName, String cipherText, IUbiqCredentials ubiqCredentials)
{

    byte[] tweakFF1 = {};

    using (var ubiqEncryptDecrypt = new UbiqStructuredEncryptDecrypt(ubiqCredentials))
    {
        var plainText = await ubiqEncryptDecrypt.DecryptAsync(FfsName, cipherText, tweakFF1);
        Console.WriteLine($"DECRYPTED plainText= {plainText}\n");
    }

    return;
}
```

### Custom Metadata for Usage Reporting

There are cases where a developer would like to attach metadata to usage information reported by the application. Both the structured and unstructured interfaces allow user_defined metadata to be sent with the usage information reported by the libraries.

The **AddReportingUserDefinedMetadata** function accepts a string in JSON format that will be stored in the database with the usage records. The string must be less than 1024 characters and be a valid JSON format.  The string must include both the **{** and **}** symbols.  The supplied value will be used until the object goes out of scope.  Due to asynchronous processing, changing the value may be immediately reflected in subsequent usage.  If immediate changes to the values are required, it would be safer to create a new encrypt / decrypt object and call the **AddReportingUserDefinedMetadata** function with the new values.

Examples are shown below.

```cs
    using var ubiq = new UbiqStructuredEncryptDecrypt(ubiqCredentials);
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
using var ubiq = new UbiqStructuredEncryptDecrypt(ubiqCredentials);

var encryptedSsns = await ubiq.EncryptForSearchAsync("SSN_Dataset", unencryptedSsn)

var user = _dbContext
                .Employees
                .Where(x => encryptedSsns.Contains(x.EncryptedSSN))
                .FirstOrDefault();
```

## UbiqConfiguration object

### Event Reporting

The **EventReporting** section contains values to control how often the usage is reported.  

- **WakeInterval** indicates the number of seconds to sleep before waking to determine if there has been enough activity to report usage
- **MinimumCount** indicates the minimum number of usage records that must be queued up before sending the usage
- **FlushInterval** indicates the sleep interval before all usage will be flushed to server.
- **TrapExceptions** indicates whether exceptions encountered while reporting usage will be trapped and ignored or if it will become an error that gets reported to the application
- **TimestampGranularity** indicates the how granular the timestamp will be when reporting events.  Valid values are
  - "NANOS"  
    // DEFAULT: values are reported down to the nanosecond resolution when possible
  - "MILLIS"  
  // values are reported to the millisecond
  - "SECONDS"  
  // values are reported to the second
  - "MINUTES"  
  // values are reported to minute
  - "HOURS"  
  // values are reported to hour
  - "HALF_DAYS"  
  // values are reported to half day
  - "DAYS"  
  // values are reported to the day

### Key Caching

The **KeyCaching** section contains values to control how and when keys are cached.

- **TtlSeconds** indicates how many seconds a cache element should remain before it must be re-retrieved. (default: 1800)
- **Structured** indicates whether keys will be cached when doing structured encryption and decryption. (default: true)
- **Unstructured** indicates whether keys will be cached when doing unstructured decryption. (default: true)
- **Encrypt** indicates if keys should be stored encrypted. If keys are encrypted, they will be harder to access via memory, but require them to be decrypted with each use. (default: false)

## Ubiq API Error Reference

Occasionally, you may encounter issues when interacting with the Ubiq API.

| Status Code | Meaning | Solution |
|---|---|---|
| 400 | Bad Request | Check name of datasets and credentials are complete. |
| 401 | Authentication issue | Check you have the correct API keys, and it has access to the datasets you are using.  Check dataset name. |
| 426 | Upgrade Required | You are using an out of date version of the library, or are trying to use newer features not supported by the library you are using.  Update the library and try again. |
| 429 | Rate Limited | You are performing operations too quickly. Either slow down, or contact <support@ubiqsecurity.com> to increase your limits. |
| 500 | Internal Server Error | Something went wrong. Contact support if this persists. |
| 504 | Internal Error | Possible API key issue.  Check credentials or contact support. |

[ubiq-dotnet-docs]:https://dev.ubiqsecurity.com/docs/dotnet-library
[ubiq-dotnet-samples]:https://gitlab.com/ubiqsecurity/ubiq-dotnet-sample
[dashboard]:https://dashboard.ubiqsecurity.com/
[nuget-cli]: https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference
[dotnet-core-cli-tools]: https://docs.microsoft.com/en-us/dotnet/core/tools/
[package-manager-console]: https://docs.microsoft.com/en-us/nuget/tools/package-manager-console
[dotnet-async]:https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
[sync-context-info]:https://docs.microsoft.com/en-us/archive/msdn-magazine/2011/february/msdn-magazine-parallel-computing-it-s-all-about-the-synchronizationcontext
[manage-keys]:https://dev.ubiqsecurity.com/docs/api-keys
[use-credentials]:https://dev.ubiqsecurity.com/docs/using-api-key-credentials
