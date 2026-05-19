/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}"],
  theme: {
    extend: {
      colors: {
        surface: {
          50: '#f8f7ff',
          100: '#f0eeff',
          200: '#e4e0ff',
          800: '#1a1730',
          900: '#0f0d1f',
          950: '#07060f',
        },
        brand: {
          400: '#a78bfa',
          500: '#8b5cf6',
          600: '#7c3aed',
        },
        danger: {
          400: '#f87171',
          500: '#ef4444',
        },
        warn: {
          400: '#fbbf24',
          500: '#f59e0b',
        },
        success: {
          400: '#34d399',
          500: '#10b981',
        },
        cyan: {
          400: '#22d3ee',
          500: '#06b6d4',
        }
      },
      fontFamily: {
        display: ['Syne', 'sans-serif'],
        body: ['DM Sans', 'sans-serif'],
        mono: ['JetBrains Mono', 'monospace'],
      },
    },
  },
  plugins: [],
}
