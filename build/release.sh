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
msbuild="/cygdrive/c/Program Files (x86)/Msbuild/14.0/bin/MSBuild.exe"
msbuildVersion=14.0
visualStudioVersion=14.0
targets=( v4.0 v4.5 )

rm -fr $productVersion net* *.js

echo -e "\n------ package restoration..."
./nuget.exe restore ../src/ExpressiveAnnotations.sln -MSBuildVersion $msbuildVersion -verbosity quiet

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
coreVersion=`strings ../src/ExpressiveAnnotations/bin/Release/ExpressiveAnnotations.dll | egrep '^[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+$' | cut -f1,2,3 -d"."`
./nuget.exe pack ExpressiveAnnotations.nuspec -version $productVersion -symbols -MSBuildVersion $msbuildVersion
./nuget.exe pack ExpressiveAnnotations.dll.nuspec -version $coreVersion -symbols -MSBuildVersion $msbuildVersion

echo -e "\n------ archive creation..."
mkdir -p $productVersion/ExpressiveAnnotations
mv net* $productVersion/ExpressiveAnnotations
mv expressive.annotations.validate* $productVersion/ExpressiveAnnotations
mv *.nupkg $productVersion
cp ../doc/api/api.chm $productVersion/ExpressiveAnnotations
cd $productVersion && zip -r ExpressiveAnnotations.zip ExpressiveAnnotations
cd ..

# nuget setapikey ...
# nuget push ExpressiveAnnotations.x.x.x.nupkg
