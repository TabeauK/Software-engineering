# Inżynieria oprogramowania (Software engineering)

## The Project Game

### Krzysztof Tabeau -- Krzysztof Woźniak -- Kamil Todorowski -- Paweł Szymkiewicz -- Aleksander Wiśniewski

* * *

#### The Project Game jest projektem wykonywanym przez pięcioosobowy zespół w ramach przedmiotu Inżynieria Oprogramowania II.

#### [Specyfikacja projektu](https://github.com/MINI-IO/IO-project-game) została wykonana przez: **Alicję Moskal, Bartka Chrostowskiego, Emila Dragańczuka, Jakuba Drak Sbahi i Mikołaja Molendę**.

#### W ramach przedmiotu Inżynieria oprogramowania I również została przygotowana dokumentacja z naszej strony, która nie została ostatecznie użyta ze względu na zachowanie spójności między grupami na zajęciach. Naszą wersję można znaleść w pliku Documentation.html

* * *

## Metodyka

Nasz projekt został wykonany zgodnie ze zwinną metodyką **SCRUM**. Podział na role w ramach grupy wyglądał następująco:

Scrum Master: **Paweł Szymkiewicz**

Product Owner: **Aleksander Wiśniewski**

Programiści i testerzy: **cały zespół**

Oznacza to, że każdy z członków będzie pisał kod i testy. Product Owner odpowiedzialny był za sprawdzanie, czy odpowiednio realizowane były założenia biznesowe,
oraz czuwanie nad taskami. Scrum Master był odpowiedzialny za sprawdzanie, czy metodyka jest odpowiednio realizowana. 

Nasza realizacja metodyki nie zakładała tworzenia historyjek użytkowników. Założyliśmy, że te wynikają bezpośrednio z wybranej specyfikacji. Jedynym backlogiem z którego korzystaliśmy był sprint backlog, tj. zbiór tasków do zadanego sprintu.

Ogólne wytyczne czasowe projektu były reprezentowane przez epiki, które zawierajały większy zakres funkcjonalności. Każdy epik był związany z pewnym deadlinem projektu.

W ramach metodyki były realizowane sprinty o zmiennej długości. Przed każdym sprintem były wybierane zadania z backlogu, ustalane kto wykona które zadanie,
a także jaki jest szacowany czas potrzebny na wykonanie danego zadania. Nasza implementacja metodyki **nie zakładała daily stand-upów**. Praca była głównie indywidualna,
tj. nie był planowany pair programming, ani w ogólności spotykanie się na wspólne pisanie. Za to była zachowana ciągła komunikowacja w przypadku wszelkich niepewności 
(Slack), a kod był często integrowany aby zachować spójność pracy wszystkich członków.

Po zakończeniu sprintu i dostarczeniu konkretnych funkcjonalności i działającego kodu, organizowany był sprint review gdzie podsumowywaliśmy pracę. Przeanalizowywaliśmy też
backlog, dodając, usuwając lub modyfikując zawartość w zależności od zmian potrzeb.

Do zarządzania backlogiem, taskami, pracą całej drużyny w ramach sprintu będzie wykorzystywana [JIRA](https://io2020.atlassian.net/).

* * *

## Technologie

Projekt został napisany w języku C#, w technologii .NET Core 2.1. Interfejs użytkownika został wykonany w AvaloniaUI.

* * *

## Harmonogram projektu

Lab 3: Deadline: Initiation phase

Lab 6: Deadline: Game

Lab 10/11: Deadline: Communication

Lab 13: Deadline: Cooperation

* * *

## Model branchowania

W projekcie wykorzystywaliśmy 3 branche: **master**, **develop** i **feature**. W masterze znajdowały się kolejne wersje aplikacji realizujące pewne epici
(tj. spełniające wymagania kolejnych deadline'ów). Develop odchodzi od mastera i jest to gałąź na którą były regularnie dodawane nowe funkcjonalności. Każda funkcjonalność
była tworzona w gałęzi feature odchodzącej od develop. Założenie było takie, że feature'y będą niewielkie i będą często mergowane z develop (przykładowo przynajmniej
raz dziennie). Pod koniec sprinta, przed deadlinem, develop mergowany był z masterem.

## Uruchomienie 
Aby uruchomić grę należy uruchomić plik run-game.sh. Użycie pliku można sprawidzić za pomocą
```
bash ./run-game.sh -h
```