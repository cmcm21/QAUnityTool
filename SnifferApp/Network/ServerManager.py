import socket
from threading import Thread
from Utils.Events import Event
from Network.DeviceManager import DeviceManager
from Network.FileServer import FileServer

bufferSize = 1024


class ServerManager:
    def __init__(self):
        self.listening = False
        self.socket = socket.socket()
        self.port = 8080
        self.maxDevices = 15
        self.hostname = socket.gethostname()
        self.Ip = socket.gethostbyname(self.hostname)
        self.devices: list[DeviceManager] = []
        self.selectedDevice: DeviceManager | None = None
        self.listenThread = Thread(target=self._listen, daemon=True)
        self.serverInitEvent = Event()
        self.newDeviceConnectedEvent = Event()
        self.fileServer = FileServer(self.Ip, self.port)

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
            (client, address) = self.socket.accept()
            device = DeviceManager(client, address)
            device.starListening()
            self.devices.append(device)
            self.newDeviceConnectedEvent(device=device)

        self.close()

    def setDeviceSelected(self, device: DeviceManager):
        self.selectedDevice = device

    def close(self):
        self.listening = False

        if self.listenThread.is_alive():
            self.listenThread.join()

        for device in self.devices:
            device.close()

        self.socket.close()

    def __del__(self):
        self.close()
