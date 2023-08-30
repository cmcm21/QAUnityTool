import socket
import threading
from Utils.Events import Event
from Network.Clients.FileClient import FileClient
from Network.GeneralSocket import GeneralSocket
from PySide6 import QtCore


BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class FileServer(GeneralSocket, QtCore.QObject):
    qFileTransferStarSignal = QtCore.Signal(int, str)
    qFileTransferUpdateSignal = QtCore.Signal(int)
    qFileTransferEndSignal = QtCore.Signal()

    def __init__(self, ip: str, port: int):
        GeneralSocket.__init__(self, socket.socket(), (ip, port))
        QtCore.QObject.__init__(self)
        self.filePath = None
        self.sendFile = False
        self.fileClients: list[FileClient] = []
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendingStartedEvent = Event()
        self.fileSendingFinishEvent = Event()

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
        fileClient.fileReceiveStartedEvent = self.fileReceiveStartedEvent
        fileClient.fileReceiveFinishedEvent = self.fileReceiveFinishedEvent
        fileClient.fileReceiveStartedEvent += self._onFileReceivedStarted
        fileClient.fileReceiveFinishedEvent += self._onFileReceivedFinished
        fileClient.fileReceiveFinishedEvent += lambda *args, **kwargs: self.fileClients.remove(fileClient)

        fileClient.fileSendStartedEvent = self.fileSendingStartedEvent
        fileClient.fileSendFinishedEvent = self.fileSendingFinishEvent
        fileClient.fileSendStartedEvent += self._onFileSendStarted
        fileClient.fileSendFinishedEvent += self._onFileSendFinished
        fileClient.fileSendFinishedEvent += lambda *args, **kwargs: self.fileClients.remove(fileClient)

        fileClient.fileTransferProgress += self._onFileTransferProgress

    def _onFileReceivedStarted(self, *args, **kwargs):
        self.qFileTransferStarSignal.emit(kwargs['size'], f"Saving File {kwargs['file']}")

    def _onFileReceivedFinished(self, *args, **kwargs):
        self.qFileTransferEndSignal.emit()

    def _onFileSendStarted(self, *args, **kwargs):
        self.qFileTransferStarSignal.emit(kwargs['size'], f"Loading File {kwargs['file']}")

    def _onFileSendFinished(self, *args, **kwargs):
        self.qFileTransferEndSignal.emit()

    def _onFileTransferProgress(self, *args, **kwargs):
        self.qFileTransferUpdateSignal.emit(kwargs['progress'])

    def close(self):
        self.listeningSocket = False
        if self.socketThread.is_alive():
            self.socketThread.join()

        for fileClient in self.fileClients:
            fileClient.fileReceiveStartedEvent -= self._onFileReceivedStarted
            fileClient.fileReceiveFinishedEvent -= self._onFileReceivedFinished
            fileClient.fileSendStartedEvent -= self._onFileSendStarted
            fileClient.fileSendFinishedEvent -= self._onFileSendFinished
            fileClient.fileTransferProgress -= self._onFileTransferProgress

            fileClient.close()
        if self.socket is not None:
            self.socket.close()

    def __del__(self):
        self.close()
