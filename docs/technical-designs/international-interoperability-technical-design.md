# International Interoperability: Technical Design

**Version**: 0.2, 3 september 2020

**Status**: Draft

**Authors**: 

* Ryan Barrett
* Steve Kellaway
* ..

# Introduction

TODO blurb

# Contents

- Status
- Phase One
  * Overview
    - Publishing flow
  * Database
    - ExternalTemporaryExposureKeys
    - DailyKeys
    - IncomingBatchJobs
    - OutgoingBatchJobs
  * New Components
    - InteropKeySetEngine (IksEngine)
    - Interop Uploader
    - Interop Downloader
  * Existing Components
    - EKS Stuffing Generator
    - GGD Portal Backend
    - GGD Portal Frontend
    - Data model [TODO, OLD!]
    - EKS Engine [TODO]
    - Cleanup tools which are in development [TODO]
  * Appendix
    - ISO Codes
    - Additions for Functional Requirements

# Status

This document is current in draft form. Any section marked as [TODO] should be completely ignored. Other sections contain TODOs to mark work which still has to occur.

In general, the following is also required before this document can be considered complete:
- Reconsilation on the yet-to-be-published software architecture document for the EU federation gateway.
- In lieu of the above, reconsiliation vs the current published source code.
- Reconsiliation against our solution architecture document to ensure that the concepts are aligned over all three documents (with the EU Federation Gateway leading)

And then
- Peer review!

# Phase One

TODO BLURB

Some of the changes required for interop will have a big impact on our current implementation. Those are:

- TemporaryExposureKeys can no longer be deleted by the clean-up tasks after they have been published.
- Stuffing keys must be persisted in our database.
- ..

## Overview

This high level overview shows the main components, data (grey) and external systems (orange).

Interoperability places a different set of constraints on the server so some functionality will be moved. This helps to simplify the design of the system, reducing the number of concepts, runtime artifacts and tables.

The design is based on standard Software Engineering practises. This leads the design to use a staging process to separate raw and production data; as well as to keep these processes separated for the existing NL server and the new interop dataflows.

![Overview](images/Overview.png "Overview of the system")

The take-away from this diagram is that with some minor changes to the server we can treat Interop as a mirrored, or special case of our normal flow.

### Publishing flow

This diagram shows how the publishing components are to be executed. The take-away from this is the requirement for the process to run sequentially. This is a simplication of the initial design made to reduce complexity given the constraints of our runtime infrastructure.

![Flow](images/Flow.png "Overview of the publishing flow")

## Database

TODO
- Interop tables into their own schema?
- Jobs tables into the jobs db?

### Workflow: ExternalTemporaryExposureKeys (new)

This is a staging table, it holds the incoming TEKs from the Federation Gateway. It mirrors the table `TemporaryExposureKeys` from the NL server.

```
CREATE TABLE [dbo].[ExternalTemporaryExposureKeys] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[IncomingBatchJobId] [bigint] NOT NULL,
	[KeyData] [varbinary](32) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[Origin] [varchar](2) NOT NULL,
	[CountriesOfInterest] [varchar](MAX) NOT NULL,
	[OriginTransmissionRiskLevel] [int] NOT NULL,
	[TransmissionRiskLevel] [int] NOT NULL,
	[ProcessingState] [int] NOT NULL,
	CONSTRAINT [PK_ExternalTemporaryExposureKeys] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
```

### Workflow: DailyKeys (new)

This table holds fully normalized, validated, stuffed and valid keys from all sources (NL & EU Federation Gateway). This table is the source of truth for the *EksEngine* and *InteropPubishingEngine*.

Keys stored in this table will always be published to an EKS (Exposure Key Sets) AND to IKS (Interop Key Sets). Keys which are not to be distributed are published to the ϵ sets.

Once published to both destinations keys are deleted from this table.

```
CREATE TABLE [dbo].[DailyKeys] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[KeyData] [varbinary](32) NOT NULL,
	[RollingStartNumber] [int] NOT NULL,
	[RollingPeriod] [int] NOT NULL,
	[Origin] [varchar](2) NOT NULL,
	[CountriesOfInterest] [varchar](MAX) NOT NULL,
	[TransmissionRiskLevel] [int] NOT NULL,
	[PublishedToEksOn] [datetime] NULL,
	[PublishedToInteropOn] [datetime] NULL,
  [IsStuffing] [bit] NOT NULL
	CONSTRAINT [PK_DailyKeys] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)
)
```

TODO: 
- IsStuffing, is this needed?
- Dates so that EOL can be ensured. 
- Ryan, better describe the epsilon sets, it's stolen from automata theory and not everyone will know that.

### Workflow: IncomingBatchJobs (new)

This table contains information on interop incoming batches. It has a FK to ExternalTemporaryExposureKeys so batches are linked.

```
CREATE TABLE [dbo].[IncomingBatchJobs] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT(GETDATE()),
	[StartedOn] [datetime] NULL,
	[CompletedAt] [datetime] NULL,
	[Status] [int] NOT NULL DEFAULT(0),
	[RetryCount] [int] NOT NULL DEFAULT(0),
	[CompletionToken] [nvarchar](max) NULL,
	[TotalKeys] [int] NULL,
	[TotalCountries] [int] NULL,
)
```

TODO
- Review structure when the InteropDownloadEngine is designed
- Cross-reference with the latest Federation Gateway design when it's published.

### Workflow: OutgoingBatchJobs (new)

This table contains information on interop outgoing batches.

CREATE TABLE [dbo].[OutgoingBatchJobs] (
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CreatedOn] [datetime] NOT NULL DEFAULT(GETDATE()),
	[StartedOn] [datetime] NULL,
	[CompletedAt] [datetime] NULL,
	[Status] [int] NOT NULL DEFAULT(0),
	[RetryCount] [int] NOT NULL DEFAULT(0),
	[TotalKeys] [int] NULL
)

TODO
- Review structure when the InteropUploadEngine is designed
- Cross-reference with the latest Federation Gateway design when it's published.

### OLD

Review against the new simplified design, most can probably go.

#### Workflow: TemporaryExposureKeys

- Add the field `Origin` as a `char(2) NOT NULL` and `DEFAULT ('NL')`.
- Add the field `IsStuffing` as a `bit NOT NULL`.
- Remove the field 'Region'.

#### Workflow: TekReleaseWorkflowState

- Add the field `CountriesOfInterest` as a `varchar(255) NOT NULL` and `DEFAULT ('NL')`.

#### PublishingJob: EksCreateJobInput

- Add the field `CountriesOfInterest` as a `varchar(255) NOT NULL`.
- Add the field `Origin` as a `char(2) NOT NULL` and `DEFAULT ('NL')`.

#### Workflow: vwTemporaryExposureKeys

- Add the fields `CountriesOfInterest`, `Origin` and `IsStuffing`.
- Remove the field `Region`.

#### Workflow: vwTekReleaseWorkflowState.sql**

- Add the field `CountriesOfInterest`.

## New Components

TODO rewrite blurb

### InteropKeySetEngine (IksEngine)

This component generates EKS from keys in the table `DailyKeys` which have the `Origin` of `NL`, where `PublishedToEksOn` is not null and `PublishedToInteropOn` is null. The keysets are then stored in the ContentDb with the tag `IKS`.

Almost all of the logic in this tool is re-used from the `EksEngine`.

The `egress filter` is implemented in this component.

Conceptually it is the mirror of the EksEngine.

### Interop Uploader (TODO: finish)

This component is responsible for uploading published TEKs - both real and stuffing - to the *EU Federation Gateway*. The tool takes the unpublished IKS from ContentDb and uploads them one-by-one and only once to the EU Federation server.

The upload accepts a protocol-buffer format keyset: https://github.com/eu-federation-gateway-service/efgs-federation-gateway/blob/master/src/main/proto/Efgs.proto.

The file is then uploaded to: [TODO].

Conceptually it is the mirror of the ContentApi.

### Interop Downloader

This component is responsible for downloading the keys from the interop server and putting them in the table `ExternalTemporaryExposureKeys`. It supports the batching logic of the EU Federation gateway.

It applies the same validation rules as postkeys (via re-use), dropping any keys which we do not consider valid.

#### Register callback (RegisterInteropCallbackCommand)

The EU federation gateway implements a callback functionality to notify the backend that there are new keysets available. The interop downloader will initally register our callback url with the federation gateway.

TODO: this would be ideal functionality for an Administration Portal?

The gateway implements a CRUD interface over callbacks, which can be found here: https://github.com/eu-federation-gateway-service/efgs-federation-gateway/blob/master/src/main/java/eu/interop/federationgateway/controller/CallbackAdminController.java

The command will first check to see if our callback is registered by calling `getCallbackSubscriptions`. If this does not return our callback then 

The callback will be configurable, with the following options:

- `federationGatewayBaseUrl`
  * the base url for EU Federation gateway.
  * e.g. : `https://someurl/something`
- `callbackId`
  * our free-text identifier for our callback.
  * e.g. `2020-09-05_Prod`
- `callbackUrl`
  *  our callback url, a full URL.
  * e.g. `https:/coronamelder-api.nl/v1/efg-callback`
- DB settings

More details on callbacks can be found here: https://github.com/eu-federation-gateway-service/efgs-federation-gateway/blob/master/docs/implementation_details/callback.md

#### Callback handler (HandleInteropCallbackCommand)

The handler will be called once per batch job published by the EU federation gateway. The gateway will retry the callback until we return a success.

We must process every batch which is published to interop.

This command will accept the callback, write it to the database table `IncomingBatchJobs` and then return an OK once the transaction has committed.

#### Batch downloader (InteropBatchDownloaderCommand)

This command will download the batches found in the table `IncomingBatchJobs` one for one. It will download the data, deserialize it and then put the content into the table `ExternalTemporaryExposureKeys`.

The keys will be validated using a subset of our validation rules:
- <TODO details>

Each batch will be processes once and once only. To ensure that this happens an exclusive row-level lock will be held on each batch as it is being processed. This can be implemented by selecting the ID of the single batch ID under the `serializable` transaction isolation level. That can be achieved via the hint `HOLDLOCK` or by setting the isolation level correctly on the context.

Once a batch has been processed the status will be changed to `BatchStatus.Downloaded`.

Conceptually it is the same as /postkeys from the Mobile API.

#### Process batches (InteropBatchProcesserCommand)

This component is responsible for taking raw keys from the table `ExternalTemporaryExposureKeys`, implementing the `Ingress Filter` and then copying the keys which pass the filter to the table `DailyKeys`. Once the keys are copied they will be removed from `ExternalTemporaryExposureKeys` (as part of the same transaction).

Conceptually this is the same as the first part of the existing EksEngine.

This command will iterate through all of the downloaded batched of status `Downloaded`.

First the keys for the batch will be loaded from `ExternalTemporaryExposureKeys` and we will apply our *ingress filter* them. The filter consists of the following checks:
- TODO
- TODO

Keys which pass the filter will be inserted into the table `DailyKeys` and marked as processed. Those that failed will be marked as processed and the details logged.

Once the batch is complete the batch status will be uploaded to `BatchStatus.Processed` and all of the processed keys from that batch will be deleted from the table `ExternalTemporaryExposureKeys`. The numbers being logged.

Once all of the batches have been processed then they will be removed from the table `IncomingBatchJobs`. The numbers and details logged.

The following settings will be added:
- TODO
- TODO

## Existing Components

One of the main design goals for phase one is to minimize the changes to existing components whilst minimizing the impact on our support of the interop standard.

Short summary of changes:
- EKS engine command will be split to allow reuse. Code changes are minimal.
- EKS engine will be modified to use the new tables. Code changess are minimal as schema changes are also minimal.
- EKS engine command will gain an origin-filter
- Some commands may need to be further parameterized so they can be reused for both Iks and Eks engine.

### EKS Stuffing Generator

*Components\ExposureKeySetsEngine\EksStuffingGenerator.cs*

* Add a function `GenerateCountriesOfInterest()` which will randomly choose one or more of the ISO country codes from the list and return that together with our own land code (`NL`).  The list will be weighted such that both the number and chosen countries are similar to actual data.
* Set `EksCreateJobInputEntity.Origin` to `NL`.
* Set `EksCreateJobInputEntity.CountriesOfInterest` to the results of `GenerateCountriesOfInterest()`.

### GGD Portal Frontend

The GGD Portal will be modified to allow the GGDs to select the countries visited from a list. 

The design looks like this:

<TODO: insert design>

**app\services\lab-confirm.service.ts**

Add the selected countries to `this.data.CountriesOfInterest` in `confirmLabId`. The countries must be a comma-seperated list in the form: `NL,DE,BE,FR`.

### GGD Portal Backend 

**Components\Workflow\Authorisation\AuthorisationArgs.cs**

- Add `CountriesOfInterest` as a string property.

**Components\Workflow\Authorisation\AuthorisationArgsValidator.cs**

- Add validation for `CountriesOfInterest`, it must be matched by this regex: `^([A-Z]{2},?)+$`.
- The values in the list must be in the ISO-3166-2 country code table.
- `NL` must be included in the list.

**Components\Workflow\Authorisation\AuthorisationWriterCommand.cs**

- Set `wf.CountriesOfInterest = args.CountriesOfInterest` in `Execute()`.

### Cleanup tools which are in development [TODO]

Only delete published TEKs after 14 days.
Update stats so that they take int account the `IsStuffing` flag.

### Data model [TODO, OLD!]

The fields Origin and Countries Of Interest must be added to the database and our data models. Both of these fields contain a two-letter ISO-3166-2 country code listed in the [appendix](#Appendix). The default value for both fields is `NL`.

**Components\EfDatabase\Entities\TekEntity.cs**

- Remove the property `Region`.
- Add `Origin` as a string property of length two matching this regex `^[A-Za-z]{2}$`.

**Components\EfDatabase\Entities\TekReleaseWorkflowStateEntity.cs**

- Add `CountriesOfInterest` as a string property matching this regex `^([A-Z]{2},?)+$`.

**Components\EfDatabase\Entities\EksCreateJobInputEntity.cs**

- Add `CountriesOfInterest` as a string property.
- Add `Origin` as a string property.

**Dacpac: table TemporaryExposureKeys in Workflow**

- Add the field `Origin` as a `char(2) NOT NULL` and `DEFAULT ('NL')`.
- Add the field `IsStuffing` as a `bit NOT NULL`.
- Remove the field 'Region'.

**Dacpac: table TekReleaseWorkflowState in Workflow**

- Add the field `CountriesOfInterest` as a `varchar(255) NOT NULL` and `DEFAULT ('NL')`.

**Dacpac: table EksCreateJobInput in PublishingJob**

- Add the field `CountriesOfInterest` as a `varchar(255) NOT NULL`.
- Add the field `Origin` as a `char(2) NOT NULL` and `DEFAULT ('NL')`.

**Devops\Database\vwTemporaryExposureKeys.sql**

- Add the fields `CountriesOfInterest`, `Origin` and `IsStuffing`.
- Remove the field `Region`.

**Devops\Database\vwTekReleaseWorkflowState.sql**

- Add the field `CountriesOfInterest`.

### EksEngine [TODO]

* Insertion of keys from origins other than NL into our Exposure Key Sets.
* We need to add all keys at this point: we shouldn't actually need to change anything in the EKS engine to get that. Only we do have to implement new stuffing requirements, as well as inserting the stuffing into the database AND making sure that nothing is deleded.

# Phase Two

* Make the ISO country codes list dynamic/configurable everywhere.
* Generate an EKS per country.
* Generate a manifest per country.
* Extend the content-api endpoints so we get: `manifest/{ISO-3166-2}`, `exposurekeyset/{ISO3166-2}`.

# Appendix

## ISO 3166-2 Codes

| Code | Country                |
| ---- | ---------------------- |
| XX   | Unspecified / Unknown  |
| NL   | The Netherlands        |
| DE   | Germany                |
| BE   | Belgium                |
| DK   | Denmark                |
| PL   | Poland                 |
| IE   | Ireland                |
| AT   | Austria                |
| ES   | Spain                  |
| IT   | Italy                  |
| CZ   | Czech Republic         |
| EE   | Estonia                |
| LV   | Latvia                 |

Other codes should be added but will not be shared.

## Missing requirements from the `Architecture` document

* In the stuffing, the `Origin` and `Region of Interest` should not be randomized between all regions. Most keys are from NL. It should match the distribution of keys per country (or a set percentage).

* The architecture talks about proving `Region of Interest` per day from the GGDs. That's not workable. Designers have designed this PER Lab Code

* Archtecture talks abount the GGD
