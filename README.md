# One.NET
An old CMS written in old technology, but still works quite well for tree based websites.

## Getting started

### Requirements
- Microsoft .NET framework 4.5
- Microsoft SQL server 2014
- IIS8

### Installation
There are a few way to install One.NET, but the most typical is to install it as a nuget module in a blank Visual Studio project, where you will add project specific code and files.

1. Open Visual studio 2015
2. File -> New -> Project -> ASP.NET Web Application -> (choose name and folder) -> (select "Empty" template)
3. Add One.NET package in Package Manager Console
```Powershell
Install-Package One.NET 
```
4. Create folder where specific site files (like css, templates,...) will be stored. By Convention the name of the folder should start with "Site". If you just need to get started fast, use default folder "site_specific".
5. "SiteDefault" folder contains sensible examples and starting files. Ideally you should copy them to your new "site" folder and then modify them. There are two exceptions:
- gitignore.template is just a typical .gitignore file for Visual studio and you should copy it to root if desired
- LocalRewrite.config, web.config and web.sitemap should be copied to root of the project
6. Edit web.config to include connection strings to at least two databases:
- DefaultConnection: connection to ASP.NET authentication database (you'll need a user there with admin role to boostrap the development)
- MsSqlConnectionString: this is where all of the CMS data will be stored.
7.... ?

## Documentation

[TemplateContent module](https://github.com/renderspace/onenet/blob/master/docs/TemplateContent.md)

## Report bugs
Yes, please!
You can [view existing issues](https://github.com/renderspace/onenet/issues) or [report a new issue](https://github.com/renderspace/onenet/issues/new).
Even better, you can contribute a solution:

## Contributing
Just send a pull request.

## License

One.NET is licensed under GNU GPL 3.0 as of ___ ___ ___ as approved by CEO of Renderspace. 

Copyright 2006-2016 [Renderspace d.o.o.](https://www.renderspace.si)

[![GNU GPL v3.0](http://www.gnu.org/graphics/gplv3-127x51.png)](http://www.gnu.org/licenses/gpl.html)
