@ECHO OFF

SET Arbor.Build.Vcs.Branch.Name=%GITHUB_REF%
SET Arbor.Build.BuilderNumber.UnixEpochSecondsEnabled=true

SET ContinuousIntegrationBuild=true

call dotnet arbor-build

EXIT /B %ERRORLEVEL%