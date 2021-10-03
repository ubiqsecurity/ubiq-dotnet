# Ubiq Security .NET Core Cross-Platform Sample Application

The Ubiq Security C# library provides convenient interaction with the Ubiq Security Platform API from applications written in the C# language.  It includes a pre-defined set of classes that will provide simple interfaces to encrypt and decrypt data.

This library also incorporates format preserving encryption (FPE), available as an optional add-on to your user account. FPE allows encrypting so that the output cipher text is in the same format as the original plaintext. This includes preserving special characters and control over what characters are permitted in the cipher text. For example, consider encrypting a social security number 123-45-6789. The cipher text will maintain the dashes and look something like: W#]-iV-`,\"j.
Additionally, Ubiq supports embedded format preserving encryption (eFPE) providing the ability to store additional meta data within the cipher text.


### Documentation

See the [.NET API docs](https://dev.ubiqsecurity.com/docs/api).

## Build From Source

Download the reference source from the Gitlab repository, open the ```ubiq-dotnet.sln``` in Visual Studio,
select ```CoreConsoleFpe``` project as the Startup Project, select the ```Release``` Solution Configuration, ```Any CPU``` Solution Configuration, and then do a full Rebuild.

## Credentials file

Edit the credentials file with your account credentials created using the Ubiq dashboard

```sh
[default]
ACCESS_KEY_ID = ...
SECRET_SIGNING_KEY = ...
SECRET_CRYPTO_ACCESS_KEY = ...
```

## View Program Options

Open a Windows command shell and change to the ```CoreConsoleFpe``` executable folder:

```sh
cd CoreConsoleFpe
dotnet bin\Release\netcoreapp2.0\CoreConsoleFpe.dll --help
```

<pre>
Usage: Ubiq Security Example [options]
	Options:
		-e, --encrypttext    Set the field text value to encrypt and will return the encrypted cipher text
		-d, --decrypttext    Set the cipher text value to decrypt and will return the decrypted text
		-s, --simple         (Default: false) Use the simple encryption / decryption interfaces
		-b, --bulk           (Default: false) Use the bulk encryption / decryption interfaces
		-n, --ffsname        Set the ffs name, for example SSN
		-h, --help           (Default: false) Print app parameter summary
		-c, --creds          Set the file name with the API credentials
		-P, --profile        (Default: default) Identify the profile within the credentials file
		-V, --version        Show program's version number and exit
		--help               Display this help screen.
		--version            Display version information.
</pre>



#### Demonstrate encrypting a social security number and returning a cipher text

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsoleFpe.dll  -e 123-45-6789 -c credentials -n ALPHANUM_SSN -s
```

#### Demonstrate decrypting a social security number and returning the plain text

```sh
dotnet bin\Release\netcoreapp2.0\CoreConsoleFpe.dll  -d W#]-iV-`,\"j -c credentials -n ALPHANUM_SSN -s
```

#### Other FFS models to explore

Depending on your installation, there are a wide variety of FFS models that are available. Each FFS model
imposes its own set of rules revolving around how the data is formatted and what characters are legal for the
given format. For example, you would not expect to see alpha characters in a social security number and the model
will identify that as a formatting error. A few models to consider are:

-   ALPHANUM_SSN 
-   BIRTH_DATE 
-   GENERIC_STRING 
-   SO_ALPHANUM_PIN

Additional information on how to use these FFS models in your own applications is available by contacting
Ubiq. You may also view some use-cases implemented in the unit test source file "UbiqFPEEncryptDecryptTests.cs".

IMPORTANT, USE ONLY DOUBLE QUOTES FOR COMMAND LINE OPTIONS AND ESCAPE DOUBLE QUOTES WITH A FORWARD SLASH (\")
