import socket
import threading
from Utils.Events import Event
from Utils.Settings import *
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
        self.files = {}
        self.fileClients = {}
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendingStartedEvent = Event()
        self.fileSendingFinishEvent = Event()

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socket.bind(self.address)
        self.socket.listen(MAX_DEVICES_TO_LISTENING)
        print(f"File server started at {self.address}")
        self.socketThread.start()

    def setFile(self, deviceId: str, filePath: str):
        self.files[deviceId] = filePath

    def _socketWorker(self):
        while self.listeningSocket:
            try:
                if self.socket is None:
                    break
                fileSocket, address = self.socket.accept()
                fileClient = FileClient(fileSocket, address)
                self._connectEvents(fileClient)
                fileClient.setFile(self.files[fileClient.id])

                if self.sendFile:
                    fileClient.startSending()
                else:
                    fileClient.start()

            except ConnectionError:
                print("File server Connection Error")
            except OSError as error:
                print(f"File server socket was disposed : {error}")
                break

    def _connectEvents(self, fileClient: FileClient):
        fileClient.fileReceiveStartedEvent = self.fileReceiveStartedEvent
        fileClient.fileReceiveFinishedEvent = self.fileReceiveFinishedEvent
        fileClient.fileReceiveStartedEvent += self._onFileReceivedStarted
        fileClient.fileReceiveFinishedEvent += self._onFileReceivedFinished
        fileClient.fileReceiveFinishedEvent += lambda *args, **kwargs: self.removeClient(fileClient)

        fileClient.fileSendStartedEvent = self.fileSendingStartedEvent
        fileClient.fileSendFinishedEvent = self.fileSendingFinishEvent
        fileClient.fileSendStartedEvent += self._onFileSendStarted
        fileClient.fileSendFinishedEvent += self._onFileSendFinished
        fileClient.fileSendFinishedEvent += lambda *args, **kwargs: self.removeClient(fileClient)

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

    def removeClient(self, fileClient: FileClient):
        if fileClient not in self.fileClients.keys():
            pass
        else:
            del self.fileClients[fileClient.id]

    def close(self):
        if not self.listeningSocket:
            return

        self.listeningSocket = False
        if self.socketThread.is_alive():
            self.socketThread.join(1)

        for fileClient in self.fileClients:
            fileClient.fileReceiveStartedEvent -= self._onFileReceivedStarted
            fileClient.fileReceiveFinishedEvent -= self._onFileReceivedFinished
            fileClient.fileSendStartedEvent -= self._onFileSendStarted
            fileClient.fileSendFinishedEvent -= self._onFileSendFinished
            fileClient.fileTransferProgress -= self._onFileTransferProgress

            fileClient.close()
        if self.socket is not None:
            try:
                self.socket.shutdown(socket.SHUT_RDWR)
                self.socket.close()
            except OSError:
                print("Socket is disconnected")

    def __del__(self):
        self.close()
