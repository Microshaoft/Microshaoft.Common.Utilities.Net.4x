dotnet build Microshaoft.Common.Utilities.Net.sln -c Debug

mkdir -p Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/wwwroot/
cp -rf Samples/MsSqlCodeDiffVersioning/MsSqlCodeDiffVersioning.Shared/wwwroot/* Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/wwwroot/

mkdir -p Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/RoutesConfig/
cp -rf Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/RoutesConfig/* Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/RoutesConfig/

mkdir -p Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MsSQL/MsSQL.Plugin.NET.Standard.2.x/bin/Debug/netstandard2.1/*Plugin*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Plugin*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/JsonParametersValidators/SamplePlugin/SamplePlugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Plugin* Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/

cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*MySql*   Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Npgsql*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Sqlite*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*SQLite*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Oracle*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*DB2*     Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/Plugins/

cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*MySql*   Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Npgsql*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Sqlite*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*SQLite*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*Oracle*  Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
cp Plugins/StoreProceduresExecutors/MySQL/MySQL.Plugin.NET.Core.3.x/bin/Debug/netcoreapp3.1/*DB2*     Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/

cd Samples/MsSqlCodeDiffVersioning/ASPNETCore.3x/bin/Debug/netcoreapp3.1/
dotnet MsSqlCodeDiffVersioning.3x.dll $1
