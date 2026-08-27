BEGIN;

ALTER TABLE public.usuarios
ADD COLUMN IF NOT EXISTS "PasswordHash"
    varchar(255);

ALTER TABLE public.usuarios
ADD COLUMN IF NOT EXISTS "UltimoAcceso"
    timestamp with time zone;

CREATE INDEX IF NOT EXISTS "IX_usuarios_UsuarioRed"
ON public.usuarios ("UsuarioRed");

CREATE INDEX IF NOT EXISTS "IX_usuarios_Correo"
ON public.usuarios ("Correo");

COMMIT;