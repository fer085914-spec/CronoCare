//const formRegistro = document.getElementById("registerForm");
//const nombre = document.getElementById("nombre");
//const correo = document.getElementById("email");
//const password = document.getElementById("password");
//const btnRegister = document.getElementById("btnRegister");
//const mensajeRegistro = document.getElementById("mensajeRegistro");

//formRegistro.addEventListener("submit", async (evento) => {
//    evento.preventDefault();

//    mensajeRegistro.classList.add("hidden");
//    btnRegister.disabled = true;
//    btnRegister.textContent = "CREANDO CUENTA...";
//    btnRegister.classList.add("opacity-50", "cursor-not-allowed");

//    // Captura el valor del radio button seleccionado
//    const tipoUso = document.querySelector('input[name="uso"]:checked').value;

//    try {
//        const respuesta = await fetch("/api/Auth/registro", {
//            method: "POST",
//            headers: { "Content-Type": "application/json" },
//            body: JSON.stringify({
//                nombre: nombre.value.trim(),
//                correo: correo.value.trim(),
//                password: password.value,
//                tipoUso: tipoUso
//            })
//        });

//        const resultado = await respuesta.json();

//        if (!respuesta.ok || !resultado.isSuccess) {
//            throw new Error(resultado.respuesta || "No fue posible registrar la cuenta.");
//        }

//        // Redirige al login tras un registro exitoso
//        window.location.replace("login.html");

//    } catch (error) {
//        mensajeRegistro.textContent = error.message || "Ocurrió un error al conectar con la API.";
//        mensajeRegistro.classList.remove("hidden");
//    } finally {
//        btnRegister.disabled = false;
//        btnRegister.textContent = "CREAR CUENTA";
//        btnRegister.classList.remove("opacity-50", "cursor-not-allowed");
//    }
//});
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

    // Captura el valor del radio button seleccionado ('personal', 'cuidador', o 'ambos')
    const tipoUsoInput = document.querySelector('input[name="uso"]:checked');
    const tipoUso = tipoUsoInput ? tipoUsoInput.value : 'personal';

    try {
        // Apuntamos al endpoint que configuraste en tu controlador de .NET 8
        const respuesta = await fetch("http://localhost:5299/api/Auth/registrar", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                nombre: nombre.value.trim(),
                correo: correo.value.trim(),
                contrasena: password.value,
                tipoUso: tipoUso
            })
        });

        // Verificamos si la respuesta tiene contenido antes de parsearla a JSON
        const textoRespuesta = await respuesta.text();
        const resultado = textoRespuesta ? JSON.parse(textoRespuesta) : {};

        if (!respuesta.ok || !resultado.success) {
            throw new Error(resultado.message || "No fue posible registrar la cuenta.");
        }

        alert("¡Cuenta creada con éxito!");
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