# Architecture overview

The portal is designed to be deployed in a classic three-tier architecture:

         Presentation tier          |     Application tier    |    Data tier
                                    |                         |
     (internet)          (DMZ)      |    (Business Zone 1)    |  (Business Zone 2)
									
    +---------+      +-----------+     +-------------------+     +----------+
    | Browser |  --> | WebServer | --> |    Web Services   | --> | Database |
    +---------+      +-----------+     +-------------------+     +----------+
                                                |
                                                |    +----------------------+
                                                -->  | External WebServices |
                                                     +----------------------+



The frontend is built with Angular and runs as a Single Page Application (SPA) in the browser. The SPA sends requests to a server running in the Presentation Tier. This then forwards these requests to the Web Services running in the Application Tier. The webservices can then communicate with databases in the Data tier or with External Webservices as per the security policy.

In a standard deployment, ICC Portal will be deployed in the Presentation Tier, ICC Backend in the Application Tier and the databases in the Data Tier. Following industry standard security policies the Browser will not be able to access the Application Tier directly. 

## Internet Information Services (IIS): Advanced Request Routing (ARR)

You can use the ARR module of IIS to provide a reverse proxy between the Presentation Tier and the Application Tier.

In this situation you want to add a virtual directory within the root of the website containiner the Angular frontend. That virtual directory name becomes part of the url, so call it `IccApi`. Add a web.config into that virtual directory and add the ARR configuration to that web.config.

### ARR Configuration

Install ARR if it isn't already installed: https://www.iis.net/downloads/microsoft/application-request-routing

Once ARR has been installed it can be configured by adding the `<rewrite>` section under `<system.webServer>` in the  `Web.config`.

	<configuration>
	  <system.webServer>
		<rewrite>
		  <rules>
			<!-- This proxies calls to iccapi/* to the rewrite url. See deploy.md for more information -->
			<rule name="ReverseProxyApiRequestsRule" patternSyntax="ECMAScript" stopProcessing="true">
			  <match url="iccapi/(.*)" negate="false" />
			  <action type="Rewrite" url="http://URL-TO-MACHINE-IN-THE-BACKEND/IccApi/{R:1}" appendQueryString="true" logRewrittenUrl="true" />
			  <conditions></conditions>
			</rule>
		  </rules>
		</rewrite>
	  </system.webServer>
	</configuration>

#### URL pattern

We use this pattern to match the url:

^/api/(.*)

That pattern does require a forward-slash after API. If it didn't, then it would match any word starting with api.

Matches the following urls:

/iccapi/
/iccapi/method
/iccapi/method?query=string
/iccapi/method/?query=string

But not these:

/
/?query=string
/iccapi
/iccapii
/api
/api/
/api?query=string
/api/?query=string
/api/abi
/api/abi?query=string
/api/abi/?query=string

With this pattern the group {R:1} contains everything that is contained in the url after the forward slash. That is used to generate the url calling the backend services.


