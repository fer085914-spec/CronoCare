//const formulario = document.getElementById("formEmpleado");
//const tabla = document.getElementById("tablaEmpleados");
//const filtro = document.getElementById("txtFiltro");
//const mensaje = document.getElementById("mensaje");
//const sinResultados = document.getElementById("sinResultados");
//const tituloFormulario = document.getElementById("tituloFormulario");
//const btnGuardar = document.getElementById("btnGuardar");
//const btnCancelar = document.getElementById("btnCancelar");
//const btnRecargar = document.getElementById("btnRecargar");

//let empleados = [];

//document.addEventListener("DOMContentLoaded", obtenerEmpleados);
//formulario.addEventListener("submit", guardarEmpleado);
//filtro.addEventListener("input", aplicarFiltro);
//btnCancelar.addEventListener("click", limpiarFormulario);
//btnRecargar.addEventListener("click", obtenerEmpleados);

//async function solicitar(url, opciones = {}) {
//  const respuesta = await fetch(url, opciones);

//  if (!respuesta.ok) {
//    const detalle = await respuesta.text();
//    throw new Error(detalle || `Error HTTP ${respuesta.status}`);
//  }

//  if (respuesta.status === 204) {
//    return null;
//  }

//  return respuesta.json();
//}

//async function obtenerEmpleados() {
//  cambiarEstadoCarga(true);

//  try {
//    empleados = await solicitar(API_URL);
//    aplicarFiltro();
//  } catch (error) {
//    mostrarMensaje(
//      "No fue posible consultar la API. Confirma que el proyecto .NET se encuentre ejecutándose.",
//      "danger"
//    );
//    console.error(error);
//  } finally {
//    cambiarEstadoCarga(false);
//  }
//}

//function mostrarEmpleados(lista) {
//  tabla.innerHTML = "";
//  sinResultados.classList.toggle("d-none", lista.length > 0);

//  lista.forEach((empleado) => {
//    const fila = document.createElement("tr");
//    fila.innerHTML = `
//      <td>${escaparHtml(empleado.nombreCompleto)}</td>
//      <td>${escaparHtml(empleado.correo)}</td>
//      <td class="text-end">${formatearMoneda(empleado.sueldo)}</td>
//      <td>${escaparHtml(empleado.fechaContrato)}</td>
//      <td class="text-center acciones">
//        <button class="btn btn-sm btn-outline-primary" data-accion="editar"
//                data-id="${empleado.idEmpleado}">Editar</button>
//        <button class="btn btn-sm btn-outline-danger" data-accion="eliminar"
//                data-id="${empleado.idEmpleado}">Eliminar</button>
//      </td>`;
//    tabla.appendChild(fila);
//  });
//}

//tabla.addEventListener("click", (evento) => {
//  const boton = evento.target.closest("button[data-accion]");
//  if (!boton) return;

//  const id = Number(boton.dataset.id);
//  if (boton.dataset.accion === "editar") cargarEmpleado(id);
//  if (boton.dataset.accion === "eliminar") eliminarEmpleado(id);
//});

//async function cargarEmpleado(id) {
//  try {
//    const empleado = await solicitar(`${API_URL}/${id}`);

//    document.getElementById("idEmpleado").value = empleado.idEmpleado;
//    document.getElementById("nombreCompleto").value = empleado.nombreCompleto ?? "";
//    document.getElementById("correo").value = empleado.correo ?? "";
//    document.getElementById("sueldo").value = empleado.sueldo ?? 0;
//    document.getElementById("fechaContrato").value = convertirFechaParaInput(empleado.fechaContrato);

//    tituloFormulario.textContent = "Editar empleado";
//    btnGuardar.textContent = "Actualizar";
//    btnCancelar.classList.remove("d-none");
//    formulario.scrollIntoView({ behavior: "smooth", block: "start" });
//  } catch (error) {
//    mostrarMensaje("No fue posible obtener el empleado.", "danger");
//    console.error(error);
//  }
//}

//async function guardarEmpleado(evento) {
//  evento.preventDefault();

//  if (!formulario.checkValidity()) {
//    formulario.classList.add("was-validated");
//    return;
//  }

//  const empleado = leerFormulario();
//  const esNuevo = empleado.idEmpleado === 0;

//  try {
//    btnGuardar.disabled = true;
//    btnGuardar.textContent = esNuevo ? "Guardando..." : "Actualizando...";

//    const resultado = await solicitar(API_URL, {
//      method: esNuevo ? "POST" : "PUT",
//      headers: { "Content-Type": "application/json" },
//      body: JSON.stringify(empleado)
//    });

//    if (!resultado.isSuccess) {
//      throw new Error(`Error ${resultado.numeroError}: ${resultado.respuesta}`);
//    }

//    mostrarMensaje(resultado.respuesta, "success");
//    limpiarFormulario();
//    await obtenerEmpleados();
//  } catch (error) {
//    mostrarMensaje(error.message || "No fue posible guardar la información.", "danger");
//    console.error(error);
//  } finally {
//    btnGuardar.disabled = false;
//    btnGuardar.textContent = document.getElementById("idEmpleado").value === "0" ? "Guardar" : "Actualizar";
//  }
//}

//async function eliminarEmpleado(id) {
//  const empleado = empleados.find((item) => item.idEmpleado === id);
//  const nombre = empleado?.nombreCompleto ?? "seleccionado";

//  if (!confirm(`¿Deseas eliminar al empleado ${nombre}?`)) return;

//  try {
//    const resultado = await solicitar(`${API_URL}/${id}`, { method: "DELETE" });

//    if (!resultado.isSuccess) {
//      throw new Error(`Error ${resultado.numeroError}: ${resultado.respuesta}`);
//    }

//    mostrarMensaje(resultado.respuesta, "success");
//    await obtenerEmpleados();
//  } catch (error) {
//    mostrarMensaje(error.message || "No fue posible eliminar el empleado.", "danger");
//    console.error(error);
//  }
//}

//function leerFormulario() {
//  return {
//    idEmpleado: Number(document.getElementById("idEmpleado").value),
//    nombreCompleto: document.getElementById("nombreCompleto").value.trim(),
//    correo: document.getElementById("correo").value.trim(),
//    sueldo: Number(document.getElementById("sueldo").value),
//    fechaContrato: document.getElementById("fechaContrato").value
//  };
//}

//function limpiarFormulario() {
//  formulario.reset();
//  formulario.classList.remove("was-validated");
//  document.getElementById("idEmpleado").value = "0";
//  tituloFormulario.textContent = "Nuevo empleado";
//  btnGuardar.textContent = "Guardar";
//  btnCancelar.classList.add("d-none");
//}

//function aplicarFiltro() {
//  const texto = filtro.value.trim().toLowerCase();
//  const resultados = empleados.filter((empleado) =>
//    (empleado.nombreCompleto ?? "").toLowerCase().includes(texto) ||
//    (empleado.correo ?? "").toLowerCase().includes(texto)
//  );
//  mostrarEmpleados(resultados);
//}

//function cambiarEstadoCarga(cargando) {
//  btnRecargar.disabled = cargando;
//  btnRecargar.textContent = cargando ? "Cargando..." : "Recargar";
//}

//function mostrarMensaje(texto, tipo) {
//  mensaje.textContent = texto;
//  mensaje.className = `alert alert-${tipo}`;

//  window.clearTimeout(mostrarMensaje.temporizador);
//  mostrarMensaje.temporizador = window.setTimeout(() => {
//    mensaje.classList.add("d-none");
//  }, 4500);
//}

//function convertirFechaParaInput(fecha) {
//  if (!fecha) return "";
//  return fecha.substring(0, 10);
//}

//function formatearMoneda(valor) {
//  return new Intl.NumberFormat("es-MX", {
//    style: "currency",
//    currency: "MXN"
//  }).format(valor ?? 0);
//}

//function escaparHtml(valor) {
//  return String(valor ?? "")
//    .replaceAll("&", "&amp;")
//    .replaceAll("<", "&lt;")
//    .replaceAll(">", "&gt;")
//    .replaceAll('"', "&quot;")
//    .replaceAll("'", "&#039;");
//}

document.addEventListener("DOMContentLoaded", () => {
    // 1. Cargar y mostrar el nombre del usuario logueado
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
        // Si no hay sesión activa, redirigir al login
        window.location.href = "login.html";
    }

    // 2. Control del Menú Lateral (Sidebar)
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

    // 3. Lógica para Cerrar Sesión
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
