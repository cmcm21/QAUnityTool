from Network.GeneralSocket import GeneralSocket
from Network.Clients.StreamingClient import StreamingClient
from Utils.Events import Event
import socket
from PySide6 import QtCore
from PySide6.QtGui import QPixmap
from Utils.StreamingVideoHelper import StreamingVideoHelper
from Utils.Settings import *


class StreamingServer(GeneralSocket, QtCore.QObject):
    qSignal = QtCore.Signal(QPixmap, str)

    def __init__(self, ip: str, port: int):
        GeneralSocket.__init__(self, socket.socket(), (ip, port))
        QtCore.QObject.__init__(self)
        self.maxClients = 5
        self.clients: list[StreamingClient] = []
        self.frameReceivedCompleted = Event()
        self.onNewStreamingClientConnected = Event()
        self.helpers: {str: StreamingVideoHelper} = {}
        self.frameNumber = 0

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socket.bind(self.address)
        self.socket.listen(MAX_DEVICES)
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
        client.frameReceivedCompleted += self._onFrameCompleted
        helper = StreamingVideoHelper()
        self.helpers[client.id] = helper
        self.clients.append(client)
        client.start()
        self.onNewStreamingClientConnected(client=client, helper=helper)

    def _onFrameCompleted(self, *args, **kwargs):
        clientId = kwargs['id']
        frame = kwargs['frame']
        if self._processQSignal(frame, clientId):
            self.helpers[clientId].addFrame(frame)
            self.frameReceivedCompleted(frame=frame, id=clientId)

    def _processQSignal(self, frame: bytes, clientId: str) -> bool:
        pixmap = QPixmap()
        loaded = pixmap.loadFromData(frame, format=".jpg")
        if loaded:
            self.qSignal.emit(pixmap, clientId)
            return True
        else:
            print("Error trying to load frame from bytes")
            return False

    def onRecordStarted(self):
        return

    def close(self):
        super().close()
        for client in self.clients:
            client.close()
