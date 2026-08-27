# Base de datos

- `init`: scripts para crear una base nueva.
- `migrations`: cambios incrementales y versionados. Nunca se modifica una migracion ya aplicada.
- `seeds`: datos semilla para desarrollo o pruebas.
- `backups`: copias locales; no se versionan en Git.

Las migraciones usan un prefijo unico y ascendente (`001_`, `002_`, etc.). Antes de aplicar una migracion en un ambiente compartido, realiza un respaldo y registra la version aplicada.
