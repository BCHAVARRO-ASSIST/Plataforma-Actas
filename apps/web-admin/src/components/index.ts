// =====================================================
// BARREL DE COMPATIBILIDAD CENTRALIZADO
// Plataforma de Actas - UI & Feature Components
// =====================================================

// UI Compartidos
export * from "./ui/Icons";
export * from "./ui/AutoDismissAlert";
export * from "./ui/FirmaManuscrita";
export * from "./ui/Modal";
export * from "./ui/Pagination";
export * from "./ui/StatusBadge";

// Core App / Seguridad
export * from "./ErrorBoundary";
export * from "./ProtectedRoute";

// Features: Actas
export * from "@/features/actas/components/ActaDetail";
export * from "@/features/actas/components/ActaLivePreview";
export * from "@/features/actas/components/ActaTable";
export * from "@/features/actas/components/ActaTypeSelector";
export * from "@/features/actas/components/FirmaConsultorView";
export * from "@/features/actas/components/NewActaForm";
export * from "@/features/actas/components/OfficialActaModal";

// Features: Equipos
export * from "@/features/equipos/components/EquipoFilters";
export * from "@/features/equipos/components/EquipoFormModal";
export * from "@/features/equipos/components/EquipoTable";

// Features: Dashboard
export * from "@/features/dashboard/components/ExecutiveSummaryCards";
export * from "@/features/dashboard/components/RecentActasCard";

// Features: Mensajero
export * from "@/features/mensajero/components/MensajeroActaDetail";

// Features: Reportes
export * from "@/features/reportes/components/ReportCharts";
export * from "@/features/reportes/components/ReportFilters";
export * from "@/features/reportes/components/ReportKpi";
export * from "@/features/reportes/components/ReportTable";
