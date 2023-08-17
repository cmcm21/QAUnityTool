import socket
from threading import Thread
from Utils.Events import Event
from enum import Enum

buffer_size = 1024


class DeviceState(Enum):
    RECORDING = 0
    PLAYBACK = 1


class DeviceManager:

    def __init__(self, deviceSocket: socket.socket, address):
        print("client: " + str(address) + " connected")
        self.client = deviceSocket
        self.address = address
        self.deviceIp = address[0]
        self.listenThread = Thread(target=self._listenClientWorker)
        self.listeningClient = False
        self.msgReceivedEvent = Event()

    def _listenClientWorker(self):
        while self.listeningClient:
            message = self.client.recv(buffer_size).decode()
            self.msgReceivedEvent(message="Device[{}]: {}".format(self.deviceIp, message))
            print(message)

    def starListening(self):
        self.listeningClient = True
        self.listenThread.start()

    def close(self):
        self.listeningClient = False

        if self.listenThread.is_alive():
            self.listenThread.join()
