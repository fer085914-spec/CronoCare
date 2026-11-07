//const SESSION_KEY = "empleados_usuario";
//const formLogin = document.getElementById("loginForm");
//const correo = document.getElementById("email");
//const password = document.getElementById("password");
//const btnLogin = document.getElementById("btnLogin");
//const mensajeLogin = document.getElementById("mensajeLogin");

//if (sessionStorage.getItem(SESSION_KEY)) window.location.replace("index.html");

//formLogin.addEventListener("submit", async (evento) => {
//    evento.preventDefault();

//    mensajeLogin.classList.add("hidden");
//    btnLogin.disabled = true;
//    btnLogin.textContent = "INICIANDO SESIÓN...";
//    btnLogin.classList.add("opacity-50", "cursor-not-allowed");

//    try {
//        const respuesta = await fetch("/api/Auth/login", {
//            method: "POST",
//            headers: { "Content-Type": "application/json" },
//            body: JSON.stringify({ correo: correo.value.trim(), password: password.value })
//        });

//        const resultado = await respuesta.json();

//        if (!respuesta.ok || !resultado.isSuccess) {
//            throw new Error(resultado.respuesta || "No fue posible iniciar sesión.");
//        }

//        sessionStorage.setItem(SESSION_KEY, JSON.stringify(resultado.usuario));
//        window.location.replace("index.html");

//    } catch (error) {
//        mensajeLogin.textContent = error.message || "Ocurrió un error al conectar con la API.";
//        mensajeLogin.classList.remove("hidden");
//    } finally {
//        btnLogin.disabled = false;
//        btnLogin.textContent = "INICIAR SESIÓN";
//        btnLogin.classList.remove("opacity-50", "cursor-not-allowed");
//    }
//});
const formLogin = document.getElementById("loginForm"); // Asegúrate de que tu form en HTML tenga este ID
const correoInput = document.getElementById("email");    // Asegúrate de que el input de correo tenga id="email"
const passwordInput = document.getElementById("password"); // Asegúrate de que el input tenga id="password"
const btnLogin = document.getElementById("btnLogin");      // El botón de enviar
const mensajeLogin = document.getElementById("mensajeLogin"); // Elemento opcional para mostrar errores en pantalla

if (formLogin) {
    formLogin.addEventListener("submit", async (evento) => {
        evento.preventDefault();

        if (mensajeLogin) mensajeLogin.classList.add("hidden");
        if (btnLogin) {
            btnLogin.disabled = true;
            btnLogin.textContent = "INICIANDO SESIÓN...";
            btnLogin.classList.add("opacity-50", "cursor-not-allowed");
        }

        try {
            const respuesta = await fetch("/api/Auth/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    correo: correoInput.value.trim(),
                    contrasena: passwordInput.value
                })
            });

            const textoRespuesta = await respuesta.text();
            const resultado = textoRespuesta ? JSON.parse(textoRespuesta) : {};

            if (!respuesta.ok || !resultado.success) {
                throw new Error(resultado.message || "Credenciales incorrectas.");
            }

            // Guardamos los datos del usuario en el almacenamiento local para usarlos en el Dashboard
            localStorage.setItem("usuarioLogueado", JSON.stringify(resultado.usuario));

            alert("¡Bienvenido de nuevo, " + resultado.usuario.nombre + "!");
            window.location.replace("index.html"); // Redirige a tu menú principal o dashboard

        } catch (error) {
            if (mensajeLogin) {
                mensajeLogin.textContent = error.message || "Ocurrió un error al conectar con la API.";
                mensajeLogin.classList.remove("hidden");
            } else {
                alert(error.message);
            }
        } finally {
            if (btnLogin) {
                btnLogin.disabled = false;
                btnLogin.textContent = "INICIAR SESIÓN";
                btnLogin.classList.remove("opacity-50", "cursor-not-allowed");
            }
        }
    });
}