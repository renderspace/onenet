rem del .\bin\nuget\*.nupkg
rem .nuget\NuGet.exe pack -Build -OutputDirectory .\bin\nuget\  -Prop Configuration=Release -IncludeReferencedProjects -Verbosity detailed
.nuget\NuGet.exe push .\*.nupkg DC838B29422CB7C5E357686CEE165 -Source http://nuget.t.renderspace.net