import os
import socket
from Utils.Events import Event
from threading import Thread

BUFFER_SIZE = 1024
FILE_BUFFER_SIZE = 4096


class FileClient:

    def __init__(self, address, fileClientSocket: socket.socket, filepath: str):
        print("client: " + str(address) + " connected")
        self.client = fileClientSocket
        self.address = address
        self.deviceIp = address[0]
        self.listenThread = Thread(target=self._listenClientWorker, daemon=True)
        self.sendThread = Thread(target=self._sendClientWorker, daemon=True)
        self.listeningClient = False
        self.filePath = filepath
        self.fileReceiveStartedEvent = Event()
        self.fileReceiveFinishedEvent = Event()
        self.fileSendStartedEvent = Event()
        self.fileSendEndedEvent = Event()

    def startListening(self):
        self.listenThread.start()

    def startSending(self):
        self.sendThread.start()

    def _listenClientWorker(self):
        fileName = os.path.basename(self.filePath)
        try:
            send = self.client.send(fileName.encode())
            if send == b'':
                self._handleFileClientDisconnection()
                return
            fileSizeBytes = self.client.recv(BUFFER_SIZE)

            if fileSizeBytes == b'':
                self._handleFileClientDisconnection()
                return
            fileSizeBytes = fileSizeBytes.decode()
            self.fileReceiveStartedEvent(size=int(fileSizeBytes), file=fileName)
            print(f"file size: {fileSizeBytes}")

            with open(self.filePath, "wb") as f:
                while True:
                    bytesRead = self.client.recv(FILE_BUFFER_SIZE)
                    if not bytesRead or bytesRead == b'':
                        break
                    f.write(bytesRead)

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
        fileName = os.path.basename(self.filePath)
        try:
            send = self.client.send(fileName.encode())
            if send == b'':
                self._handleFileClientDisconnection()
                return

            self.fileSendStartedEvent(address=self.address, file=fileName)
            print(f"file {fileName} size {os.path.getsize(self.filePath)}")
            with open(self.filePath, "rb") as f:
                while True:
                    bytesRead = f.read(FILE_BUFFER_SIZE)
                    if not bytesRead:
                        break

                    print(f"File sending bytes {len(bytesRead)}")
                    send = self.client.sendall(bytesRead)
                    if send == b'':
                        break

        except ConnectionResetError:
            print("Error while sending file")
        except ConnectionAbortedError:
            print("Error while sending file")
        except ConnectionError:
            print("Error while sending file")
        except RuntimeError:
            print("Error while sending file")
        finally:
            self.fileSendEndedEvent(address=self.address, file=fileName)
            self.close()

    def _handleFileClientDisconnection(self):
        self.close()
        return

    def close(self):

        if self.listenThread.is_alive():
            try:
                self.listenThread.join()
            except RuntimeError:
                print("Listen thread couldn't join")

        self.client.close()
