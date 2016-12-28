# Visual Studio Extensions

This is the list of Visual Studio Extensions I use when developing for a combined web and mobile application:

## .ignore

There are many files that get added to projects that don't really need an editor.  They get set once and then
forgotten about.  One of those is `.gitignore`.  There are others that follow this same pattern.  So there is
an extension for adding them and dealing with them.

[More Information][1]

## Add New File

Not all files that you need to create have an extension.  A couple that I tend to have to deal with are README
and LICENSE.  They have no extension.  Others include the `.babelrc` file for configuring BabelJS.  With this
extension, you can create any file with any extension you need.

[More Information][2]

## Editor Enhancements

I love small, single function extensions like this.  This one provides HTML and URL encoding - something I have
to do a lot of when I am developing for the web.

[More Information][3]

## EditorConfig

There is a specification for a file called `.editorconfig` that will configure your editor for the requirements
of a project in terms of character set, line ending, what a tab means and so on.  This extension reads that
file and configured the settings for the project accordingly.

[More Information][4]

## File Icons

Visual Studio comes with a pretty good list of file icons that appear in the Solution Explorer to tell you what
sort of file it is.  But it isn't as exhaustive as it could be.  Two of our extensions thus far have dealt with
adding files that Visual Studio can't handle - this gives them icons.

[More Information][5]

## Glyphfriend 2015

In our web applications, we use a lot of glyphs.  These are available through web frameworks like [Bootstrap].
This extensions shows what the glyph actually looks like right in the editor, which helps me ensure what I am
doing is correct.

[More Information][6]

## Indent Guides

I sometimes write really long blocks of code.  This extension tells me where the blocks start and end.

[More Information][7]

## NPM Task Runner

If you are doing web development, this is not only recommended; it's vital.  It allows you to run npm commands
from within the Task Runner Explorer, thus relieving you of the process of dropping down to the command line.

[More Information][8]

## Open Command Line

For those occassions when you just can't avoid dropping down to the command line, this will ensure your command
line starts at the right place.

## Package Installer

In web development, sometimes you need to grab a package from elsewhere.  Unfortunately, there are a dozen different
ways of grabbing that package.  This extension deals with all the myriad ways of getting the package.

[More Information][10]

## Regex Tester

Are you a regex master?  Me neither, which is why I like to test all my regular expressions before they go in my
code.  This extension adds that functionality as a window.

[More Information][11]

## Roslynator

Roslyn was a major step forward in the power of C#, and there are a number of ways that it makes your code cleaner.
Unfortunately, you have to learn them - all 160+ of them.  This extension looks for common refactorings for you, so
you can carry on coding as before.  Over time, you will learn how to do those things in Roslyn right the first time.

[More Information][12]

## SQLite / SQL Server Compact Toolbox

Azure Mobile Apps uses SQLite as the basis of its offline cache.  That means that occassionally you are going to need
to peek inside the SQLite database, even if you are only curious.

[More Information][13]

## SQLite for Universal Windows Platform

Did I mention Azure Mobile Apps uses SQLite?  If you are developing UWP applications, you will need this extension.

[More Information][14]

## Trailing Whitespace Visualizer

Languages are sometimes a little problematic when it comes to embedded white space at the end of lines, especially in
multi-line strings.  This extension shows them up in the editor, allowing you to easily find and destroy them.

[More Information][15]

## Web Essentials

There are few extensions that are more needed when you switch to web development.  This provides capabilities for all
the common file types used for web development, including style sheets and JavaScript files.

[More Information][16]

## Web Extension Package

This isn't one extension.  It's several.  There are several extension modules for image optimizations, bundling,
icon handling and accessibility monitoring.  This one extension installs all the others.  Use this if you are
going to be doing web + mobile development.

[More Information][17]

## Xamarin Forms Player

One of the big gotchas in Xamarin Forms development is the process by which the XAML is parsed, built and reviewed.
If you install the Xamarin Forms Player onto a device, this extension will push your XAML to that app and the app
will render the page for you, allowing a much tighter development cycle.

[More Information][19]

## Xamarin Forms Templates

The default Xamarin Forms templates include projects for iOS and Android.  I wanted UWP, Windows Phone 8.1 and
potentially others as well.  This set of templates gives me those additional templates.

[More Information][20]

## Xamarin Test Recorder

Creating UI tests is painful.  Running them on a large number of mobile devices is also painful.  Fortunately,
this extension handles the former - creating UI tests.  You can run the same UI tests across thousands of devices
by using [Visual Studio Mobile Center][vsmc] testing facilities.

[More Information][21]

## XAML Styler

I find organizing XAML tedious.  Fortunately, I can clean up my XAML and give it a consistent style by using this
extension.  You may not agree with its opinions, but at least it will make your code consistent.

[More Information][22]

Taking a look at all these extensions, a big shout-out goes to [Mads Kristensen][mads].  He may work at Microsoft,
but he puts out the great web extensions that make Visual Studio a major player in that space.

<!-- Links -->
[mads]: http://madskristensen.net/
[Bootstrap]: http://getbootstrap.com/
[vsmc]: https://mobile.azure.com/

[1]: https://github.com/madskristensen/IgnoreFiles
[2]: https://marketplace.visualstudio.com/items?itemName=MadsKristensen.AddNewFile
[3]: https://github.com/madskristensen/Editorsk
[4]: http://editorconfig.org/
[5]: https://github.com/madskristensen/FileIcons
[6]: https://marketplace.visualstudio.com/items?itemName=RionWilliams.Glyphfriend
[7]: https://marketplace.visualstudio.com/items?itemName=SteveDowerMSFT.IndentGuides
[8]: https://github.com/madskristensen/NpmTaskRunner
[10]: https://marketplace.visualstudio.com/items?itemName=MadsKristensen.PackageInstaller
[11]: https://marketplace.visualstudio.com/items?itemName=RomanKurbangaliyev.RegexTester
[12]: https://github.com/JosefPihrt/Roslynator
[13]: https://github.com/ErikEJ/SqlCeToolbox
[14]: http://www.sqlite.org/
[15]: https://marketplace.visualstudio.com/items?itemName=MadsKristensen.TrailingWhitespaceVisualizer
[16]: http://vswebessentials.com/
[17]: https://marketplace.visualstudio.com/items?itemName=MadsKristensen.WebExtensionPack
[19]: https://marketplace.visualstudio.com/items?itemName=MobileEssentials.XamarinFormsPlayer
[20]: https://marketplace.visualstudio.com/items?itemName=picolyl.XamarinFormsTemplates
[21]: https://www.xamarin.com/test-cloud/recorder
[22]: https://github.com/Xavalon/XamlStyler/
