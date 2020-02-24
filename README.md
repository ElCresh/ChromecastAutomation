# ChromecastAutomation
ChromecastAutomation è un progetto C# con lo scopo di gestire un unmero infinto di VM con a bordo Linux per pilotare un egual numero di Chromecast tramite VLC.
Questo sistema è stato progettato per cercare di ovviare in modo grezzo ma semplice al limite di VLC che può pilotare massimo un Chromecast per macchina.

Il progetto è composta dai seguenti componenti:
* VLCAutoChromecast
* VboxManager

### VLCAutoChromecast
VLCAutoChromecast si occupa di identificare il percorso di VLC e di utilizzarlo per pilotare il Chromecast indicato nella sua configutazione.

### VboxManager
VboxManager si occupa di verificare che le VM impostate nel suo file di configurazione siano sempre attive e operative. In caso di chiusura si preccuperà di avviarle nuovamente.
