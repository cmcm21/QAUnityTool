import socket
from threading import Thread

buffer_size = 1024


class DeviceManager:

    def __init__(self, deviceSocket: socket.socket, address):
        print("client: " + str(address) + " connected")
        self.client = deviceSocket
        self.address = address
        self.listenThread = Thread(target=self._listenClientWorker)
        self.listeningClient = False

    def _listenClientWorker(self):
        while self.listeningClient:
            message = self.client.recv(buffer_size).decode()
            print(message)

    def starListening(self):
        self.listeningClient = True
        self.listenThread.start()

    def close(self):
        self.listeningClient = False

        if self.listenThread.is_alive():
            self.listenThread.join()
