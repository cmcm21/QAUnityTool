import time

from Network.GeneralSocket import GeneralSocket
from Utils.Events import Event
import socket

BUFFER_SIZE = 32754
MSG_BUFFER_SIZE = 1024


class StreamingClient(GeneralSocket):

    def __init__(self, ownSocket: socket.socket, address):
        super().__init__(ownSocket, address)
        print(f"\nStreaming client {address} is connected\n")
        self.frameReceivedCompleted = Event()
        self.disconnectedEvent = Event()
        self.frameNumber = 0

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socketThread.start()

    def _socketWorker(self):
        frameBytes = []
        frameBytesAmount = 0
        frameStartTime = 0
        frameEndTime = 0
        while self.listeningSocket:
            try:
                bytesRead = self.socket.recv(BUFFER_SIZE)
                if bytesRead == b'':
                    self._streamClientDisconnected()
                try:
                    bytesRead.decode()
                    if self.frameNumber > 0:
                        frameEndTime = time.time()
                        self.frameReceivedCompleted(frame=b''.join(frameBytes), id=self.id)
                        frameBytes.clear()
                        frameBytesAmount = 0
                        frameStartTime = time.time()
                    else:
                        frameStartTime = time.time()
                    self.frameNumber += 1
                except UnicodeDecodeError:
                    frameBytes.append(bytesRead)
                    frameBytesAmount += len(bytesRead)
            except ConnectionError:
                print("Error while reading frame")
            except RuntimeError:
                print("Error while reading frame")

    def _streamClientDisconnected(self):
        self.disconnectedEvent(device=self)
        self.close()

    def close(self):
        super().close()
