import socket
import threading
from Utils.Events import Event
from Network.Clients.FileClient import FileClient
from Network.GeneralSocket import GeneralSocket


BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class FileServer(GeneralSocket):

    def __init__(self, ip: str, port: int):
        super().__init__(socket.socket(), (ip, port))
        self.filePath = None
        self.sendFile = False
        self.fileClients: list[FileClient] = []
        self.progressEventsThreads: list[threading.Thread] = []
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendingStartedEvent = Event()
        self.fileSendingEndedEvent = Event()

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socket.bind(self.address)
        self.socket.listen(5)
        print(f"File server started at {self.address}")
        self.socketThread.start()

    def setFile(self, filePath: str):
        self.filePath = filePath

    def _socketWorker(self):
        while self.listeningSocket:
            try:
                fileSocket, address = self.socket.accept()
                fileClient = FileClient(fileSocket, address, self.filePath)

                if self.sendFile:
                    fileClient.startSending()
                else:
                    fileClient.start()

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
        self.listeningSocket = False
        if self.socketThread.is_alive():
            self.socketThread.join()

        for fileClient in self.fileClients:
            fileClient.fileReceiveStartedEvent -= self.fileReceiveStartedEvent
            fileClient.fileReceiveFinishedEvent -= self.fileReceiveFinishedEvent

            fileClient.close()
        if self.socket is not None:
            self.socket.close()

    def __del__(self):
        self.close()
