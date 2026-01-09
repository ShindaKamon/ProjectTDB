@echo off
REM Script pour nettoyer le cache Unity et résoudre les problèmes d'import
REM À exécuter QUAND UNITY EST FERMÉ

echo ====================================
echo Nettoyage du cache Unity
echo ====================================
echo.
echo ATTENTION: Assurez-vous que Unity est bien fermé avant de continuer!
echo.
pause

echo.
echo Suppression de ArtifactDB...
if exist "Library\ArtifactDB" del /F /Q "Library\ArtifactDB"
if exist "Library\ArtifactDB-lock" del /F /Q "Library\ArtifactDB-lock"

echo Suppression du dossier Artifacts...
if exist "Library\Artifacts" rmdir /S /Q "Library\Artifacts"

echo Suppression du dossier TempArtifacts...
if exist "Library\TempArtifacts" rmdir /S /Q "Library\TempArtifacts"

echo Suppression du dossier ScriptAssemblies...
if exist "Library\ScriptAssemblies" rmdir /S /Q "Library\ScriptAssemblies"

echo.
echo ====================================
echo Nettoyage terminé!
echo ====================================
echo.
echo Vous pouvez maintenant rouvrir Unity.
echo Unity va reconstruire le cache automatiquement.
echo.
pause
