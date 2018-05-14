rm -r build
dotnet clean
cd DeliveryTrackerWeb
dotnet publish -c Release -r linux-x64 --self-contained 
cd ..
cd DeliveryTrackerScheduler
dotnet publish -c Release -r linux-x64 --self-contained
cd ..
mkdir -p build
cp -R DeliveryTrackerWeb/bin/Release/netcoreapp2.0/linux-x64/publish build/web
cp -R DeliveryTrackerScheduler/bin/Release/netcoreapp2.0/linux-x64/publish build/scheduler
