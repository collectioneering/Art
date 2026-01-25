[![.NET](https://github.com/collectioneering/Art/actions/workflows/dotnet.yml/badge.svg)](https://github.com/collectioneering/Art/actions/workflows/dotnet.yml)

## Art.BrowserCookies

Art.BrowserCookies provides rudimentary support for cookie extraction on select browsers. It is a part of the [Art](https://github.com/collectioneering/Art) archival package set.

### Browser Support

| Browser / Platform | Chrome                   | Edge                      |
|--------------------|--------------------------|---------------------------|
| Windows            | v10[1], v20[1], DPAPI[1] | v10[1], v20[1], DPAPI[1]  |
| macOS              | v10[2]                   | v10[2]                    |

[1] v10, v20, and DPAPI refer to cookie encryption types;
v10 and v20 are used as prefixes with potentially browser-specific encryption procedures,
while DPAPI refers to straightforward decryption as-is via Windows's [Data Protection API](https://en.wikipedia.org/wiki/Data_Protection_API)

[2] v10 refers to a cookie encryption type;
v10 is used as a prefix, with a key retrieved from
[Keychain](https://en.wikipedia.org/wiki/Keychain_(software))

### Elevation for Decryption

v20 cookies on Windows Chromium browsers go through
[the System account](https://learn.microsoft.com/en-us/windows/security/identity-protection/access-control/local-accounts#system)
for encryption, and decryption requires a process to run under that account for the right DPAPI context.
A helper Windows PowerShell (not PowerShell Core) script is embedded in this library which includes
the [Invoke-CommandAs](https://github.com/mkellerman/Invoke-CommandAs) library and a small job which will
ultimately execute a decryption via the System account. Getting the job scheduled requires elevation to administrator, so the
PowerShell process that runs the helper script is elevated which will trigger a UAC prompt if those are enabled.

v10 cookies on macOS go through Keychain, specifically via the [`security`](https://www.unix.com/man-page/osx/1/security/)
CLI which accesses the browser's store in Keychain. This will trigger a privilege check with an option to
always allow the calling program access to that store. ***Do not*** select Always Allow, as the permissions are being granted to the `security` utility.

### Resources

[Windows v10 key source](https://chromium.googlesource.com/chromium/src/+/refs/heads/main/components/os_crypt/sync/os_crypt_win.cc)

[Windows v20 keys (runassu Python PoC)](https://github.com/runassu/chrome_v20_decryption)

[(Further explanation)](https://stackoverflow.com/a/79216440)

[Google Security Blog - Improving the security of Chrome cookies on Windows](https://security.googleblog.com/2024/07/improving-security-of-chrome-cookies-on.html)
