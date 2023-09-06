import os
import socket
import time

from Utils.Events import Event
from threading import Thread
from Network.GeneralSocket import GeneralSocket

BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class FileClient(GeneralSocket):

    def __init__(self, socket: socket.socket, address):
        super().__init__(socket, address)
        print("File client: " + str(address) + f" connected")
        self.sendThread = Thread(target=self._sendClientWorker, daemon=True)
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendStartedEvent = Event()
        self.fileSendFinishedEvent = Event()
        self.fileTransferProgress = Event()
        self.fileSet = False
        self.filePath = None

    def setFile(self, filePath: str):
        self.filePath = filePath
        self.fileSet = True

    def start(self):
        self.socketThread.start()

    def startSending(self):
        self.sendThread.start()

    def _socketWorker(self):
        while not self.fileSet:
            time.sleep(0.001)

        fileName = os.path.basename(self.filePath)
        try:
            send = self.socket.send(fileName.encode())
            if send == b'':
                self._handleFileClientDisconnection()
                return
            fileSizeBytes = self.socket.recv(BUFFER_SIZE)

            if fileSizeBytes == b'':
                self._handleFileClientDisconnection()
                return
            fileSizeBytes = fileSizeBytes.decode()
            self.fileReceiveStartedEvent(size=int(fileSizeBytes), file=fileName)
            print(f"file size: {fileSizeBytes}")

            with open(self.filePath, "wb") as f:
                while True:
                    bytesRead = self.socket.recv(FILE_BUFFER_SIZE)
                    if not bytesRead or bytesRead == b'':
                        break
                    f.write(bytesRead)
                    self.fileTransferProgress(progress=len(bytesRead))

        except ConnectionResetError:
            print("Error while reading file")
        except ConnectionAbortedError:
            print("Error while reading file")
        except ConnectionError:
            print("Error while reading file")
        except RuntimeError:
            print("Error while reading file")
        finally:
            self.fileReceiveFinishedEvent(file=fileName)
            print("File received process finished")
            self.close()

    def _sendClientWorker(self):
        while not self.fileSet:
            time.sleep(0.001)

        fileName = os.path.basename(self.filePath)
        fileSize = os.path.getsize(self.filePath)
        try:
            send = self.socket.send(fileName.encode())
            if send == b'':
                self._handleFileClientDisconnection()
                return

            self.fileSendStartedEvent(size=fileSize, address=self.address, file=fileName)
            print(f"sending file {fileName} size {os.path.getsize(self.filePath)}")
            with open(self.filePath, "rb") as f:
                while True:
                    bytesRead = f.read(FILE_BUFFER_SIZE)
                    if not bytesRead:
                        break

                    send = self.socket.sendall(bytesRead)
                    if send == b'':
                        break

                    self.fileTransferProgress(progress=len(bytesRead))

        except ConnectionResetError:
            print("Error while sending file")
        except ConnectionAbortedError:
            print("Error while sending file")
        except ConnectionError:
            print("Error while sending file")
        except RuntimeError:
            print("Error while sending file")
        finally:
            self.fileSendFinishedEvent(address=self.address, file=fileName)
            self.close()

    def _handleFileClientDisconnection(self):
        self.close()
