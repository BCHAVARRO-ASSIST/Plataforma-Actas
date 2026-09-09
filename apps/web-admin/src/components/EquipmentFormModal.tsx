import { useState, useEffect } from "react";
import type { Equipment, CreateEquipmentRequest, UpdateEquipmentRequest } from "../features/equipment/equipmentService";
import {
  CloseIcon,
  EquipmentIcon,
  CpuIcon,
  UserIcon,
  TagIcon,
  BuildingIcon,
  HardDriveIcon,
  LayersIcon,
  TruckIcon,
  PlusIcon,
  CheckIcon,
} from "./Icons";
import { AutoDismissAlert } from "./AutoDismissAlert";

interface EquipmentFormModalProps {
  isOpen: boolean;
  equipmentToEdit: Equipment | null;
  onClose: () => void;
  onSave: (data: CreateEquipmentRequest | UpdateEquipmentRequest) => Promise<void>;
  saving: boolean;
}

export function EquipmentFormModal({
  isOpen,
  equipmentToEdit,
  onClose,
  onSave,
  saving,
}: EquipmentFormModalProps) {
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape" && isOpen && !saving) {
        onClose();
      }
    }
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, saving, onClose]);

  if (!isOpen) return null;

  return (
    <div className="modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <EquipmentFormContent
        key={equipmentToEdit?.id ?? "new"}
        equipmentToEdit={equipmentToEdit}
        onClose={onClose}
        onSave={onSave}
        saving={saving}
      />
    </div>
  );
}

const STATUS_OPTIONS = [
  { value: "Activo", label: "Activo (Disponible)", dotColor: "var(--color-success-500)" },
  { value: "Asignado", label: "Asignado (En uso)", dotColor: "var(--color-info-500)" },
  { value: "Mantenimiento", label: "En Mantenimiento", dotColor: "var(--color-warning-500)" },
  { value: "Inactivo", label: "Inactivo / Baja", dotColor: "var(--color-slate-400)" },
];

function EquipmentFormContent({
  equipmentToEdit,
  onClose,
  onSave,
  saving,
}: {
  equipmentToEdit: Equipment | null;
  onClose: () => void;
  onSave: (data: CreateEquipmentRequest | UpdateEquipmentRequest) => Promise<void>;
  saving: boolean;
}) {
  const [inventoryKey, setInventoryKey] = useState(equipmentToEdit?.inventoryKey || "");
  const [nombreEquipo, setNombreEquipo] = useState(equipmentToEdit?.nombreEquipo || "");
  const [empresa, setEmpresa] = useState(equipmentToEdit?.empresa || "");
  const [asignadoA, setAsignadoA] = useState(equipmentToEdit?.asignadoA || "");
  const [proveedor, setProveedor] = useState(equipmentToEdit?.proveedor || "");
  const [marca, setMarca] = useState(equipmentToEdit?.marca || "");
  const [procesador, setProcesador] = useState(equipmentToEdit?.procesador || "");
  const [discoDuro, setDiscoDuro] = useState(equipmentToEdit?.discoDuro || "");
  const [memoriaRam, setMemoriaRam] = useState(equipmentToEdit?.memoriaRam || "");
  const [departamento, setDepartamento] = useState(equipmentToEdit?.departamento || "");
  const [estatusEquipo, setEstatusEquipo] = useState(equipmentToEdit?.estatusEquipo || "Activo");
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!inventoryKey.trim()) {
      setError("El código de placa / inventario es obligatorio.");
      return;
    }
    if (!nombreEquipo.trim()) {
      setError("El nombre o modelo del equipo es obligatorio.");
      return;
    }

    try {
      setError(null);
      await onSave({
        inventoryKey: inventoryKey.trim().toUpperCase(),
        nombreEquipo: nombreEquipo.trim(),
        empresa: empresa.trim() || null,
        asignadoA: asignadoA.trim() || null,
        proveedor: proveedor.trim() || null,
        marca: marca.trim() || null,
        procesador: procesador.trim() || null,
        discoDuro: discoDuro.trim() || null,
        memoriaRam: memoriaRam.trim() || null,
        departamento: departamento.trim() || null,
        estatusEquipo: estatusEquipo.trim() || "Activo",
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al guardar el equipo.");
    }
  }

  return (
    <div className="modal-content large" onClick={(e) => e.stopPropagation()}>
      {/* Header Premium */}
      <div className="modal-header">
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          <div className="modal-header-badge">
            <EquipmentIcon size={20} />
          </div>
          <div>
            <h2 style={{ fontSize: "17px", fontWeight: 800, color: "var(--text-primary)", letterSpacing: "-0.4px", margin: 0 }}>
              {equipmentToEdit ? "Editar Información del Activo" : "Registrar Nuevo Activo en Inventario"}
            </h2>
            <p style={{ margin: "2px 0 0", fontSize: "12.5px", color: "var(--text-muted)" }}>
              {equipmentToEdit
                ? `Actualizando especificaciones y custodia de [${equipmentToEdit.inventoryKey}]`
                : "Ingresa los datos técnicos, custodia física y estado operativo del equipo."}
            </p>
          </div>
        </div>

        <button
          type="button"
          className="modal-close"
          onClick={onClose}
          aria-label="Cerrar modal"
          title="Cerrar (Esc)"
        >
          <CloseIcon size={16} />
        </button>
      </div>

      <form onSubmit={handleSubmit}>
        <div className="modal-body">
          <AutoDismissAlert
            type="error"
            message={error}
            duration={5000}
            onDismiss={() => setError(null)}
          />

          {/* SECCIÓN 1: IDENTIFICACIÓN PRINCIPAL */}
          <div className="form-section-card">
            <div className="form-section-header">
              <span className="form-section-title">
                <TagIcon size={15} style={{ color: "var(--color-primary-600)" }} />
                Identificación del Activo
              </span>
              <span className="form-section-tag">Datos Principales</span>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1.3fr", gap: "14px" }}>
              <div className="form-group">
                <label className="form-label" htmlFor="eq-key">
                  Placa / Código de Inventario <span className="required-dot">*</span>
                </label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <TagIcon size={15} />
                  </span>
                  <input
                    id="eq-key"
                    name="inventoryKey"
                    className="form-input"
                    type="text"
                    placeholder="Ej. LAP-2026-0042"
                    value={inventoryKey}
                    onChange={(e) => setInventoryKey(e.target.value)}
                    disabled={saving || Boolean(equipmentToEdit)}
                    style={{
                      fontFamily: "var(--font-mono)",
                      fontWeight: 700,
                      letterSpacing: "0.5px",
                      textTransform: "uppercase",
                    }}
                    required
                  />
                </div>
                {equipmentToEdit && (
                  <span style={{ fontSize: "11px", color: "var(--text-muted)", marginTop: "2px" }}>
                    El código único no se puede modificar.
                  </span>
                )}
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-name">
                  Nombre / Modelo del Equipo <span className="required-dot">*</span>
                </label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <EquipmentIcon size={15} />
                  </span>
                  <input
                    id="eq-name"
                    name="nombreEquipo"
                    className="form-input"
                    type="text"
                    placeholder="Ej. ThinkPad T14 Gen 4 / Latitude 5440"
                    value={nombreEquipo}
                    onChange={(e) => setNombreEquipo(e.target.value)}
                    disabled={saving}
                    required
                  />
                </div>
              </div>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "14px" }}>
              <div className="form-group">
                <label className="form-label" htmlFor="eq-marca">Marca</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <LayersIcon size={15} />
                  </span>
                  <input
                    id="eq-marca"
                    name="marca"
                    className="form-input"
                    type="text"
                    placeholder="Ej. Lenovo, Dell, HP, Apple"
                    value={marca}
                    onChange={(e) => setMarca(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-empresa">Empresa / Razón Social</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <BuildingIcon size={15} />
                  </span>
                  <input
                    id="eq-empresa"
                    name="empresa"
                    className="form-input"
                    type="text"
                    placeholder="Ej. MiEmpresa S.A.S."
                    value={empresa}
                    onChange={(e) => setEmpresa(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>
            </div>
          </div>

          {/* SECCIÓN 2: ESPECIFICACIONES TÉCNICAS */}
          <div className="form-section-tinted">
            <div className="form-section-header">
              <span className="form-section-title">
                <CpuIcon size={15} style={{ color: "var(--color-primary-600)" }} />
                Especificaciones Técnicas de Hardware
              </span>
              <span className="form-section-tag">Specs Técnicas</span>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1.2fr 1fr 1fr", gap: "14px" }}>
              <div className="form-group">
                <label className="form-label" htmlFor="eq-cpu">Procesador (CPU)</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <CpuIcon size={15} />
                  </span>
                  <input
                    id="eq-cpu"
                    name="procesador"
                    className="form-input"
                    type="text"
                    placeholder="Ej. Core i7-1365U / M2 Pro"
                    value={procesador}
                    onChange={(e) => setProcesador(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-ram">Memoria RAM</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <LayersIcon size={15} />
                  </span>
                  <input
                    id="eq-ram"
                    name="memoriaRam"
                    className="form-input"
                    type="text"
                    placeholder="Ej. 16 GB DDR5"
                    value={memoriaRam}
                    onChange={(e) => setMemoriaRam(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-disk">Almacenamiento (Disco)</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <HardDriveIcon size={15} />
                  </span>
                  <input
                    id="eq-disk"
                    name="discoDuro"
                    className="form-input"
                    type="text"
                    placeholder="Ej. 512 GB NVMe SSD"
                    value={discoDuro}
                    onChange={(e) => setDiscoDuro(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>
            </div>
          </div>

          {/* SECCIÓN 3: ESTADO OPERATIVO Y ASIGNACIÓN */}
          <div className="form-section-card">
            <div className="form-section-header">
              <span className="form-section-title">
                <UserIcon size={15} style={{ color: "var(--color-primary-600)" }} />
                Estado Operativo y Asignación
              </span>
              <span className="form-section-tag">Custodia y Ciclo</span>
            </div>

            <div className="form-group">
              <label className="form-label">Estado Operativo del Equipo</label>
              <div className="status-pill-group">
                {STATUS_OPTIONS.map((opt) => {
                  const isSelected = estatusEquipo === opt.value;
                  return (
                    <button
                      key={opt.value}
                      type="button"
                      className={`status-pill-btn ${isSelected ? `active-${opt.value}` : ""}`}
                      onClick={() => setEstatusEquipo(opt.value)}
                      disabled={saving}
                    >
                      <span
                        style={{
                          width: "8px",
                          height: "8px",
                          borderRadius: "50%",
                          background: opt.dotColor,
                          display: "inline-block",
                        }}
                      />
                      <span>{opt.label}</span>
                      {isSelected && <CheckIcon size={13} style={{ marginLeft: "2px" }} />}
                    </button>
                  );
                })}
              </div>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "1.2fr 1fr 1fr", gap: "14px" }}>
              <div className="form-group">
                <label className="form-label" htmlFor="eq-asignado">Responsable (Asignado a)</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <UserIcon size={15} />
                  </span>
                  <input
                    id="eq-asignado"
                    name="asignadoA"
                    className="form-input"
                    type="text"
                    placeholder="Nombre del colaborador"
                    value={asignadoA}
                    onChange={(e) => setAsignadoA(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-dep">Departamento / Área</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <BuildingIcon size={15} />
                  </span>
                  <input
                    id="eq-dep"
                    name="departamento"
                    className="form-input"
                    type="text"
                    placeholder="Ej. Tecnología, Finanzas"
                    value={departamento}
                    onChange={(e) => setDepartamento(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" htmlFor="eq-prov">Proveedor / Factura</label>
                <div className="input-with-icon">
                  <span className="input-icon">
                    <TruckIcon size={15} />
                  </span>
                  <input
                    id="eq-prov"
                    name="proveedor"
                    className="form-input"
                    type="text"
                    placeholder="Ej. Compumundo / #INV-92"
                    value={proveedor}
                    onChange={(e) => setProveedor(e.target.value)}
                    disabled={saving}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* Footer Fijo */}
        <div className="modal-footer">
          <span style={{ fontSize: "11.5px", color: "var(--text-muted)", display: "flex", alignItems: "center", gap: "4px" }}>
            <span style={{ color: "var(--color-danger-500)", fontWeight: 700 }}>*</span> Campos requeridos para el catálogo
          </span>

          <div style={{ display: "flex", alignItems: "center", gap: "10px" }}>
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={saving}
            >
              Cancelar
            </button>
            <button
              type="submit"
              className="btn btn-primary"
              disabled={saving}
              style={{ minWidth: "160px" }}
            >
              {saving ? (
                <>
                  <span
                    style={{
                      width: "14px",
                      height: "14px",
                      border: "2px solid #ffffff",
                      borderTopColor: "transparent",
                      borderRadius: "50%",
                      animation: "spin 0.8s linear infinite",
                      display: "inline-block",
                    }}
                  />
                  <span>Guardando...</span>
                </>
              ) : (
                <>
                  {equipmentToEdit ? <CheckIcon size={15} /> : <PlusIcon size={15} />}
                  <span>{equipmentToEdit ? "Actualizar cambios" : "Registrar equipo"}</span>
                </>
              )}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
