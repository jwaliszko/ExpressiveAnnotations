#!/bin/bash

if [ $# -eq 0 ]; then
    echo "product version required"
    exit 1
fi

rm -fr $1

if [ ! -f nuget.exe ]; then
    echo "------ nuget.exe downloading..."
    wget -nv 'http://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
fi

echo "------ package restoration..."
./nuget.exe restore ExpressiveAnnotations.sln

echo "------ release net40 config compilation..."
MSBuild.exe ExpressiveAnnotations.sln /t:Build /p:Configuration=Release-Net40
echo "------ release net45 config compilation..."
MSBuild.exe ExpressiveAnnotations.sln /t:Build /p:Configuration=Release-Net45

echo "------ script processing..."
sed '/for testing only/,/----------------/{N;d;}' expressive.annotations.validate.js > bin/expressive.annotations.validate.js

echo "------ core dll version extraction..."
core_ver=`strings ./bin/Release-Net40/ExpressiveAnnotations.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' | cut -f1,2,3 -d"."`

echo "------ packages building..."
./nuget.exe pack ExpressiveAnnotations.nuspec -version $1 -symbols
./nuget.exe pack ExpressiveAnnotations.dll.nuspec -version $core_ver -symbols

echo "------ archive creation..."
mkdir -p $1/ExpressiveAnnotations
cp bin/{Release-NET45/ExpressiveAnnotations*,expressive.annotations.validate.js} $1/ExpressiveAnnotations
mv bin/* *.nupkg $1
cd $1 && zip ExpressiveAnnotations.zip ExpressiveAnnotations/*

#nuget setapikey ...
#nuget push ...

echo "------ done."
