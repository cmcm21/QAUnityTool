import numpy as np

from Network.GeneralSocket import GeneralSocket
from Network.Clients.StreamingClient import  StreamingClient
from Utils.Events import Event
from collections import deque
import socket
import cv2
import numpy
import pickle

class StreamingServer(GeneralSocket):

    def __init__(self, ip: str, port: int):
        super().__init__(socket.socket(), (ip, port))
        self.maxClients = 5
        self.clients = deque()
        self.frameReceivedCompleted = Event()
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
                self.frameNumber += 1
                streamingClient = StreamingClient(streamingSocket, address, self.frameNumber)
                self._handleClient(streamingClient)
            except ConnectionError:
                print("Error while connecting to streaming client")
            except RuntimeError:
                print("Error while connecting to streaming client")

        self.close()

    def _handleClient(self, client: StreamingClient):
        client.frameReceivedCompleted += self._onStreamClientFinished

        if len(self.clients) == 0:
            client.start()

        self.clients.append(client)
        return

    def _onStreamClientFinished(self, *args, **kwargs):
        if len(self.clients) > 0:
            streamClient: StreamingClient = self.clients.popleft()
            streamClient.start()

        self.frameReceivedCompleted(frame=kwargs['frame'])
        while self.listeningSocket:
            fileName = kwargs['fileName']
            image = cv2.imread(fileName)
            cv2.imshow("Streaming video", image)

    @staticmethod
    def imFromBytes(bytesData, flag='color'):
        """Read an image from bytes.

        Args:
            bytes (bytes): Image bytes got from files or other streams.
            flag (str): Same as :func:`imread`.

        Returns:
            ndarray: Loaded image array.
            :param flag:
            :param bytesData:
        """
        imread_flags = {
            'color': cv2.IMREAD_COLOR,
            'grayscale': cv2.IMREAD_GRAYSCALE,
            'unchanged': cv2.IMREAD_UNCHANGED
        }
        img_np = np.fromstring(bytesData, np.uint8)
        flag = imread_flags[flag] if isinstance(flag, str) else flag
        img = cv2.imdecode(img_np, flag)
        return img

    def close(self):
        super().close()

