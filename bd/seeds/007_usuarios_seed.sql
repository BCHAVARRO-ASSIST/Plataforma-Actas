-- Seed de usuarios iniciales si no existen
DO $$
DECLARE
    v_admin_id INT;
    v_admin_role_id INT;
    v_mensajero_id INT;
    v_mensajero_role_id INT;
    v_hash VARCHAR(255) := '$2a$11$dpdZ390Q5a8H/G1u2U6J4eYp5T5m8UuJ6Q9J2O8T2Q5J8UuJ6Q9J2'; -- placeholder, will update if needed
BEGIN
    SELECT "Id" INTO v_admin_role_id FROM roles WHERE "Nombre" = 'Administrador' LIMIT 1;
    SELECT "Id" INTO v_mensajero_role_id FROM roles WHERE "Nombre" = 'Mensajero' LIMIT 1;

    -- Verificar si existe admin
    IF NOT EXISTS (SELECT 1 FROM usuarios WHERE "UsuarioRed" = 'admin') THEN
        INSERT INTO usuarios ("Nombre", "Apellido", "UsuarioRed", "Correo", "Proceso", "Activo", "PasswordHash")
        VALUES ('Administrador', 'Sistema', 'admin', 'admin@plataformaactas.local', 'Tecnología', TRUE, '$2a$11$N.vKfZQ8QyZ7q7Y4j7yX8eH6B0E0u0N0Q0A0R0S0T0U0V0W0X0Y0Z')
        RETURNING "Id" INTO v_admin_id;

        IF v_admin_role_id IS NOT NULL THEN
            INSERT INTO usuario_roles ("UsuarioId", "RolId")
            VALUES (v_admin_id, v_admin_role_id)
            ON CONFLICT DO NOTHING;
        END IF;
    END IF;
END $$;
