# File2ArduinoHeaderFile
This .NET Core application converts files to byte array variables into a C header file for the Arduino Framework. In future you can also use the gzip algorithm to make the byte array more compact.

The main purpose of this is to make files available on ESP Web Servers. Icons, CSS files or HTML files for example. 

## Why .NET Core console application?
I like .NET Core and doing it this way, this tool is available in Windows and Linux.

## How to use
### Parameters

| Param | Function |
|-------|----------|
| -d | Set the directory, in which all files except for header files (ending with .h) are converted |
| -f | Select a single file which will be converted |
| -u | Do not ignore header files |
| -g | Gzip file content before writing it into header file |

You need specify either -d or -f. Example call: 

### Example
`File2ArduinoHeaderFile.exe -d "c:\temp" -g`: This will convert all files in the directory `c:\temp` except for header files and use gzip algorithm on them before writing the header files.

The corresponding header file for a file called `bootstrap.min.css` would be named `bootstrap-min-css.h` and would contain a const byte[] definition called `bootstrapMinCss`.

### Build it
You can use the call the command `dotnet publish` to build the binaries which you can use then.

For Windows: `dotnet publish -c Release -r win10-x64`

For Ubuntu: `dotnet publish -c Release -r ubuntu.16.10-x64`

Of course you can use other runtimes as well.
