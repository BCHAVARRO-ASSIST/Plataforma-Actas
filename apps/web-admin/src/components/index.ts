// =====================================================
// BARREL DE COMPATIBILIDAD CENTRALIZADO
// Plataforma de Actas - UI & Feature Components
// =====================================================

// UI Compartidos
export * from "./ui/Icons";
export * from "./ui/AutoDismissAlert";
export * from "./ui/HandwrittenSignature";
export * from "./ui/FirmaManuscrita";
export * from "./ui/Modal";
export * from "./ui/ConfirmDialog";
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
export * from "@/features/actas/components/ConsultantSignatureView";
export * from "@/features/actas/components/FirmaConsultorView";
export * from "@/features/actas/components/NewActaForm";
export * from "@/features/actas/components/OfficialActaModal";

// Features: Equipment
export * from "@/features/equipment/components/EquipmentFilters";
export * from "@/features/equipment/components/EquipmentFormModal";
export * from "@/features/equipment/components/EquipmentTable";

// Features: Dashboard
export * from "@/features/dashboard/components/ExecutiveSummaryCards";
export * from "@/features/dashboard/components/RecentActasCard";

// Features: Delivery
export * from "@/features/delivery/components/DeliveryActaDetail";
export * from "@/features/delivery/components/RecipientNotificationModal";

// Features: Reports
export * from "@/features/reports/components/ReportCharts";
export * from "@/features/reports/components/ReportFilters";
export * from "@/features/reports/components/ReportKpi";
export * from "@/features/reports/components/ReportTable";
