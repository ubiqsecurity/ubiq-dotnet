# MSSQL

### IDP Integration Setup

1. Login to https://dashboard.ubiqsecurity.com/
1. Go to your Account Profile @ https://dashboard.ubiqsecurity.com/#/profile
1. Enable IDP Integration (note: requires Enterprise plan)
    1. In the IDP Provider dropdown, select Self-Signed
    1. Click Generate Key Pair button to generate a new private/public key pair
    1. Click Copy to Clipboard when prompted and save the private key somewhere safe for later use
    1. Copy the Ubiq SCIM URL value somewhere safe for later use
    1. Click Save
1. Create an API Key for each SQL Login that will be using Ubiq (names must match)


### MS SQL 2014 Installation

Pre-requisites: dotnet framework 4.8

1. Download ubiq-mssql.zip from latest Ubiq-Security.MSSQL release at https://gitlab.com/ubiqsecurity/ubiq-dotnet/-/packages
1. Copy to and unzip on MSSQL server (e.g. into c:\Ubiq for this example)
1. Copy the following dlls from the dotnet framework folder (e.g. C:\Windows\Microsoft.NET\Framework64\v4.0.30319) into c:\Ubiq
    1. SMDiagnostics.dll
    1. System.Net.Http.dll
    1. System.Runtime.Caching.dll
    1. System.Runtime.Serialization.dll
    1. System.ServiceModel.Internals.dll
1. In c:\Ubiq, create a file named 'config.json' with the following contents

```json
{
  "idp": {
    "provider": "ubiq",
    "ubiq_customer_id": ", // Set to the GUID part of the Ubiq SCIM URL that you copied during the IDP Integration Setup above
    "self_sign_key": "" // Set to the value of the Private Key from the IDP Integration Setup above
  }
}
```

1. Run the following SQL statements, changing the database name and Ubiq file paths accordingly

```sql
    EXEC sp_configure 'show advanced options', 1;
    EXEC sp_configure 'clr enabled', 1;

    ALTER DATABASE your_database_name SET TRUSTWORTHY ON;
    
    CREATE ASSEMBLY ubiq FROM 'C:\Ubiq\UbiqSecurity.Mssql.dll' WITH PERMISSION_SET = UNSAFE;
 
    CREATE FUNCTION encrypt
    (
        @datasetName nvarchar(100),
	    @plainText nvarchar(100)
    )
    RETURNS nvarchar(100)
    AS
    EXTERNAL NAME ubiq.[UbiqSecurity.Mssql.UbiqSql].Encrypt;

    CREATE FUNCTION decrypt
    (
        @datasetName nvarchar(100),
        @cipherText nvarchar(100)
    )
    RETURNS nvarchar(100)
    AS
    EXTERNAL NAME ubiq.[UbiqSecurity.Mssql.UbiqSql].Decrypt;
```

### Example Usage

```sql
    select dbo.encrypt('SSN', '123-12-1434');

    select dbo.decrypt('SSN', '100-0N-sxfz');
```