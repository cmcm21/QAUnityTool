import socket
from threading import Thread
from Utils.Events import Event
from Network.DeviceClient import DeviceClient
from Network.FileServer import FileServer

bufferSize = 1024


class ServerManager:
    def __init__(self):
        self.listening = False
        self.socket = socket.socket()
        self.port = 8080
        self.maxDevices = 5
        self.hostname = socket.gethostname()
        self.Ip = socket.gethostbyname(self.hostname)
        self.devices: list[DeviceClient] = []
        self.selectedDevice: DeviceClient | None = None
        self.listenThread = Thread(target=self._listen, daemon=True)
        self.serverInitEvent = Event()
        self.newDeviceConnectedEvent = Event()
        self.NoMoreDevicesConnectedEvent = Event()
        self.fileServer = FileServer(self.Ip, 9999)

    def startServer(self):
        if self.listening:
            return

        self.listening = True
        address = (self.Ip, self.port)
        self.socket.bind(address)
        self.socket.listen(self.maxDevices)
        self.serverInitEvent(message="Server started at :" + self.Ip + " on port: " + str(self.port))
        self.listenThread.start()

    def _listen(self):
        while self.listening:
            try:
                (client, address) = self.socket.accept()
                device = DeviceClient(client, address)
                self._handleNewDevice(device)
            except ConnectionError:
                print("[ServerManager]::Error while listening clients")
            except RuntimeError:
                print("[ServerManager]::Runtime Error in Server")

        self.close()

    def setDeviceSelected(self, device: DeviceClient):
        self.selectedDevice = device

    def _handleNewDevice(self, device: DeviceClient):
        self.devices.append(device)
        self.newDeviceConnectedEvent(device=device)
        device.deviceDisconnectedEvent += self._deviceDisconnected
        device.starListening()

    def _deviceDisconnected(self, *args, **kwargs):
        device = kwargs['device']
        if device == self.selectedDevice:
            self.selectedDevice = None

        self.devices.remove(device)

        if len(self.devices) <= 0:
            self.NoMoreDevicesConnectedEvent()

    def close(self):
        self.listening = False

        if self.listenThread.is_alive():
            self.listenThread.join()

        for device in self.devices:
            device.close()

        self.socket.close()

    def __del__(self):
        self.close()
