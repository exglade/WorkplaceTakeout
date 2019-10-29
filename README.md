# WorkplaceTakeout
A simple console application for taking out (majority of) Workplace contents.

## Description

This is a custom-made takeout .NET Framework console application to extract content of interest from Workplace using Workplace integration.

This is not an official Facebook Workplace takeout application. I made this to extract data to migrate to other platform.

Be careful of the amount of data extracted. Please study the [Workplace's Platform Policies](https://developers.facebook.com/docs/workplace/introduction).

## Getting Started

### Prerequisites

* .NET Framework >4.6.1 - you can change the target framework, but this should be the minimum.
* [Visual Studio 2019](https://visualstudio.microsoft.com/vs/) - you can use Community edition. The required workload: .NET desktop development

### Dependencies

* [Facebook 7.0.6](https://www.nuget.org/packages/Facebook/)

### Executing 

* Get Facebook Workplace Integration Access Token.
* Edit appsettings.json:
  * Fill in facebook:AccessToken - for the calling API.
  * Fill in takeout:OutputFolderPath - takeout content will be populated in specified folder.
* Open WorkplaceTakeout.sln.
* Build & Run project.

## Help

### How do I get the Access Token?
You need a [custom integration](https://developers.facebook.com/docs/workplace/custom-integrations-new) which can be created in the Admin Panel as Workplace System Administrator.

### Which permissions do I need?
The demo within uses all *Read* access permissions. Read [Workplace permissions doc](https://developers.facebook.com/docs/workplace/reference/permissions).

### What other Facebook Workplace Graph API are there?
Read [Graph API doc](https://developers.facebook.com/docs/workplace/reference/graph-api).
