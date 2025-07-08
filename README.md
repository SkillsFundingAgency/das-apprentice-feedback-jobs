## ⛔Never push sensitive information such as client id's, secrets or keys into repositories including in the README file⛔

# Apprentice Feedback Jobs

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">


[![Build Status](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-apprentice-feedback-jobs?repoName=SkillsFundingAgency%2Fdas-apprentice-feedback-jobs&branchName=main)](https://sfa-gov-uk.visualstudio.com/Digital%20Apprenticeship%20Service/_apis/build/status/das-apprentice-feedback-jobs?repoName=SkillsFundingAgency%2Fdas-apprentice-feedback-jobs&branchName=main)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-apprentice-feedback-jobs&metric=alert_status)](https://sonarcloud.io/api/project_badges/measure?project=SkillsFundingAgency_das-apprentice-feedback-jobs&metric=alert_status)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)

# Developer Setup
### Requirements

In order to run this solution locally you will need:
- Install [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
- Install [.NET Core 3.1](https://www.microsoft.com/net/download)
- Install [Azure Functions SDK](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local)
- Install [SQL Server 2019 (or later) Developer Edition](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- Install [SQL Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- Install [Azure Storage Emulator](https://go.microsoft.com/fwlink/?linkid=717179&clcid=0x409) (Make sure you are on atleast v5.3)
- Install [Azure Storage Explorer](http://storageexplorer.com/)
### Environment Setup

* **local.settings.json** - Create a `local.settings.json` file (Copy to Output Directory = Copy always) with the following data:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true;",
    "AzureWebJobsDashboard": "UseDevelopmentStorage=true;",
    "AppName": "das-apprenticefeedback-jobs",
    "EnvironmentName": "DEV",
    "ConfigurationStorageConnectionString": "UseDevelopmentStorage=true;",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsServiceBus": "Disabled",
    "Version": "1.0"
  },
  "ApprenticeFeedbackOuterApi": {
    "ApiBaseUrl": "https://localhost:5131/",
    "SubscriptionKey": "key",
    "ApiVersion": "1"
  },
  "NServiceBusConfiguration": {
    "License": ""
  },
  "FunctionsOptions": {
    "ApprenticeFeedbackSummarySchedule": "0 */3 * * *",
    "GenerateFeedbackTransactionsSchedule": "*/30 * * * *",
    "ProcessFeedbackTransactionsSchedule": "*/30 * * * *",
    "UpdateApprenticeFeedbackTargetSchedule": "*/30 * * * *",
    "ProcessFeedbackTargetVariantsSchedule": "*/30 * * * *"
  },
  "FeedbackTargetVariantConfiguration": {
    "BlobStorageConnectionString": "UseDevelopmentStorage=true",
    "BlobContainerName": "apprentice-feedback-template-variants",
    "ArchiveFolder": "archive",
    "FeedbackTargetVariantBatchSize": 100
  },
  "UpdateBatchSize": 100,
  "EmailBatchSize": 100
}
```

* **Azure Table Storage Explorer** - Add the following to your Azure Table Storage Explorer.

    Row Key: SFA.DAS.ApprenticeFeedback.Jobs_1.0

    Partition Key: LOCAL

    Data: [data](https://github.com/SkillsFundingAgency/das-employer-config/blob/master/das-apprentice-feedback-jobs/SFA.DAS.ApprenticeFeedback.Jobs.json)

You will also need to create the following Blob storage, open up Azure Storage Explorer, right click on the Blob Containers and click on Create Blob Container and name the following container **apprentice-feedback-template-variants**