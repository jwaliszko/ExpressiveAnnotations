#!/bin/bash

if [ $# -ne 2 ]; then
    echo "usage: release.sh product-version keyfile-path"
    exit 1
fi

if [ ! -f nuget.exe ]; then
    echo -e "\n------ nuget.exe downloading..."
    wget -nv 'http://dist.nuget.org/win-x86-commandline/latest/nuget.exe'
fi

productVersion=$1
keyfile=$2
msbuild="/cygdrive/c/Program Files (x86)/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/MSBuild.exe"
visualStudioVersion=15.0
targets=( v4.0 v4.5 )

rm -fr $productVersion

echo -e "\n------ package restoration..."
./nuget.exe restore ../src/ExpressiveAnnotations.sln -verbosity quiet

if [ $? -ne 0 ]; then
    exit 1
fi

for target in "${targets[@]}"
do
    const="NET"`echo $target | sed 's/[.v]//g'`

    echo -e "\n------ release $const compilation..."
    "$msbuild" /nologo ../src/ExpressiveAnnotations.MvcUnobtrusive/ExpressiveAnnotations.MvcUnobtrusive.csproj "/t:Clean;Rebuild" /p:BuildProjectReferences=true /p:Configuration=Release /p:PlatformTarget=AnyCPU /p:AssemblyOriginatorKeyFile=$keyfile /p:TreatWarningsAsErrors=true /p:TargetFrameworkVersion=$target "/p:DefineConstants=$const SIGNED" /p:RunCodeAnalysis=False /p:SignAssembly=True /p:VisualStudioVersion=$visualStudioVersion /verbosity:minimal

    if [ $? -ne 0 ]; then
        exit 1
    fi

    dist=`echo $const | tr '[:upper:]' '[:lower:]'`
    mkdir -p $dist
    cp ../src/ExpressiveAnnotations.MvcUnobtrusive/bin/Release/ExpressiveAnnotations* $dist
done

echo -e "\n------ help generation..."
"$msbuild" /nologo ../doc/api/api.shfbproj /t:Build /p:Configuration=Release /verbosity:minimal

if [ $? -ne 0 ]; then
    exit 1
fi

echo -e "\n------ script preparation..."
sed '/!debug section enter/,/!debug section leave/{N;d;}' ../src/expressive.annotations.validate.js > expressive.annotations.validate.js
uglifyjs --compress --mangle --comments /Copyright/ --output expressive.annotations.validate.min.js -- expressive.annotations.validate.js
cp expressive.annotations.validate.min.js ../src/expressive.annotations.validate.min.js

echo -e "\n------ package building..."
coreVersion=`strings ../src/ExpressiveAnnotations/bin/Release/ExpressiveAnnotations.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+' | cut -f1,2,3 -d"."`
./nuget.exe pack ExpressiveAnnotations.nuspec -version $productVersion -symbols
./nuget.exe pack ExpressiveAnnotations.dll.nuspec -version $coreVersion -symbols

scriptVersion=`strings ../src/expressive.annotations.validate.js | egrep -o -m1 '[0-9]+\.[0-9]+\.[0-9]+'`
sed -E 's/[0-9]+\.[0-9]+\.[0-9]+-pre/'$scriptVersion'/' ../bower.json > bower.json
sed -E 's/[0-9]+\.[0-9]+\.[0-9]+-pre/'$scriptVersion'/' ../package.json > package.json

unobVersion=`strings ../src/ExpressiveAnnotations.MvcUnobtrusive/bin/Release/ExpressiveAnnotations.MvcUnobtrusive.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+' | cut -f1,2,3 -d"."`

echo "This product version contains following components:
ExpressiveAnnotations.dll v"$coreVersion", ExpressiveAnnotations.MvcUnobtrusive.dll v"$unobVersion", expressive.annotations.validate.js v"$scriptVersion"" > readme.txt
unix2dos readme.txt

echo -e "\n------ archive creation..."
mkdir -p $productVersion/dist
mv net* $productVersion/dist
mv expressive.annotations.validate* $productVersion/dist
mv *.txt $productVersion/dist
cp ../doc/api/api.chm $productVersion/dist
mv *.nupkg $productVersion
mv *.json $productVersion
cd $productVersion
mv dist ExpressiveAnnotations
zip -r ExpressiveAnnotations.zip ExpressiveAnnotations
mv ExpressiveAnnotations dist
cd ..

# nuget setapikey ...
# nuget push ExpressiveAnnotations.x.x.x.nupkg
