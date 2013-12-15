$Archs = {"Portable-Net45+WinRT45+WP8", "Xamarin.Android", "Xamarin.iOS"}

if (Test-Path .\Release) {
    rmdir -r -force .\Release
}

foreach-object $Archs | %{mkdir -Path ".\Release\$_"}

cp .\Lager\bin\Release\Portable\Lager.dll .\Release\Portable-Net45+WinRT45+WP8\
cp .\Lager\bin\Release\Portable\Lager.xml .\Release\Portable-Net45+WinRT45+WP8\

cp .\Lager\bin\Release\Android\Lager.dll .\Release\Xamarin.Android\
cp .\Lager\bin\Release\Android\Lager.xml .\Release\Xamarin.Android\
cp .\Lager.Android\bin\Release\Lager.Android.dll .\Release\Xamarin.Android\
cp .\Lager.Android\bin\Release\Lager.Android.xml .\Release\Xamarin.Android\
cp .\ext\Monoandroid\*.* .\Release\Xamarin.Android\

cp .\Lager\bin\Release\iOS\Lager.dll .\Release\Xamarin.iOS\
cp .\Lager\bin\Release\iOS\Lager.xml .\Release\Xamarin.iOS\
cp .\ext\Monotouch\*.* .\Release\Xamarin.iOS\

