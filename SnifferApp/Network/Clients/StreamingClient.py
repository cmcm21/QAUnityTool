from Network.GeneralSocket import GeneralSocket
from Utils.Events import Event
import socket

BUFFER_SIZE = 32754
STREAMING_FRAMES_PATH = "RecFrames/"


class StreamingClient(GeneralSocket):

    def __init__(self, socket: socket.socket, address, frameNumber: int):
        super().__init__(socket, address)
        self.frameReceivedCompleted = Event()
        self.frameNumber = frameNumber

    def start(self):
        if self.listeningSocket:
            return

        self.listeningSocket = True
        self.socketThread.start()

    def _socketWorker(self):
        frameBytes = []
        try:
            while True:
                bytesRead = self.socket.recv(BUFFER_SIZE)
                if not bytesRead or bytesRead == b'':
                    break
                frameBytes.append(bytesRead)

        except ConnectionResetError:
            print("Error while reading frame")
        except ConnectionAbortedError:
            print("Error while reading frame")
        except ConnectionError:
            print("Error while reading frame")
        except RuntimeError:
            print("Error while reading frame")
        finally:
            #print("Frame received process finished")
            self.frameReceivedCompleted(frame=b''.join(frameBytes))
            self.close()

    def close(self):
        return
