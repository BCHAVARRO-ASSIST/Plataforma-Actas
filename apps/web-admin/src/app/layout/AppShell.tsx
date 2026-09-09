import type { ReactNode } from "react";

interface AppShellProps {
  sidebar: ReactNode;
  header: ReactNode;
  children: ReactNode;
}

export function AppShell({
  sidebar,
  header,
  children,
}: AppShellProps) {
  return (
    <div className="app-shell">
      {sidebar}
      <main className="app-main">
        {header}
        <div className="page-container">
          {children}
        </div>
      </main>
    </div>
  );
}
