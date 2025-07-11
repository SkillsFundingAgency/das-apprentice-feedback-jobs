## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Apprentice Feedback Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-apprentice-feedback-jobs?repoName=SkillsFundingAgency%2Fdas-apprentice-feedback-jobs&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-apprentice-feedback-jobs?repoName=SkillsFundingAgency%2Fdas-apprentice-feedback-jobs&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-apprentice-feedback-jobs&metric=alert_status)](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-apprentice-feedback-jobs&metric=alert_status)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

This azure functions solution is part of Apprentice Feedback (APPFB) project. Here we have background jobs in the form of Azure functions that carry out periodical jobs.

## How It Works

The apprentice feedback summary job causes the summary table of the latest feedback (stars) to be rebuilt.

The generate feedback transactions job causes the email template schedule to be created for an apprentice feedback target.

The process feedback target variants job causes the variants file to be loaded from blob storage and variant information to be stored in the database.

The process feedback transactions job causes emails to be sent according to the current schedule.

The update apprentice feedback targets job causes the learner details to be updated for an apprentice feedback target.

## 🚀 Installation

### Pre-Requisites
* A clone of this repository

## Developer Setup
### Requirements

In order to run this solution locally you will need:
- Install [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
- Install [.NET Core 8.0](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- Install [Visual Studio 2022 (Community or more advanced)](https://visualstudio.microsoft.com/vs/community/)
- Install [SQL Server 2019 (or later) Developer Edition](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Explorer](http://storageexplorer.com/)

### Config

You can find the latest config file in [das-employer-config repository](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-apprentice-feedback-jobs/SFA.DAS.ApprenticeFeedback.Jobs.json). 

* **Azure Table Storage Explorer** - Add the following to your Azure Table Storage Explorer.

    Row Key: SFA.DAS.ApprenticeFeedback.Jobs_1.0

    Partition Key: LOCAL

    Data: [data](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-apprentice-feedback-jobs/SFA.DAS.ApprenticeFeedback.Jobs.json)

Alternatively use the [das-config-updater](https://github.com/SkillsFundingAgency/das-employer-config-updater) to load all the current configurations.

You will also need to create the following Blob storage, open up Azure Storage Explorer, right click on the Blob Containers and click on Create Blob Container and name the following container **apprentice-feedback-template-variants**

In the `SFA.DAS.ApprenticeFeedback.Jobs` project, if not existing already, add `local.settings.json` file (Copy to Output Directory = Copy always) with following content:
```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsServiceBus": "Disabled",
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "AzureWebJobsDashboard": "UseDevelopmentStorage=true",
        "ConfigNames": "SFA.DAS.ApprenticeFeedback.Jobs",
        "EnvironmentName": "LOCAL",
        "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "ApprenticeFeedbackSummarySchedule": "*/5 * * * * *",
        "GenerateFeedbackTransactionsSchedule": "*/30 * * * *",
        "ProcessFeedbackTargetVariantsSchedule": "0 0 3 * * *",
        "ProcessFeedbackTransactionsSchedule": "*/30 * * * *",
        "UpdateApprenticeFeedbackTargetSchedule": "*/30 * * * *"
    }
}
```

In the above file the AzureWebJobsServiceBus is set to Disabled this will turn off the NServiceBus integration locally, alternatively obtain a running Azure Service Bus and configure it to work with service bus message locally.

## 🔗 External Dependencies

* The Apprentice Feedback Outer API defined in [das-apim-endpoints](https://github.com/SkillsFundingAgency/das-apim-endpoints/tree/master/src/ApprenticeFeedback) to connect to the Inner API.
* The Apprentice Feedback Inner API defined in [das-apprentice-feedback-api](https://github.com/SkillsFundingAgency/das-apprentice-feedback-api) to connect to the database.
* The database defined in [das-apprentice-feedback-api](https://github.com/SkillsFundingAgency/das-apprentice-feedback-api) as the primary data source.


### 📦 Internal Package Dependencies
* SFA.DAS.Configuration.AzureTableStorage

## Technologies
* .Net 8.0
* Azure Functions V4
* Azure Table Storage
* NUnit
* Moq
* FluentAssertions

