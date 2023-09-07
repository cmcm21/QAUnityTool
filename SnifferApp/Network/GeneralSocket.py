from abc import abstractmethod
from threading import Thread
from Utils.Events import Event
import socket


class GeneralSocket:
    def __init__(self, ownSocket: socket.socket, address):
        self.socket = ownSocket
        self.address = address
        self.ip = address[0]
        self.port = address[1]
        self.socketThread = Thread(target=self._socketWorker, daemon=True)
        self.listeningSocket = False
        self.id = str(self.ip)
        self.hostname = None

    @abstractmethod
    def start(self):
        return

    @abstractmethod
    def _socketWorker(self):
        return

    @abstractmethod
    def close(self):
        self.listeningSocket = False

        if self.socketThread.is_alive():
            try:
                self.socketThread.join()
            except RuntimeError:
                print(f"device {self.address} listening thread couldn't join ")

        self.socket.close()
