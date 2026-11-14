const formLogin = document.getElementById("loginForm"); 
const correoInput = document.getElementById("email");    
const passwordInput = document.getElementById("password"); 
const btnLogin = document.getElementById("btnLogin");      
const mensajeLogin = document.getElementById("mensajeLogin"); 

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

            localStorage.setItem("usuarioLogueado", JSON.stringify(resultado.usuario));

            alert("¡Bienvenido de nuevo, " + resultado.usuario.nombre + "!");
            window.location.replace("index.html"); 

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