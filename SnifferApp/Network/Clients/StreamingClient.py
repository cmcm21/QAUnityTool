import time

from Network.GeneralSocket import GeneralSocket
from Utils.Events import Event
from enum import Enum
import socket

BUFFER_SIZE = 32754
MSG_BUFFER_SIZE = 1024


class StreamingClient(GeneralSocket):

    def __init__(self, socket: socket.socket, address):
        super().__init__(socket, address)
        print(f"\nStreaming client {address} is connected\n")
        self.frameReceivedCompleted = Event()
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
                        self.frameReceivedCompleted(frame=b''.join(frameBytes), seconds=frameEndTime-frameStartTime)
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
        self.close()

    def close(self):
        super().close()
