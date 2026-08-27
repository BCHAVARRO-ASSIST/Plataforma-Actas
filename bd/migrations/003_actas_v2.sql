CREATE TABLE IF NOT EXISTS actas_v2
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    numero_acta VARCHAR(50),

    equipment_id UUID NOT NULL,

    usuario_id INTEGER NOT NULL,

    destinatario_usuario_id INTEGER,

    mensajero_usuario_id INTEGER,

    acta_entregada BOOLEAN NOT NULL DEFAULT FALSE,

    estado VARCHAR(50) NOT NULL DEFAULT 'Borrador',

    tipo_acta VARCHAR(20) NOT NULL DEFAULT 'Entrega',

    acta_origen_id UUID,

    ruta_documento VARCHAR(500),

    fecha_firma TIMESTAMPTZ,

    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    fecha_anulacion TIMESTAMPTZ,

    fecha_finalizacion TIMESTAMPTZ,

    device_id VARCHAR(200),

    form_data_json JSONB NOT NULL DEFAULT '{}'::jsonb,

    hash_sha256 VARCHAR(128),

    motivo_anulacion VARCHAR(2000),

    signature_data_json JSONB,

    CONSTRAINT fk_actas_v2_acta_origen
        FOREIGN KEY (acta_origen_id)
        REFERENCES actas_v2(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_actas_v2_equipment
        FOREIGN KEY (equipment_id)
        REFERENCES equipment(id)
        ON DELETE RESTRICT,

    CONSTRAINT fk_actas_v2_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES usuarios("Id")
        ON DELETE RESTRICT,

    CONSTRAINT fk_actas_v2_destinatario
        FOREIGN KEY (destinatario_usuario_id)
        REFERENCES usuarios("Id")
        ON DELETE RESTRICT,

    CONSTRAINT fk_actas_v2_mensajero
        FOREIGN KEY (mensajero_usuario_id)
        REFERENCES usuarios("Id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_actas_v2_numero_acta
    ON actas_v2(numero_acta);

CREATE INDEX IF NOT EXISTS ix_actas_v2_tipo_acta
    ON actas_v2(tipo_acta);

CREATE INDEX IF NOT EXISTS ix_actas_v2_acta_origen_id
    ON actas_v2(acta_origen_id);


CREATE INDEX IF NOT EXISTS ix_actas_v2_equipment_id
    ON actas_v2(equipment_id);

CREATE INDEX IF NOT EXISTS ix_actas_v2_usuario_id
    ON actas_v2(usuario_id);

CREATE INDEX IF NOT EXISTS ix_actas_v2_destinatario_usuario_id
    ON actas_v2(destinatario_usuario_id);

CREATE INDEX IF NOT EXISTS ix_actas_v2_mensajero_usuario_id
    ON actas_v2(mensajero_usuario_id);

CREATE INDEX IF NOT EXISTS ix_actas_v2_estado
    ON actas_v2(estado);


CREATE TABLE IF NOT EXISTS acta_observations_v2
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    acta_id UUID NOT NULL,

    texto VARCHAR(4000) NOT NULL,

    usuario_id INTEGER,

    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_acta_observations_v2_acta
        FOREIGN KEY (acta_id)
        REFERENCES actas_v2(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_acta_observations_v2_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES usuarios("Id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_acta_observations_v2_acta_id
    ON acta_observations_v2(acta_id);

CREATE INDEX IF NOT EXISTS ix_acta_observations_v2_usuario_id
    ON acta_observations_v2(usuario_id);


CREATE TABLE IF NOT EXISTS acta_signatures_v2
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    acta_id UUID NOT NULL,

    signature_base64 TEXT NOT NULL,

    consent_accepted BOOLEAN NOT NULL,

    legal_clause_version VARCHAR(50),

    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_acta_signatures_v2_acta
        FOREIGN KEY (acta_id)
        REFERENCES actas_v2(id)
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_acta_signatures_v2_acta_id
    ON acta_signatures_v2(acta_id);


CREATE TABLE IF NOT EXISTS acta_status_history_v2
(
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    acta_id UUID NOT NULL,

    estado_anterior VARCHAR(50) NOT NULL,

    estado_nuevo VARCHAR(50) NOT NULL,

    usuario_id INTEGER,

    comentario VARCHAR(1000),

    fecha_creacion TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_acta_status_history_v2_acta
        FOREIGN KEY (acta_id)
        REFERENCES actas_v2(id)
        ON DELETE CASCADE,

    CONSTRAINT fk_acta_status_history_v2_usuario
        FOREIGN KEY (usuario_id)
        REFERENCES usuarios("Id")
        ON DELETE RESTRICT
);

CREATE INDEX IF NOT EXISTS ix_acta_status_history_v2_acta_id
    ON acta_status_history_v2(acta_id);

CREATE INDEX IF NOT EXISTS ix_acta_status_history_v2_usuario_id
    ON acta_status_history_v2(usuario_id);
