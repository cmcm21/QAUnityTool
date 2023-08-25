import socket
from threading import Thread
from Utils.Events import Event
from Network.Clients.DeviceClient import DeviceClient
from Network.Servers.FileServer import FileServer
from Network.GeneralSocket import GeneralSocket

bufferSize = 1024


class ServerManager(GeneralSocket):
    def __init__(self):
        super().__init__(socket.socket(), (socket.gethostbyname(socket.gethostname()), 8080))
        self.maxDevices = 5
        self.devices: list[DeviceClient] = []
        self.selectedDevice: DeviceClient | None = None
        self.serverInitEvent = Event()
        self.newDeviceConnectedEvent = Event()
        self.NoMoreDevicesConnectedEvent = Event()
        self.fileServer = FileServer(self.ip, 9999)

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        address = (self.ip, self.port)
        self.socket.bind(address)
        self.socket.listen(self.maxDevices)
        self.serverInitEvent(message="Server started at :" + self.ip + " on port: " + str(self.port))
        self.socketThread.start()

    def _socketWorker(self):
        while self.listeningSocket:
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
        device.start()

    def _deviceDisconnected(self, *args, **kwargs):
        device = kwargs['device']
        if device == self.selectedDevice:
            self.selectedDevice = None

        self.devices.remove(device)

        if len(self.devices) <= 0:
            self.NoMoreDevicesConnectedEvent()

    def close(self):
        self.listeningSocket = False

        if self.socketThread.is_alive():
            self.socketThread.join()

        for device in self.devices:
            device.close()

        self.socket.close()

    def __del__(self):
        self.close()
