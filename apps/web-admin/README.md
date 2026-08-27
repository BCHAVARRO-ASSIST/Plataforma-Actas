# Plataforma de Actas — Web Admin

Aplicación web Single Page Application (SPA) para la administración, control de inventario, emisión de actas y captura de firmas digitales manuscritas.

## 🛠️ Stack Tecnológico
- **Framework**: React 19
- **Build Tool**: Vite 7
- **Lenguaje**: TypeScript 5.8
- **Estilos**: Vanilla CSS modular con variables y diseño responsivo
- **Iconografía y Gráficos**: SVG nativo interactivo (Donut charts, Timeline bars, métricas ejecutivas)
- **Firma Digital**: HTML5 Canvas con captura táctil y compatibilidad multidispositivo

## 🚀 Desarrollo Local

```bash
# Instalar dependencias
npm install

# Iniciar servidor de desarrollo (por defecto http://localhost:5173)
npm run dev

# Compilar para producción
npm run build

# Validar linting
npm run lint
```

## 🌐 Configuración de Entorno
Copia `.env.example` a `.env.local` para configurar la URL del backend:
```env
VITE_API_URL=http://localhost:8080
```
