This folder contains the configuration for a local dev environment for EFGS for NL.

* Execute the two SQL statements in your EFGS docker container to register the dutch certs with your local test EFGS.
* Put both of the Java Key Stores (*.jks) in the correct folder as per the EFGS readme.
* Install the **nl-auth** and **nl-signing** certificates into local machine in Windows (use the pfx!).
	* PFX password: `qwerty123`
* TODO: someone want to translate ^ for Linux and OSx?
* Configure the correct thumbprint with EfgsUploader/EfgsDownloader as per requirements.
