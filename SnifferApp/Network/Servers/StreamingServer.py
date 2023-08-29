from Network.GeneralSocket import GeneralSocket
from Network.Clients.StreamingClient import StreamingClient
from Utils.Events import Event
import socket
from PySide6 import QtCore
from PySide6.QtGui import QPixmap
from Utils.StreamingVideoHelper import StreamingVideoHelper


class StreamingServer(GeneralSocket, QtCore.QObject):
    qSignal = QtCore.Signal(QPixmap)

    def __init__(self, ip: str, port: int):
        GeneralSocket.__init__(self, socket.socket(), (ip, port))
        QtCore.QObject.__init__(self)
        self.maxClients = 5
        self.clients: list[StreamingClient] = []
        self.frameReceivedCompleted = Event()
        self.streamingHelper = StreamingVideoHelper()
        self.frameNumber = 0

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socket.bind(self.address)
        self.socket.listen(self.maxClients)
        print(f"Streaming Server Started at {self.address}")
        self.socketThread.start()

    def _socketWorker(self):
        while self.listeningSocket:
            try:
                streamingSocket, address = self.socket.accept()
                streamingClient = StreamingClient(streamingSocket, address)
                self._handleClient(streamingClient)
            except ConnectionError:
                print("Error while connecting to streaming client")
            except RuntimeError:
                print("Error while connecting to streaming client")

        self.close()

    def _handleClient(self, client: StreamingClient):
        client.frameReceivedCompleted += self._onStreamClientFinished
        client.start()
        self.clients.append(client)

    def _onStreamClientFinished(self, *args, **kwargs):
        self.streamingHelper.addFrame(kwargs['frame'])
        self.frameReceivedCompleted(frame=kwargs['frame'])
        self._processQSignal(frame=kwargs['frame'])

    def _processQSignal(self, frame: bytes):
        pixmap = QPixmap()
        loaded = pixmap.loadFromData(frame, format=".bmp")
        if loaded:
            self.qSignal.emit(pixmap)
        else:
            print("Error trying to load frame from bytes")

    def saveStreamAsVideo(self, videoPath: str):
        self.streamingHelper.saveFramesToVideo(videoPath)

    def resetFramesRecorded(self):
        self.streamingHelper.restart()

    def close(self):
        super().close()
        for client in self.clients:
            client.close()

