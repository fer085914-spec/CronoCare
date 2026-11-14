document.addEventListener("DOMContentLoaded", () => {
    // mostrar el nombre del usuario logueado
    const usuarioGuardado = localStorage.getItem("usuarioLogueado");

    if (usuarioGuardado) {
        try {
            const usuario = JSON.parse(usuarioGuardado);
            const spanNombre = document.getElementById("nombreUsuario");

            if (spanNombre && usuario.nombre) {
                spanNombre.textContent = usuario.nombre;
            }
        } catch (error) {
            console.error("Error al leer los datos del usuario desde localStorage:", error);
        }
    } else {
        window.location.href = "login.html";
    }

    // menu lateral
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const openBtn = document.getElementById('openSidebar');
    const closeBtn = document.getElementById('closeSidebar');

    function toggleMenu() {
        if (sidebar && overlay) {
            sidebar.classList.toggle('translate-x-full');
            overlay.classList.toggle('hidden');
        }
    }

    if (openBtn) openBtn.addEventListener('click', toggleMenu);
    if (closeBtn) closeBtn.addEventListener('click', toggleMenu);
    if (overlay) overlay.addEventListener('click', toggleMenu);

    // carrar sesion
    function cerrarSesion() {
        localStorage.removeItem("usuarioLogueado");
        window.location.replace("login.html");
    }

    const btnCerrarSesionLateral = document.getElementById('btnCerrarSesionLateral');
    if (btnCerrarSesionLateral) {
        btnCerrarSesionLateral.addEventListener('click', cerrarSesion);
    }

    const btnCerrarSesionMovil = document.getElementById('btnCerrarSesion');
    if (btnCerrarSesionMovil) {
        btnCerrarSesionMovil.addEventListener('click', cerrarSesion);
    }
});
