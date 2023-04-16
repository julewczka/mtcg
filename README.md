# SWE Monster Trading Card Game 
Semester-Projekt an der FH-Technikum-Wien

## Datenbank Setup

Als Datenbank für dieses Projekt wird eine PostgreSQL Datenbank verwendet. PostgreSQL wird über ein offizielles Docker-Image in einem Container bereitgestellt.
Dadurch ist es nicht notwendig, PostgreSql selbst zu installieren.
Um den Docker-Container zu erstellen:
```
docker run --name mtcg-container -e POSTGRES_USER=mtcg-user -e POSTGRES_PASSWORD=mtcg-pw -e POSTGRES_DB=mtcg-db -p 5432:5432 -d postgres
```

Der Container ist mit diesem Befehl erstellt und kann mit folgendem gestartet werden:

```
docker start mtcg-container
```

Läuft der Container auf der Host-Maschine, kann dieser über localhost:5432 angesprochen werden. Wird der Container extern gehosted, muss der Container über die IP-Adresse des externen Rechners angesprochen werden.

Credentials:
* **Datenbank Technologie**: PostgreSQL
* **User**: mtcg-user
* **Passwort**: mtcg-pw
* **Datenbank**: mtcg-db

Im Ordner `{root-project}/scripts` befindet sich die mtcg-creates.sql um alle Datenbank-Tabellen zu erstellen. In dieser Datei kann man die Tabellen auch wieder löschen, sofern es benötigt werden sollte. Falls benötigt, kann auch die mtcg-inserts.sql ausgeführt werden, um c.a. 100 Dummy-User zu erstellen.


## Erstellen einer ausführbaren Datei unter Linux Mint 20

Um die Server-Applikation ausführbar zu machen, wird ein sogenanntes binary file benötigt. Dazu muss das Projekt bzw. der Quellcode von .NET mit folgendem Befehl kompiliert werden:

```
dotnet publish -r linux-x64 --self-contained false -o bin/executable/
```

.NET baut dieses Projekt nun als framework-dependent ohne der .NET Laufzeitumgebung. Das bedeutet, dass jeder, der diese App ausführen möchte, die .NET Core Laufzeitumgebung auf seiner lokalen Maschine installiert haben muss.
Durch diese Art wird eine plattformunabhängige binary als dll erzeugt sowie eine plattform-spezifische ausführbare Datei (executable). Die dll Datei ist plattformunabhängig, die executable nicht. Unter Linux wird die App nach dem Projektnamen (in diesem Fall mtcg) benannt.

Durch den Parameter **-o** spezifizieren wir, in welchen Ordner der **Release-Build** erzeugt wird. Der `--self-contained` Flag spezifiziert, ob wir die .NET Core Library und Laufzeitumgebung ebenfalls in den Release-Build hinzufügen.
Durch die Setzung auf **false** wird der Deployment kleiner (um ca. 150MB) und die App wird plattformunabhängig.
 
Um die Server-App dann zu starten, muss man ins Verzeichnis der ausführbaren Datei. Die wurde auf `{root-project}/bin/executable` gesetzt. Anschließend startet man die App mit diesen Befehl:

```
dotnet run mtcg –project ~/path/to/your/project

or

dotnet run mtcg.dll –project ~/path/to/your/project
```
