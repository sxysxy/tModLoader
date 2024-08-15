#!/usr/bin/env sh

if ! command -v git &> /dev/null
then
    echo "git could not be found on the PATH, please install git."
    exit
else
    echo "git found..."
fi

if ! command -v dotnet &> /dev/null
then
    echo "dotnet could not be found on the PATH, please install dotnet core."
    exit
else
    echo "dotnet found..."
fi

build() {
    echo "New commits detected since last run, recompiling the binary..."
    echo "building setup/CLISetup/Cli/CliSetup.csproj..."
    dotnet build ./setup/CLISetup/Cli/CliSetup.csproj
    echo $cur > .gitbuffer
}

fb=false

if [[ $1 == "--force-build" ]]
then
    fb=true
    shift
fi

if ! [ -r ".gitbuffer" ] || $fb
then
    build
else
    buffer=`cat .gitbuffer`
    cur=`git rev-parse --short HEAD`
    if [[ $cur == $buffer ]]
    then
        echo "No new commits detected since last run"
    else
        build
    fi
fi

./setup/CLISetup/Cli/bin/Debug/net6.0/CliSetup $@
