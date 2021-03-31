# Discord Bot

Funktionen
- Audio Streams abspielen 
- Audi Streams k�nnen fest hinterlegt werden, sodass nur der Name zum abspielen ben�tigt wird
- Benachrichtigen wenn ein Twich Channel Online geht

## Installation

### API Daten generieren

#### Discord

https://docs.stillu.cc/guides/getting_started/first-bot.html

#### Twitch

https://dev.twitch.tv/console/apps

URL: http://localhost

### Einstellungen

Die Einstellungen werden in der appsettings.json vorgenommen. Diese befindet sich immer im Release-Archiv und muss deshalb bei einem Update vor dem �berschreiben 
der vorhandenen Dateien gel�scht werden.

In dieser Datei werden unteranderem die Api Keys f�r Discord und Twitch (optional) hinterlegt.

### LavaLink

F�r das Abspielen der Lieder wird [Lavalink](https://github.com/Frederikam/Lavalink) verwenden. Dieses muss auf dem Server installiert werden. Daf�r ist eine Java Runtime notwendig.

Lavalink kann ebenfalls als Service angelegt werden. https://dzone.com/articles/run-your-java-application-as-a-service-on-ubuntu


### Linux (Systemd)

Nachdem die Abh�ngikeiten installiert worden sind, kann die Bot an sich installiert werden. Das Linux Archive entpacken, z.B unter `/usr/sbin/RCDiscordBot/` 
und die `RCDiscordBot.service` nach `/etc/systemd/system/` verschieben. Falls ein anderer Ordner verwendet wird, muss der Pfad noch angepasst werden.
```
[Unit]
Description=RC Discord Bot

[Service]
Type=notify
# will set the Current Working Directory (CWD). Worker service will have issues without this setting
WorkingDirectory=/usr/sbin/RCDiscordBot/
# systemd will run this executable to start the service
ExecStart=/usr/sbin/RCDiscordBot/RCDiscordBot
SyslogIdentifier=RCDiscordBot

# ensure the service restarts after crashing
Restart=always
# amount of time to wait before restarting the service              
RestartSec=5

[Install]
WantedBy=multi-user.target
```

```
// Datei laden
sudo systemctl daemon-reload

// Bot Starten
sudo systemctl start RCDiscordBot.service

// Autostart
sudo systemctl enable RCDiscordBot.service
```

### Windows

F�r Window muss nur das Archive entpackt werden. Die Runtime ist bereits enthalten. Der Bot kann entweder durch `DiscordMusicBot.Service.exe` gestartet werden. Der Bot
 kann mit `sc.exe create RCDiscordBot binpath="c:\xxx\Rc.DiscordBot.exe"` als Service installiert werden.

```
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "Enrich": [ "FromLogContext" ],
    "WriteTo": [
      {
        "Name": "Async",
        "Args": {
          "configure": [
            { "Name": "Console" },
            {
              "Name": "File",
              "Args": {
                "path": "logs/log.txt",
                "rollingInterval": "Day"
              }
            }
          ]
        }
      }
    ]
  },
  "Discord": {
    "BotToken": "",
    "Prefix": "!",
    "GameStatus": "!help"
  "Audio": {
    "Streams": []
  },
  "Twitch": {
    "ClientId": "",
    "Secret": "",
    "TwitchChannels": {
    },
    "OnlineCheckIntervall": 60
  },
  "Lavalink": {
    "Authorization": "youshallnotpass",
    "BufferSize": 512,
    "EnableResume": false,
    "Hostname": "127.0.0.1",
    "LogSeverity": "Info",
    "Port": 2333,
    "IsSSL": false,
    "UserAgent": null,
    "ReconnectAttempts": 10,
    "ReconnectDelay": "0:0:10",
    "ResumeKey": "Victoria",
    "ResumeTimeout": "0:0:30",
    "SelfDeaf": true
  },
  "Rss": {
    "Interval": "1:0:0",
    "Feeds": []
  }
}
```

### Docker

Der Bot kann als Docker Container gestartet werden. Die appsettings.json muss au�erhalb der Containers bereitgestellt werden. Im Anschluss kann der Container gestartet werden.

`docker run --name RcDiscordBot -v HOSTPATH:/app/appsettings.json rcdicordapp:latest`

# Befehle

Der Prefix f�r die Befehle kann beliebig unter 'appsettings.json' angepasst werden. Diese Anleitung geht vom StandardPrefix `'!'` aus.

- `!help` => Verf�gbare Befehle anzeigen
- `!help {Command}` => {Command} mit dem Befehl des zu erkl�renden Befehls ersetzen
- `!join` => Den Bot zum aktullen Sprachchannel hinzuf�gen
- `!ListStream` => Gespeicherte Streams anzeigen
- `!play` => Sucht musik von einer URL ab oder sucht in Youtube
- `!sc` => Sucht musik von einer URL ab oder sucht in SoundCloud
- `!stream streamName` => Spielt einen gespeicherten Stream ab
- `!Stop` => Beendet den Stream und entfernt den Bot aus dem Channel