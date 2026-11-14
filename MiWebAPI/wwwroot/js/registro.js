const formRegistro = document.getElementById("registerForm");
const nombre = document.getElementById("nombre");
const correo = document.getElementById("email");
const password = document.getElementById("password");
const btnRegister = document.getElementById("btnRegister");
const mensajeRegistro = document.getElementById("mensajeRegistro");

formRegistro.addEventListener("submit", async (evento) => {
    evento.preventDefault();

    mensajeRegistro.classList.add("hidden");
    btnRegister.disabled = true;
    btnRegister.textContent = "CREANDO CUENTA...";
    btnRegister.classList.add("opacity-50", "cursor-not-allowed");

    const valorCorreo = correo.value.trim();
    const valorPassword = password.value;

    // validar el formato del correo
    const regexCorreo = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!regexCorreo.test(valorCorreo)) {
        mensajeRegistro.textContent = "Por favor, ingresa un formato de correo electrónico válido.";
        mensajeRegistro.classList.remove("hidden");

        btnRegister.disabled = false;
        btnRegister.textContent = "CREAR CUENTA";
        btnRegister.classList.remove("opacity-50", "cursor-not-allowed");
        return; 
    }

    // contraseña con 8 caracteres
    if (valorPassword.length < 8) {
        mensajeRegistro.textContent = "La contraseña debe tener al menos 8 caracteres.";
        mensajeRegistro.classList.remove("hidden");

        btnRegister.disabled = false;
        btnRegister.textContent = "CREAR CUENTA";
        btnRegister.classList.remove("opacity-50", "cursor-not-allowed");
        return; // Detiene el envío
    }

    const tipoUsoInput = document.querySelector('input[name="uso"]:checked');
    const tipoUso = tipoUsoInput ? tipoUsoInput.value : "Personal";

    try {
        const respuesta = await fetch("https://localhost:7150/api/Auth/registrar", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre: nombre.value.trim(),
                correo: valorCorreo,
                contrasena: valorPassword,
                tipoUso: tipoUso
            })
        });

        const textoRespuesta = await respuesta.text();
        const resultado = textoRespuesta ? JSON.parse(textoRespuesta) : {};

        if (!respuesta.ok || !resultado.success) {
            throw new Error(resultado.message || "No fue posible registrar la cuenta.");
        }

        alert("¡Cuenta creada con éxito! Por favor inicia sesión.");
        window.location.replace("login.html");

    } catch (error) {
        mensajeRegistro.textContent = error.message || "Ocurrió un error al conectar con la API.";
        mensajeRegistro.classList.remove("hidden");
    } finally {
        btnRegister.disabled = false;
        btnRegister.textContent = "CREAR CUENTA";
        btnRegister.classList.remove("opacity-50", "cursor-not-allowed");
    }
});