import asyncio
import socket
import threading
import os
from Utils.Events import Event
from Network.FileClient import FileClient

BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class FileServer:

    def __init__(self, ip: str, port: int):
        self.ip = ip
        self.port = port
        self.address = (self.ip, self.port)
        self.filePath = None
        self.listening = False
        self.sendFile = False
        self.socket = None
        self.fileClients: list[FileClient] = []
        self.listenThread = threading.Thread(target=self._listenThread, daemon=True)
        self.progressEventsThreads: list[threading.Thread] = []
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendingStartedEvent = Event()
        self.fileSendingEndedEvent = Event()

    def startServer(self):
        self.socket = socket.socket()
        self.listening = True
        self.socket.bind(self.address)
        self.socket.listen(5)
        self.listenThread.start()

    def setFile(self, filePath: str):
        self.filePath = filePath

    def _listenThread(self):
        while self.listenThread:
            try:
                fileSocket, address = self.socket.accept()
                fileClient = FileClient(address, fileSocket, self.filePath)

                if self.sendFile:
                    fileClient.startSending()
                else:
                    fileClient.startListening()

                self._connectEvents(fileClient)
                self.fileClients.append(fileClient)
            except ConnectionError:
                print("File server Connection Error")

    def _connectEvents(self, fileClient: FileClient):
        fileClient.fileSendStartedEvent = self.fileSendingStartedEvent
        fileClient.fileReceiveStartedEvent = self.fileReceiveStartedEvent
        fileClient.fileReceiveFinishedEvent += lambda *args, **kwargs: self.fileClients.remove(fileClient)
        fileClient.fileReceiveFinishedEvent += self.fileReceiveFinishedEvent
        fileClient.fileSendEndedEvent += lambda *args, **kwargs: self.fileClients.remove(fileClient)
        fileClient.fileSendEndedEvent += self.fileSendingEndedEvent

    def _onClientWorkerFinished(self, fileClient: FileClient, *args, **kwargs):
        self.fileClients.remove(fileClient)
        self.fileSendingStartedEvent(args, kwargs)

    def close(self):
        self.listening = False
        if self.listenThread.is_alive():
            self.listenThread.join()

        for fileClient in self.fileClients:
            fileClient.fileReceiveStartedEvent -= self.fileReceiveStartedEvent
            fileClient.fileReceiveFinishedEvent -= self.fileReceiveFinishedEvent

            fileClient.close()
        if self.socket is not None:
            self.socket.close()

    def __del__(self):
        self.close()
