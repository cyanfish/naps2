# NAPS2 (Not Another PDF Scanner 2)

NAPS2 is a document scanning application with a focus on simplicity and ease of use. Scan your documents from WIA- and TWAIN-compatible scanners, organize the pages as you like, and save them as PDF, TIFF, JPEG, PNG, and other file formats. Requires .NET Framework 4.7.2 or higher. If developing, please use VS 2017, the [4.7.2 Developer Pack](https://www.microsoft.com/net/download/thank-you/net472-developer-pack), and [Rosylynator](https://github.com/JosefPihrt/Roslynator) to compile & catch possible errors.

Visit the NAPS2 home page at [www.naps2.com](http://www.naps2.com).

Other links:
- [Documentation](http://www.naps2.com/support.html)
- [Translations](http://translate.naps2.com/) - [Doc](http://www.naps2.com/doc-translations.html)
- [File a Ticket](https://sourceforge.net/p/naps2/tickets/) - For bug reports, feature requests, and general support inquiries.
- [Discussion Forums](https://sourceforge.net/p/naps2/discussion/general/) - For more open-ended discussion.
- [Donate](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=M77MFAP2ZV9RG)

**NAPS2 proposed 5.9.x dev-branch changes**

Done
- Now uses .NET Framework 4.7.2; instead of a mix of 4.0 / 4.5 / PCL.
- NuGet packages updated, and references using the newer format for inclusion.
- Aggressively re-factored via Rosylnator. A lot of changes were made, that appear to reflect C# 7.x additions.
- The latest VS2017 appears to prefer Windows Form variables to be capitalized. This has been accomplished: via an egregious use of Find/Replace, that no doubt left some odd errors somewhere.
- Used [CodeMaid](http://www.codemaid.net/) to clean-up the code base styling and format.

In Progress
- Finish off the VS2017 & Roslynator-located warnings and errors.
- WIABackgroundThread could use additional attention: may be source of issues such as feed scans not working right.
