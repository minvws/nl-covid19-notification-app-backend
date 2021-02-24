# Changelog GGD Portal

Dit is het changelog van het de design van het GGD-app-portal. 
Dit is het systeem waarin medewerkers van de GGD tijdens een telefoongesprek met de gebruiker van de CoronaMelder de GGD-sleutel moeten invoeren. 

De verschillende versies van de designs van het GGD-app-portal zijn beschikbaar via [de Figma van het project](https://www.figma.com/file/EJ4aJwKnemkxysCZ6aAzFv/Covid-19-notificatie-app-(Read-only)?node-id=29297%3A0). 


## 0.4 
Versie 0.3 is getest in een dubbeltest met 2 GGD-medewerkers op locatie simultaan met app-gebruikers in het lab ([inzichten](http://corona.sticktailapp.com/study-share/mgzWw55GdKxA/dubbeltest-app-app-portaal-115/)). 
Later ook nog met bco-medewerkers doorgesproken ([inzichten](http://corona.sticktailapp.com/study-share/z7FWobsVMbIe/meelopen-en-test-ggd-169/)). 
En de test incl. een e-learning versie 0.1 is ook getest met 2 bco-medewerkers ([inzichten](http://corona.sticktailapp.com/study-share/oLE5AHArHrzt/test-app-portaal-bij-de-ggd-702/)).

Deze hebben geleid tot de volgende changes:

- We tonen bij invullen van eerste ziektedag ook de besmettingsdag. Die wordt automatisch berekend
- Linkje naar de e-learning
- Visuele que voor GGD medewerkers de index te vragen de telefoon op speaker te zetten
- Code upload flow screenshots toegevoegd
- Als index codes upload ga je automatisch naar de succes state (een aangepaste versie)


## 0.3 
Versie 0.2 en 0.3 van het GGD-app portaal is getest met medewerkers van de GGD op afstand, en in dezelfde week op 2 locaties. 
Tussen de testen door zijn de changes 0.3 gemaakt. 
[De inzichten van deze testdagen](http://corona.sticktailapp.com/study-share/pJjR4djzQYWt/app-portaal-testen-bij-de-ggd-714/) zijn gebundeld in de documentatie en  hebben geleid tot de volgende aanpassingen in het design:

- Screenshots in stap 2 zijn geupdate zodat die gelijk zijn aan het laatste design van de CoronaMelder-app. 
- Link om uit te loggen uit het portal is toegevoegd. 
- Om meer de aandacht te leggen op de instructie en minder op de eventuele vragen zijn de kleuren van de background en tekstvelden aangepast. 
- Label toegevoegd boven de vragen omdat we in onderzoek zagen dat sommige GGD-medewerkers ook de vragen gingen voorlezen.
- App icoon toegevoegd aan de screenshots.


## 0.2 
- Er zijn aanvullende instructies toegevoegd bij de verschillende stappen in het portal zodat er een belscript ontstaat in het app-portaal. Dit moet ervoor zorgen dat er geen aparte werkinstructie hoeft te komen waar medewerkers informatie/instructies in moeten opzoeken. Ook als later het werkproces wordt aangepast is dit gelijk zichtbaar.
- De term tijdelijk wachtwoord is aangepast naar GGD-sleutel. Uit onderzoek bleek dat [mensen geleerd hebben geen wachtwoorden door te geven aan de telefoon](https://corona.sticktailapp.com/share/view/def89e67a9ae1b8/rYar1oji4Yqe/mijn-wachtwoord-geef-ik-nooit-weg-over-privacy/).
- Label “sinds wanneer” is aangepast naar “Eerste ziektedag” dit is een herkenbare term voor medewerkers van de GGD. 
- De stap symptomen ja/nee eruit gehaald, deze is overbodig. 
- Het invullen van andere landen waar de patiënt geweest is, is uit het design gehaald. Deze functionaliteit wordt nog niet op korte termijn ondersteund. 
- Het invullen de de Eerste ziektedag staat nu op het eerste scherm omdat de validatie van de GGD-sleutel pas gedaan kan worden als ook de eerste ziektedag bekend is. 


## 0.1 
Voorafgaand aan het ontwerpen van het GGD-app-portal heeft een van de gebruikersonderzoekers meerdere dagen meegelopen op locatie bij een GGD om te zien hoe bron en contactonderzoek (BCO) wordt uitgevoerd. De documentatie van deze onderzoeken:
- [Meeloopdag 1](http://corona.sticktailapp.com/study-share/AeHf5ulXRQL4/meeloopdag-ggd-fryslan-324/) op 19 mei
- [Meeloopdag 2](http://corona.sticktailapp.com/study-share/tHbLG3OXvq3H/meeloopdag-ggd-bron-en-contactonderzoek-804/) (specifiek bij het BCO) op 3 juni
- [Meeloopdag 3](http://corona.sticktailapp.com/study-share/F29AF8mGiOUF/meelopen-in-testfaciliteit-644/) (bij testfaciliteit) op 9 juni
- [Meeloopdag 4](http://corona.sticktailapp.com/study-share/r5XRgcAehLpB/meeloopdag-ggd-330/) op 16 juni

De inzichten uit deze meeloopdagen zijn gebruikt om de eerste versie van het design te maken:
- Het is belangrijk dat het invoeren van de benodigde gegevens in het GGD-app-portaal zo simpel mogelijk is en tijdens het telefoongesprek uitgevoerd kan worden. GGD medewerkers zijn gewend om complexere administratieve taken te verplaatsen tot later op de dag. Daarom is ervoor gekozen:
    - om zo min mogelijk handelingen en stappen in het ontwerp op te nemen. 
    - de stappen die er zijn zo gebruiksvriendelijk mogelijk te maken. De GGD-medewerker weet wat en waarom hij moet doen zonder een extra werkinstructie/ handleiding ernaast. 
    - het schakelen met andere applicaties niet nodig is en er geen informatie eerst ergens anders opgeslagen dan wel opgehaald hoeft te worden.  
- De app maakt gebruik van single sign on via identity hub van de GGD. Zo hoef je niet opnieuw in te loggen met een ander wachtwoord tijdens een telefoongesprek met een patiënt. Er is dus geen inlog dialoog in het ontwerp opgenomen. 
- Omdat de patiënt tijdens het telefoongesprek een tijdelijk wachtwoord  vanuit de app moet oplezen aan de GGD-Medewerker is het handig als de medewerker kan zien wat de patiënt ook ziet in de app, dan kan je makkelijker instructies geven. Daarom zijn er screenshots van de app met daarin de stappen die de patiënt moet uitvoeren toegevoegd aan in het GGD-app-Portal.  
- Om het GGD-app-portal echt ondersteunend te laten zijn aan het telefoongesprek zijn er ook antwoorden op vragen opgenomen in het portal die de medewerker kan verwachten tijdens het telefoongesprek.




