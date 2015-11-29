#!/bin/bash

if [ $# -eq 0 ]; then
    echo "product version required"
    exit 1
fi

echo "------ cleanup..."
rm -fr $1
rm -fr ../src/bin

if [ ! -f nuget.exe ]; then
    echo "------ nuget.exe downloading..."
    wget -nv 'http://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
fi

echo "------ package restoration..."
./nuget.exe restore ExpressiveAnnotations.sln

echo "------ release config compilation..."
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Build /p:Configuration=Release /verbosity:minimal

echo "------ release-net40 config compilation..."
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Build /p:Configuration=Release-Net40 /verbosity:minimal
echo "------ release-net45 config compilation..."
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Build /p:Configuration=Release-Net45 /verbosity:minimal

echo "------ generating documentation..."
MSBuild.exe ../doc/api/api.shfbproj /t:Build /p:Configuration=Release

echo "------ cutting of debug section from script..."
sed '/!debug section enter/,/!debug section leave/{N;d;}' ../src/expressive.annotations.validate.js > expressive.annotations.validate.js

echo "------ generating minified script version..."
uglifyjs --compress --mangle --comments /Copyright/ --output expressive.annotations.validate.min.js -- expressive.annotations.validate.js
cp expressive.annotations.validate.min.js ../src/expressive.annotations.validate.min.js

echo "------ core dll version extraction..."
core_ver=`strings ../src/bin/Release-Net40/ExpressiveAnnotations.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' | cut -f1,2,3 -d"."`

echo "------ packages building..."
./nuget.exe pack ExpressiveAnnotations.nuspec -version $1 -symbols
./nuget.exe pack ExpressiveAnnotations.dll.nuspec -version $core_ver -symbols

echo "------ archive creation..."
mkdir -p $1/ExpressiveAnnotations{/net40,/net45}
mv ../src/bin/Release-Net40/ExpressiveAnnotations* $1/ExpressiveAnnotations/net40
mv ../src/bin/Release-Net45/ExpressiveAnnotations* $1/ExpressiveAnnotations/net45
mv expressive.annotations.validate* $1/ExpressiveAnnotations
mv *.nupkg $1
cp ../doc/api/api.chm $1/ExpressiveAnnotations
cd $1 && zip ExpressiveAnnotations.zip ExpressiveAnnotations/*

echo "------ cleanup..."
cd ..
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Clean /p:Configuration=Release /verbosity:quiet
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Clean /p:Configuration=Release-Net40 /verbosity:quiet
MSBuild.exe ../src/ExpressiveAnnotations.sln /t:Clean /p:Configuration=Release-Net45 /verbosity:quiet

#nuget setapikey ...
#nuget push ...

echo "------ done."
