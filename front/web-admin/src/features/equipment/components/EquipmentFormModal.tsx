import { useState } from "react";
import type { Equipment, CreateEquipmentRequest, UpdateEquipmentRequest } from "../types/equipment";

interface EquipmentFormModalProps {
  isOpen: boolean;
  equipmentToEdit: Equipment | null;
  onClose: () => void;
  onSave: (data: CreateEquipmentRequest | UpdateEquipmentRequest) => Promise<void>;
  saving: boolean;
}

interface EquipmentFormContentProps {
  equipmentToEdit: Equipment | null;
  onClose: () => void;
  onSave: (data: CreateEquipmentRequest | UpdateEquipmentRequest) => Promise<void>;
  saving: boolean;
}

function EquipmentFormContent({
  equipmentToEdit,
  onClose,
  onSave,
  saving,
}: EquipmentFormContentProps) {
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
      setError("El código de inventario es obligatorio.");
      return;
    }
    if (!nombreEquipo.trim()) {
      setError("El nombre del equipo es obligatorio.");
      return;
    }

    try {
      setError(null);
      await onSave({
        inventoryKey: inventoryKey.trim(),
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
    <div
      style={{
        background: "#ffffff",
        borderRadius: "18px",
        width: "100%",
        maxWidth: "680px",
        maxHeight: "90vh",
        overflowY: "auto",
        padding: "28px",
        boxShadow: "0 25px 50px -12px rgba(0, 0, 0, 0.25)",
        border: "1px solid #e2e8f0",
      }}
      onClick={(e) => e.stopPropagation()}
    >
      {/* HEADER */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "20px" }}>
        <div>
          <span style={{ fontSize: "11px", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.5px" }}>
            GESTIÓN DE INVENTARIO
          </span>
          <h2 style={{ margin: "2px 0 0", fontSize: "20px", fontWeight: 800, color: "#0f172a" }}>
            {equipmentToEdit ? "Editar Equipo Tecnológico" : "Registrar Nuevo Equipo"}
          </h2>
        </div>

        <button
          type="button"
          onClick={onClose}
          style={{
            background: "#f1f5f9",
            border: "none",
            borderRadius: "50%",
            width: "32px",
            height: "32px",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            cursor: "pointer",
            color: "#64748b",
            fontSize: "14px",
            fontWeight: 700,
          }}
          aria-label="Cerrar modal"
        >
          ✕
        </button>
      </div>

      {/* ERROR */}
      {error && (
        <div
          style={{
            marginBottom: "18px",
            padding: "12px 16px",
            background: "#fee2e2",
            border: "1px solid #fecaca",
            borderRadius: "10px",
            color: "#991b1b",
            fontSize: "13px",
            fontWeight: 500,
          }}
        >
          {error}
        </div>
      )}

      {/* FORM */}
      <form onSubmit={handleSubmit}>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: "16px" }}>
          {/* INVENTARIO */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Código de Inventario *
            </label>
            <input
              type="text"
              required
              value={inventoryKey}
              onChange={(e) => setInventoryKey(e.target.value)}
              placeholder="Ej. EQ-2026-001 o N° Serie"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* NOMBRE */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Nombre / Modelo del Equipo *
            </label>
            <input
              type="text"
              required
              value={nombreEquipo}
              onChange={(e) => setNombreEquipo(e.target.value)}
              placeholder="Ej. Laptop Dell Latitude 5420"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* MARCA */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Marca
            </label>
            <input
              type="text"
              value={marca}
              onChange={(e) => setMarca(e.target.value)}
              placeholder="Ej. Dell, Lenovo, HP, Apple"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* ESTADO */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Estado Operativo
            </label>
            <select
              value={estatusEquipo}
              onChange={(e) => setEstatusEquipo(e.target.value)}
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px", background: "#ffffff" }}
            >
              <option value="Activo">Activo (Disponible en inventario)</option>
              <option value="Asignado">Asignado (En custodia de usuario)</option>
              <option value="Inactivo">Inactivo (Bodega / De baja)</option>
            </select>
          </div>

          {/* PROCESADOR */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Procesador
            </label>
            <input
              type="text"
              value={procesador}
              onChange={(e) => setProcesador(e.target.value)}
              placeholder="Ej. Intel Core i7 12th Gen / Ryzen 7"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* RAM */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Memoria RAM
            </label>
            <input
              type="text"
              value={memoriaRam}
              onChange={(e) => setMemoriaRam(e.target.value)}
              placeholder="Ej. 16 GB DDR4"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* DISCO */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Disco Duro / Almacenamiento
            </label>
            <input
              type="text"
              value={discoDuro}
              onChange={(e) => setDiscoDuro(e.target.value)}
              placeholder="Ej. 512 GB SSD NVMe"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* DEPARTAMENTO */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Departamento / Área
            </label>
            <input
              type="text"
              value={departamento}
              onChange={(e) => setDepartamento(e.target.value)}
              placeholder="Ej. Tecnología, Finanzas, Operaciones"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* EMPRESA */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Empresa
            </label>
            <input
              type="text"
              value={empresa}
              onChange={(e) => setEmpresa(e.target.value)}
              placeholder="Ej. Organización Principal"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* ASIGNADO A */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Asignado a (Custodio)
            </label>
            <input
              type="text"
              value={asignadoA}
              onChange={(e) => setAsignadoA(e.target.value)}
              placeholder="Ej. Nombre del funcionario"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>

          {/* PROVEEDOR */}
          <div>
            <label style={{ display: "block", fontSize: "12px", fontWeight: 700, color: "#334155", marginBottom: "6px" }}>
              Proveedor
            </label>
            <input
              type="text"
              value={proveedor}
              onChange={(e) => setProveedor(e.target.value)}
              placeholder="Ej. Distribuidor Autorizado"
              disabled={saving}
              style={{ width: "100%", padding: "10px 14px", border: "1px solid #cbd5e1", borderRadius: "8px", fontSize: "14px" }}
            />
          </div>
        </div>

        {/* BOTONES */}
        <div style={{ display: "flex", justifyContent: "flex-end", gap: "12px", marginTop: "24px", borderTop: "1px solid #f1f5f9", paddingTop: "18px" }}>
          <button
            type="button"
            className="actas-reset"
            onClick={onClose}
            disabled={saving}
          >
            Cancelar
          </button>
          <button
            type="submit"
            className="actas-primary"
            disabled={saving}
          >
            {saving ? "Guardando..." : equipmentToEdit ? "Guardar Cambios" : "+ Registrar en Inventario"}
          </button>
        </div>
      </form>
    </div>
  );
}

export function EquipmentFormModal({
  isOpen,
  equipmentToEdit,
  onClose,
  onSave,
  saving,
}: EquipmentFormModalProps) {
  if (!isOpen) return null;

  return (
    <div
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        background: "rgba(15, 23, 42, 0.65)",
        backdropFilter: "blur(6px)",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        zIndex: 9999,
        padding: "20px",
      }}
      onClick={onClose}
    >
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
