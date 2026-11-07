const SESSION_KEY = "empleados_usuario";

function obtenerUsuario() {
  const valor = sessionStorage.getItem(SESSION_KEY);
  try {
    return valor ? JSON.parse(valor) : null;
  } catch {
    sessionStorage.removeItem(SESSION_KEY);
    return null;
  }
}

function protegerPagina() {
  if (!obtenerUsuario()) window.location.replace("login.html");
}

function mostrarUsuario(elementId) {
  const usuario = obtenerUsuario();
  const elemento = document.getElementById(elementId);
  if (usuario && elemento) elemento.textContent = usuario.nombre;
}

function cerrarSesion() {
  sessionStorage.removeItem(SESSION_KEY);
  window.location.replace("login.html");
}
