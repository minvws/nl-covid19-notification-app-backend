#Test scenarios

Test strategy to validate the backend system multiple type of test are defined:
## A. Endpoints tests => health of all endpoints
To validate if the endpoints are up-and-running the following validations are performed per endpoint:
- [x] response time
- [x] status
- [x] keys present
- [x] max-age
- [x] sig. validation
- [x] number of keys
- [ ] certificate => unclear which endpoints to validate => key file?
- [x] versioning (v1 and v2)

## B. Scenario's => integration of all endpoints (business rules)
	1. Happy flows
	2. timing
	3. sequence
	4. Frequency
	5. Error flows (invalid data)
	6. Versioning (different certificate validation)

### B. Scenario's workout

1. Happy flow scenarios (all 200 OK and manifest is updated)
    - [x] Register > LabConfirm > Postkeys > Labverify > wachten (6 minutes) > Manifest > EKS
      * determine transmissionRiskLevel (TRL) 1/2/3 based on de RollingStartNumber (RSN) + DateOfSymptomsOnset (aantal dagen tussen)
	  * validate new key is in manifest
	  * Etag is changed with new manifest (304 vs. 200)  
    - [x] validate all exposure keys in manifest 
    - [ ] API versioning => v1 validatie / v2 validatie => different certificate
    - [x] 13 keys in postkey array

2. Timing scenarios (validation of the business rules round the postkeys)
    - [ ] Postkey of 3 weeks is to old => not added to manifest
    - [ ] Postkey is to new (tomorrow) => not added to manifest
	- [ ] Register > LabConfirm > wachten > Labverify > (x minutes) > Labverify => validate that polltoken is expired and new is provided
	- [ ] Register > LabConfirm > wachten (x minutes) > Labverify > (121 minutes) Postkeys > Labverify => validate that polltoken is expired, no new token + false returned
	GAEN framework 1.4 versus 1.5
	    - [ ] 1.4 GAEN framework (key today + key yesterday): yesterday keys are processed & today not
	    - [ ] 1.5 GAEN framework (key today + key yesterday): yesterday is processed & today is delayed in backend

3. Sequence scenarios (validation of the business rules round the postkeys)
	- [x] Register > Postkeys > LabConfirm > Labverify > wachten (6 minutes) > Manifest > EKS

4. Frequency scenarios (validation of the business rules round the postkeys)
	- [ ] Register > Postkeys > Postkeys > Postkeys > Postkeys > Postkeys > Postkeys > Postkeys > Postkeys > LabConfirm > wachten (6 minutes) > Labverify > Manifest > EKS
	- [ ] Register > Postkeys > Postkeys > Postkeys > Postkeys > Postkeys > Register > Postkeys > Postkeys > Postkeys > LabConfirm > wachten (6 minutes) > Labverify > Manifest > EKS
	- [ ] Register > manifest => run x times like 50-100 times

5. Error flow scenarios (API + business rules validation, no 500 error is returned)
	- [ ] Invalid input data (bucketID, etc)
	- [ ] duplicate keys in keys in postkey array
	- [ ] already processed keys
	- [ ] 0, 14, 30 keys in postkey array

These happy flows need more workout based on business rules from https://github.com/minvws/nl-covid19-notification-app-coordination-private/blob/master/architecture/Key%20Upload%20Process.md

	- GAEN 1.4
		- collect manifest + all CDN data with etag
		- register => validity
		- get Bearer token ICC
		- labconfirm => timestamp
		- labverify => poltoken
		- postkeys (max 13) => within 2 hours of labconfirm + keys validation
		- labverify => true
		- manifest updated
		- collect new manifest => based on new etag => new key is added to the manifest

	- GEAN 1.5
		- collect manifest + all CDN data with etag
		- register
		- labconfirm
		- labverify
		- postkeys (max 13)
		- labverify => true
		- manifest updated
		- collect new manifest => based on new etag => new key is added to the manifest


Discussion
The difference between 1.4 and 1.5 GAEN framework is in the postkeys handling. Need to determine if the the user scenario's in
total need to be split-up in a 1.4 and 1.5 set?

GAEN 1.4 / 1.5
Up until the GAEN 1.4 version of the framework, retrieving keys from the framework was
'idempotent': every call on the same day would retrieve the same set of keys, and wouldn't change the underlying keys.

keyset
	- last 13 days
	- not send before
	- timestamp of the past