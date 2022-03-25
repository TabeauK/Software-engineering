#!/usr/bin/bash

PLAYER_NUM=2 # liczba uruchomionych agentow dla jednej druzyny
DOTNET="dotnet"
BUILD_MODE="Release"
WIN_CMD="start"

run_gm=true
run_gui=false
run_cs=true
build_solution=true

verbose=false
GM_PORT=3729
PLAYER_PORT=5000
PLAYER_HOST=localhost
RED_NUM=
BLUE_NUM=

usage() 
{
	echo "usage: ./run-game.sh [-h][-g][-v][-n <players>][-r <players>][-b <players>]
            [-B][-G][-C][-P <port>][-H <hostname>]
Skrypt uruchamia CS, GM i okreslona liczbę Playerow (w tej kolejnosci).
Każdy program powinien uruchomic sie w oddzielnym oknie.
Na windowsie uruchom przez Git Bash, ktory instaluje sie wraz z Git for Windows.


-h              Wypisz te wiadomosc
-g              Uruchom GUI
-v              Tryb verbose

-n <players>    Liczbe graczy dla obu druzyn (domyslnie: ${PLAYER_NUM})
-r <players>    Liczbe graczy w druznie Red (nadpisuje -n) 
-b <players>    Liczbe graczy w druznie Blue (nadpisuje -n)

-m <mode>       Konfiguracja (wspierane wartosci: Debug, Release, domyslnie ${BUILD_MODE})
-B              Nie buduj rozwiazania

-G              Nie uruchamiaj GM
-C              Nie uruchamiaj CS

-P <port>       Port Playera (domyslnie: ${PLAYER_PORT})
-H <hostname>   Host Playera (domyslnie: ${PLAYER_HOST})
"
	exit
}

check_player_num()
{
	[[ ! "$1" =~ ^[0-9]+$ ]] && {
		echo "$1 to niepoprawna liczba graczy"
		exit 1
	}
}

# Wczytaj argumenty
while getopts ":hgn:BGCm:P:H:r:b:v" o
do
	case "$o" in
		h)
			usage
			;;
		g)
			run_gui=true
			;;
		n)
			PLAYER_NUM="$OPTARG"
            check_player_num "$PLAYER_NUM"
			;;
        r)
            RED_NUM="$OPTARG"
            check_player_num "$RED_NUM"
            ;;
        b)
            BLUE_NUM="$OPTARG"
            check_player_num "$BLUE_NUM"
            ;;
		B)
			build_solution=false
			;;
		G)
			run_gm=false
			;;
		C)
			run_cs=false
			;;
        v)
            verbose=true
            ;;
		m)
			BUILD_MODE="$OPTARG"
			[[ ! "$BUILD_MODE" =~ Release|Debug ]] && {
				echo "$BUILD_MODE to niepoprawna konfiguracja"
				exit 1
			}
			;;
		P)
			PLAYER_PORT="$OPTARG"
			[[ ! "$PLAYER_PORT" =~ ^[1-9][0-9]*$ ]] && {
				echo "$PLAYER_PORT to niepoprawny numer portu"
				exit 1
			}
			;;
        H)
            PLAYER_HOST="$OPTARG"
            ;;
		:)
			echo "Opcja $OPTARG wymaga argumentu"
			usage
			;;
		*)
			echo "Niepoprwana opcja $OPTARG"
			usage
			;;
	esac
done

# Jesli nie podano liczby graczy danego teamu domyslnie to PLAYER_NUM
[[ -z "$RED_NUM" ]] && RED_NUM="$PLAYER_NUM"
[[ -z "$BLUE_NUM" ]] && BLUE_NUM="$PLAYER_NUM"

# Sciezki do dll
BIN_PREFIX="bin/$BUILD_MODE/netcoreapp2.1"

CS=CommunicationServer
CS_BIN="$BIN_PREFIX/CommunicationServer.dll"

GM=GameMaster
GM_BIN="$BIN_PREFIX/GameMaster.dll"

GM_GUI=GameMaster.GUI
# GUI tez uruchamiane jest z folderu GameMaster/
GM_GUI_BIN="../$GM_GUI/$BIN_PREFIX/GameMaster.GUI.dll"

PLAYER=Player
PLAYER_BIN="$BIN_PREFIX/Player.dll"

[[ "$run_gui" == true ]] && {
	GM_BIN="$GM_GUI_BIN"
}

# Okresl sposob uruchamiania, w zaleznosci od systemu
if [[ -z "$MSYSTEM" ]]
then
	if [[ -z "$TERMINAL" ]]
	then
		MY_TERM="xterm"
	else
		MY_TERM="$TERMINAL"
	fi
	EXEC_OPT=-e
else
	DOTNET="dotnet.exe"
	MY_TERM="$WIN_CMD"
	EXEC_OPT=
	[[ ! -x "$(command -v $MY_TERM)" ]] && echo "$MY_TERM: my_term not found" && exit 1
fi

# Sprawdz obecnosc polecenia dotnet
[[ ! -x "$(command -v $DOTNET)" ]] && echo "$DOTNET: dotnet not found" && exit 1

if [[ "$build_solution" = true ]]
then
	# Zbuduj rozwiazanie
	"$DOTNET" build --configuration "$BUILD_MODE"
	[[ "$?" -ne 0 ]] && echo "build failed" && exit 1
fi

# Uruchom CS
[[ "$run_cs" == true ]] && {
	(
    cd "$CS"
	"$MY_TERM" "$EXEC_OPT" "$DOTNET" "$CS_BIN" -- "$GM_PORT" "$PLAYER_PORT" "$verbose" &
	)
	sleep 1
}

# Uruchom GM
[[ "$run_gm" == true ]] && {
	(
    cd "$GM"
	"$MY_TERM" "$EXEC_OPT" "$DOTNET" "$GM_BIN" &
	)
	sleep 3
}

# Uruchom Playerow
(
cd "$PLAYER"

while [[ "$RED_NUM" -gt 0 ]] || [[ "$BLUE_NUM" -gt 0 ]]
do
    [[ "$RED_NUM" -gt 0 ]] && {
        ((RED_NUM--))
	    "$MY_TERM" "$EXEC_OPT" "$DOTNET" "$PLAYER_BIN" -- "$PLAYER_HOST" "$PLAYER_PORT" Red $verobse &
    } 

    [[ "$BLUE_NUM" -gt 0 ]] && {
        ((BLUE_NUM--))
	    "$MY_TERM" "$EXEC_OPT" "$DOTNET" "$PLAYER_BIN" -- "$PLAYER_HOST" "$PLAYER_PORT" Blue $verbose &
    }
done
)

