#!/bin/bash

if [ $# -ne 2 ]; then
    echo "usage: release.sh product-version keyfile-path"
    exit 1
fi

echo -e "\n------ vacuum..."
rm -fr $1 net40 net45

if [ ! -f nuget.exe ]; then
    echo "\n------ nuget.exe downloading..."
    wget -nv 'http://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
fi

echo -e "\n------ package restoration..."
./nuget.exe restore ../src/ExpressiveAnnotations.sln

msbuild="/cygdrive/c/Program Files (x86)/Msbuild/14.0/bin/MSBuild.exe"

echo -e "\n------ release-net40 config compilation..."
"$msbuild" /nologo ../src/ExpressiveAnnotations.MvcUnobtrusive/ExpressiveAnnotations.MvcUnobtrusive.csproj "/t:Clean;Rebuild" /p:BuildProjectReferences=true /p:Configuration=Release /p:AssemblyOriginatorKeyFile=$2 /p:TreatWarningsAsErrors=true /p:TargetFrameworkVersion=v4.0 "/p:DefineConstants=NET40 SIGNED" /p:SignAssembly=true /verbosity:minimal

if [ $? -ne 0 ]; then
    exit 1
fi

mkdir -p net40
cp ../src/ExpressiveAnnotations.MvcUnobtrusive/bin/Release/ExpressiveAnnotations* net40

echo -e "\n------ release-net45 config compilation..."
"$msbuild" /nologo ../src/ExpressiveAnnotations.MvcUnobtrusive/ExpressiveAnnotations.MvcUnobtrusive.csproj "/t:Clean;Rebuild" /p:BuildProjectReferences=true /p:Configuration=Release /p:AssemblyOriginatorKeyFile=$2 /p:TreatWarningsAsErrors=true /p:TargetFrameworkVersion=v4.5 "/p:DefineConstants=NET45 SIGNED" /p:SignAssembly=true /verbosity:minimal

if [ $? -ne 0 ]; then
    exit 1
fi

mkdir -p net45
cp ../src/ExpressiveAnnotations.MvcUnobtrusive/bin/Release/ExpressiveAnnotations* net45

echo -e "\n------ help file generation..."
"$msbuild" /nologo ../doc/api/api.shfbproj /t:Build /p:Configuration=Release /verbosity:minimal

if [ $? -ne 0 ]; then
    exit 1
fi

echo -e "\n------ script debug section removal..."
sed '/!debug section enter/,/!debug section leave/{N;d;}' ../src/expressive.annotations.validate.js > expressive.annotations.validate.js

echo -e "\n------ script minification..."
uglifyjs --compress --mangle --comments /Copyright/ --output expressive.annotations.validate.min.js -- expressive.annotations.validate.js
cp expressive.annotations.validate.min.js ../src/expressive.annotations.validate.min.js

echo -e "\n------ core version extraction..."
core_ver=`strings ../src/ExpressiveAnnotations/bin/Release/ExpressiveAnnotations.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' | cut -f1,2,3 -d"."`

echo -e "\n------ package building..."
./nuget.exe pack ExpressiveAnnotations.nuspec -version $1 -symbols
./nuget.exe pack ExpressiveAnnotations.dll.nuspec -version $core_ver -symbols

echo -e "\n------ archive creation..."
mkdir -p $1/ExpressiveAnnotations{/net40,/net45}
mv net40 $1/ExpressiveAnnotations
mv net45 $1/ExpressiveAnnotations
mv expressive.annotations.validate* $1/ExpressiveAnnotations
mv *.nupkg $1
cp ../doc/api/api.chm $1/ExpressiveAnnotations
cd $1 && zip -r ExpressiveAnnotations.zip ExpressiveAnnotations
cd ..

echo -e "\nsuccess"

# nuget setapikey ...
# nuget push ExpressiveAnnotations.x.x.x.nupkg
