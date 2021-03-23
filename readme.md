# Discord Bot

Funktionen
- Audio Streams abspielen 
- Audi Streams k�nnen fest hinterlegt werden, sodass nur der Name zum apsielen ben�tigt wird
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
und die `RCDiscordBot.service` (befindet sich ebenfalls im Archive) nach `/etc/systemd/system/` verschieben. Falls ein anderer Ordner verwendet wird, muss der Pfad noch angepasst werden.

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

# Befehle

Der Prefix f�r die Befehle kann beliebig unter 'appsettings.json' angepasst werden. Diese Anleitung geht vom StandardPrefix `'!'` aus.

- `!help` => Verf�gbare Befehle anzeigen
- `!help {Command}` => {Command} mit dem Befehl des zu erkl�renden Befehls ersetzen
- `!join` => Den Bot zum aktullen Sprachchannel hinzuf�gen
- `!ListStream` => Gespeicherte Streams anzeigen
- `!PlayStream {NameOderAudioStreamUrl}` => Spielt einen gespeicherten Stream bzw. den Stream von einer URL ab
- `!Stop` => Beendet den Stream und entfernt den Bot aus dem Channel