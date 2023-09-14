# Sniffer Plugin (Plugin de Unity)
## Instalar Plugin.

Para la instalación, por el momento se tiene que hacer de forma manual.
Ve a la carpeta del projecto de Sniffer, depués ve a la carpeta de Packages.
Una vez ahí, copia la carpeta **Tagwizz QA Sniffer** y pega esta carpeta dentro de la carpeta 
Packages dentro del Juego donde quieres instalar el plugin.

![[TagwizzQASnifferPath.png]]

### Packages Necesarios para utilizar Sniffer.

Las dependencias de Sniffer deberian de instalarse de forma automatica al momento de pegar el paquete en el projecto.

## Sniffer Core

El sniffer core es la responsable de ejecutar las siguientes acciones (entre otras cosas).
-  Record
-  Stop
-  Save (To a file)
-  Load (from a file)
-  Replay
-  Stop Replay

El sniffer core carga la configuración y dependiendo de los settings que se pongan en el SnifferSettings el comportamiento de este cambia.
	- El SnifferSettings es un scriptable Object que se crea de forma automatica en la Carpeta de Assets/Resources cuando se agrega el package al proyecto de Unity.
 ![[SnifferSettingsImage.png]]

El prefab de TagwizzQASnifferLifeCycle se encuentra en este path: Packages/com.tagwizz.qa.sniffer/Runtime/Core/TagwizzQASnifferLifeCycle.prefab
arrastra este prefab al campo de **Sniffer Observer** en el **SnifferSettings**

Tambien es necesario que se marque la casilla de Livestreaming , se ponga un numero de frames maximo y por ultimo el frame rate en el que se deseé hacer livestreaming.

- Input recorder (EventTrace) algunas veces no detecta los primeros inputs y los inputs finales
	- El Input Recorder es el encargado de hacer el recording de los inputs del usuario y guardarlos (los guarda en un buffer, el tamaño de este buffer se pasa al inicializar el event Trace en la clase Input Recorder.)
	- El input Recorder solo es un wrapper para controlar el EventTrace que es una clase del nuevo systema de inputs de Unity.
	- Este EventTrace Tiene un objeto llamado replayerController.
		- El ReplayerController es el encargado de hacer el replay del recording de los inputs.
	- Para mas información sobre el EventTrace puedes consultar su documentación: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.LowLevel.InputEventTrace.html#UnityEngine_InputSystem_LowLevel_InputEventTrace_FrameMarkerEvent
	- Para más información sobre el ReplayerController puedes consultar su documentación: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.LowLevel.InputEventTrace.ReplayController.html#UnityEngine_InputSystem_LowLevel_InputEventTrace_ReplayController_trace
- 


# Sniffer Hub ( Aplicación de Python )

Para el desarrollo de esta aplicación se utilizo la version 3.11 de python.
antes de pasar a la configuración, asegurate de tener esta version de python instalada en tu maquina.

Todas las dependencias para poder ejectuar el proyecto se encuentran en el virtual environment en la carpeta de venv.

# Pycharm configuration

Para configurar el virtual environment en pycharm se tienen que seguir los siguientes pasos:

- Abre la carpeta del proyecto con pycharm. (qa_sniffer\\SnifferApp)
- Presiona el boton para agregar un nuevo interpretador al proyecto y despues presiona agregar local interpreter ![[PythonInterpreterScreenShot.png]]
- Se deberia de mostrar la siguiente pantalla.
- ![[Pasted image 20230913174619.png]]
- En esta pantalla selecciona **Existing** y despues selecciona el icono de los tres puntitos.
- ![[Pasted image 20230913174720.png]]
Busca el archivo python.exe dentro de la carpeta venv/Scripts del mismo proyecto.
![[Pasted image 20230913175018.png]]

selecciona el archivo python.exe y depues presiona ok y apply para que se empieze a descargar de forma automatica todas las dependencias de python para ejectuar el proyecto.

Con esto ya deberias poder ejecutar el proyecto.